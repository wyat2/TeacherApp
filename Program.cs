using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;
using TeacherApp.Models;
using TeacherApp.Services;
using Google.Cloud.SecretManager.V1;

var builder = WebApplication.CreateBuilder(args);

// Load secrets from Google Secret Manager if in Production
if (builder.Environment.IsProduction())
{
    var projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT") ?? "fab-penguin";
    var client = SecretManagerServiceClient.Create();
    
    try
    {
        var emailPasswordSecret = client.AccessSecretVersion($"projects/{projectId}/secrets/email-password/versions/latest");
        var emailUsernameSecret = client.AccessSecretVersion($"projects/{projectId}/secrets/email-username/versions/latest");
        var emailSmtpServerSecret = client.AccessSecretVersion($"projects/{projectId}/secrets/email-smtp-server/versions/latest");
        
        builder.Configuration["EmailSettings:Password"] = emailPasswordSecret.Payload.Data.ToStringUtf8();
        builder.Configuration["EmailSettings:Username"] = emailUsernameSecret.Payload.Data.ToStringUtf8();
        builder.Configuration["EmailSettings:SmtpServer"] = emailSmtpServerSecret.Payload.Data.ToStringUtf8();
        builder.Configuration["EmailSettings:SenderEmail"] = emailUsernameSecret.Payload.Data.ToStringUtf8();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not load secrets from Secret Manager: {ex.Message}");
    }
}

// Check for command-line arguments
if (args.Contains("--populate-test-data"))
{
    // Run test data population and exit
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
    
    using (var context = new ApplicationDbContext(optionsBuilder.Options))
    {
        await TeacherApp.PopulateTestData.PopulateAsync(context);
    }
    return;
}

if (args.Contains("--populate-case-numbers"))
{
    // Run case number population and exit
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
    
    using (var context = new ApplicationDbContext(optionsBuilder.Options))
    {
        await TeacherApp.PopulateCaseNumbers.Run(context);
    }
    return;
}

if (args.Contains("--update-academic-standing"))
{
    // Run academic standing update and exit
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
    
    using (var context = new ApplicationDbContext(optionsBuilder.Options))
    {
        await TeacherApp.UpdateAcademicStanding.Run(context);
    }
    return;
}

if (args.Contains("--update-names-uppercase"))
{
    // Run names uppercase update and exit
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
    
    using (var context = new ApplicationDbContext(optionsBuilder.Options))
    {
        await TeacherApp.UpdateNamesToUppercase.Run(context);
    }
    return;
}

if (args.Contains("--update-dropdown-values"))
{
    // Run dropdown values update and exit
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
    
    using (var context = new ApplicationDbContext(optionsBuilder.Options))
    {
        await TeacherApp.UpdateDropdownValues.UpdateExistingStudentData(context);
    }
    return;
}

// Configure Npgsql to use timestamp without time zone for DateTime
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Strong password policy for FERPA compliance
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;
    
    // Account lockout to prevent brute force attacks
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISetupWizardService, SetupWizardService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enforce HTTPS for FERPA compliance
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Setup wizard middleware - redirect to setup if first run
app.Use(async (context, next) =>
{
    var setupWizard = context.RequestServices.GetRequiredService<ISetupWizardService>();
    var isFirstRun = await setupWizard.IsFirstRunAsync();
    
    if (isFirstRun && !context.Request.Path.StartsWithSegments("/Setup") && !context.Request.Path.StartsWithSegments("/Account/Login"))
    {
        context.Response.Redirect("/Setup");
        return;
    }
    
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure DB is created and Admin role exists (setup wizard will create admin user)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    if (!roleManager.RoleExistsAsync("Admin").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
    }

    if (!roleManager.RoleExistsAsync("Director").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Director")).Wait();
    }

    // Ensure Director role has all permissions
    var directorPermission = context.RolePermissions.FirstOrDefault(rp => rp.RoleName == "Director");
    if (directorPermission == null)
    {
        directorPermission = new RolePermission
        {
            RoleName = "Director",
            CanDeleteStudents = true,
            CanManageAdmins = true,
            CanDeleteAdmins = true,
            CanDeleteDirectors = true,
            CanDeleteTutors = true,
            CanEditAdmins = true,
            CanEditDirectors = true,
            CanEditTutors = true
        };
        context.RolePermissions.Add(directorPermission);
        context.SaveChanges();
    }

    if (!roleManager.RoleExistsAsync("Student").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Student")).Wait();
    }

    if (!roleManager.RoleExistsAsync("Tutor").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Tutor")).Wait();
    }

        // Optional seeding when SEED_USERS environment variable is set to "true"
        var seedUsers = Environment.GetEnvironmentVariable("SEED_USERS");
        if (!string.IsNullOrEmpty(seedUsers) && seedUsers.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            
            // Create specific Director user
            var directorEmail = "Director@gmail.com";
            if (userManager.FindByEmailAsync(directorEmail).Result == null)
            {
                var directorUser = new ApplicationUser 
                { 
                    UserName = directorEmail, 
                    Email = directorEmail, 
                    FirstName = "Director", 
                    LastName = "User", 
                    PhoneNumber = "555-1000" 
                };
                var directorResult = userManager.CreateAsync(directorUser, "Password123!").Result;
                if (directorResult.Succeeded)
                {
                    userManager.AddToRoleAsync(directorUser, "Director").Wait();
                }
            }
            
            var names = new[] { "Olivia", "Liam", "Emma", "Noah", "Ava", "Oliver", "Sophia", "Elijah", "Isabella", "Lucas", "Mia", "Mason", "Amelia", "Logan", "James", "Ethan", "Harper", "Benjamin", "Evelyn", "Alexander" };
            var lasts = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
            var rnd = new Random();
            string[] rolesToSeed = new[] { "Tutor", "Student", "Admin" };
            foreach (var role in rolesToSeed)
            {
                for (int i = 1; i <= 40; i++)
                {
                    var f = names[rnd.Next(names.Length)];
                    var l = lasts[rnd.Next(lasts.Length)];
                    var email = $"{f}.{l}.{role}.{i}@example.com".ToLower();
                    if (userManager.FindByEmailAsync(email).Result != null) continue;
                    var user = new ApplicationUser { UserName = email, Email = email, FirstName = f, LastName = l, PhoneNumber = $"555-1{rnd.Next(100,999)}" };
                    var pw = "Password123!";
                    var res = userManager.CreateAsync(user, pw).Result;
                    if (res.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }
        }

    // Optional seeding of Student records when SEED_STUDENTS environment variable is set to "true"
    var seedStudents = Environment.GetEnvironmentVariable("SEED_STUDENTS");
    if (!string.IsNullOrEmpty(seedStudents) && seedStudents.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var names = new[] { "Olivia", "Liam", "Emma", "Noah", "Ava", "Oliver", "Sophia", "Elijah", "Isabella", "Lucas", "Mia", "Mason", "Amelia", "Logan", "James", "Ethan", "Harper", "Benjamin", "Evelyn", "Alexander" };
        var lasts = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
        var classes = new[] { "1A", "1B", "2A", "2B", "3A", "3B", "4A", "4B", "5A", "5B" };
        var ethnicities = new[] { "Hispanic", "Asian", "African American", "Caucasian", "Native American", "Pacific Islander", "Multiracial" };
        var rnd = new Random();
        var adminUser = userManager.FindByEmailAsync("admin@example.com").Result;
        string createdByUserId = adminUser?.Id ?? "system";
        
        for (int i = 1; i <= 40; i++)
        {
            var f = names[rnd.Next(names.Length)];
            var l = lasts[rnd.Next(lasts.Length)];
            var email = $"{f}.{l}.student.{i}@example.com".ToLower();
            if (dbContext.Students.Any(s => s.Email == email)) continue;
            
            var student = new Student
            {
                FirstName = f,
                LastName = l,
                Email = email,
                Ethnicity = ethnicities[rnd.Next(ethnicities.Length)],
                CreatedByUserId = createdByUserId
            };
            dbContext.Students.Add(student);
        }
        dbContext.SaveChanges();
    }
}

app.Run();

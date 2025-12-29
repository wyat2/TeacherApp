using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;
using TeacherApp.Models;
using TeacherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Check for command-line arguments
if (args.Contains("--populate-case-numbers"))
{
    // Run case number population and exit
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=teacherapp.db");
    
    using (var context = new ApplicationDbContext(optionsBuilder.Options))
    {
        await TeacherApp.PopulateCaseNumbers.Run(context);
    }
    return;
}

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=teacherapp.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ISetupWizardService, SetupWizardService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
        var grades = new[] { "A", "B", "C", "D", "F" };
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
                ClassName = classes[rnd.Next(classes.Length)],
                Grade = grades[rnd.Next(grades.Length)],
                Ethnicity = ethnicities[rnd.Next(ethnicities.Length)],
                CreatedByUserId = createdByUserId
            };
            dbContext.Students.Add(student);
        }
        dbContext.SaveChanges();
    }
}

app.Run();

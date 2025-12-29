using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectorsPageInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectorsPageInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SchoolName = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    SchoolPhone = table.Column<string>(type: "TEXT", nullable: true),
                    DirectorName = table.Column<string>(type: "TEXT", nullable: true),
                    DirectorEmail = table.Column<string>(type: "TEXT", nullable: true),
                    DirectorPhone = table.Column<string>(type: "TEXT", nullable: true),
                    GrantType = table.Column<string>(type: "TEXT", nullable: true),
                    GrantId = table.Column<string>(type: "TEXT", nullable: true),
                    GrantBudget = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectorsPageInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectorsPageInfos");
        }
    }
}

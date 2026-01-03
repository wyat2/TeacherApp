using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectorRoleCheckboxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDataEntryPerson",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProjectDirector",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDataEntryPerson",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsProjectDirector",
                table: "AspNetUsers");
        }
    }
}

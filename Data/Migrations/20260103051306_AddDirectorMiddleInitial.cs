using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectorMiddleInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MiddleInitial",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MiddleInitial",
                table: "AspNetUsers");
        }
    }
}

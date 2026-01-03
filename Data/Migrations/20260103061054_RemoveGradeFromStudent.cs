using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGradeFromStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

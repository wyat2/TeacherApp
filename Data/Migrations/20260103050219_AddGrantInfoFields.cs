using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrantInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GPAScale",
                table: "DirectorsPageInfos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameOfGranteeInstitution",
                table: "DirectorsPageInfos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectType",
                table: "DirectorsPageInfos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfInstitution",
                table: "DirectorsPageInfos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GPAScale",
                table: "DirectorsPageInfos");

            migrationBuilder.DropColumn(
                name: "NameOfGranteeInstitution",
                table: "DirectorsPageInfos");

            migrationBuilder.DropColumn(
                name: "ProjectType",
                table: "DirectorsPageInfos");

            migrationBuilder.DropColumn(
                name: "TypeOfInstitution",
                table: "DirectorsPageInfos");
        }
    }
}

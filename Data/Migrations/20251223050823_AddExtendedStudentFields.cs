using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedStudentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicNeed",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcademicStanding",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountOfFinancialAidReceived",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountOfSSSGrantAidAwarded",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeGradeLevelAtBeginningOfAcademicYear",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeGradeLevelAtEndOfAcademicYear",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeGradeLevelEntryIntoProject",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CumulativeGPA",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfFirstProjectService",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastProjectService",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfUndergraduateDegreeCertificate",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Eligibility",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentStatusAtEndOfAcademicYear",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentStatusInAcademicYearFirstServed",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldOfStudyDegreeCertificateEarned",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstEnrollmentDate",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantNameChange",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantStatusDuringAcademicYearBeingReported",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersistenceStatusAtBeginningOfAcademicYear",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonForWithdrawalOrNotReturning",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentCohortYear",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentDisability",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transfers",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UndergraduateDegreeCertificateCompletedAtGranteeInstitution",
                table: "Students",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicNeed",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicStanding",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AmountOfFinancialAidReceived",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AmountOfSSSGrantAidAwarded",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CollegeGradeLevelAtBeginningOfAcademicYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CollegeGradeLevelAtEndOfAcademicYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CollegeGradeLevelEntryIntoProject",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CumulativeGPA",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfFirstProjectService",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfLastProjectService",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfUndergraduateDegreeCertificate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Eligibility",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrollmentStatusAtEndOfAcademicYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrollmentStatusInAcademicYearFirstServed",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FieldOfStudyDegreeCertificateEarned",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FirstEnrollmentDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParticipantNameChange",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParticipantStatusDuringAcademicYearBeingReported",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PersistenceStatusAtBeginningOfAcademicYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ReasonForWithdrawalOrNotReturning",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentCohortYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentDisability",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Transfers",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UndergraduateDegreeCertificateCompletedAtGranteeInstitution",
                table: "Students");
        }
    }
}

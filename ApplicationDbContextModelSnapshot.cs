using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee_Verification_System.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminID);
                });

            migrationBuilder.CreateTable(
                name: "FraudDetections",
                columns: table => new
                {
                    DetectionID = table.Column<string>(type: "TEXT", nullable: false),
                    EmployeeID = table.Column<string>(type: "TEXT", nullable: false),
                    SuspicionReason = table.Column<string>(type: "TEXT", nullable: false),
                    DetectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActionTaken = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudDetections", x => x.DetectionID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminID = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReportData = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_Reports_Admins_AdminID",
                        column: x => x.AdminID,
                        principalTable: "Admins",
                        principalColumn: "AdminID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Verifications",
                columns: table => new
                {
                    VerificationID = table.Column<string>(type: "TEXT", nullable: false),
                    EmployeeEmail = table.Column<string>(type: "TEXT", nullable: false),
                    VerificationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AdminID = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verifications", x => x.VerificationID);
                    table.ForeignKey(
                        name: "FK_Verifications_Admins_AdminID",
                        column: x => x.AdminID,
                        principalTable: "Admins",
                        principalColumn: "AdminID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "AdminID", "Department", "Email", "FirstName", "LastName", "MiddleName", "Role" },
                values: new object[] { 1, "IT", "admin@company.com", "System", "Administrator", null, "Super Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AdminID",
                table: "Reports",
                column: "AdminID");

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_AdminID",
                table: "Verifications",
                column: "AdminID");

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_EmployeeEmail",
                table: "Verifications",
                column: "EmployeeEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_Status",
                table: "Verifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_VerificationDate",
                table: "Verifications",
                column: "VerificationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FraudDetections");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Verifications");

            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}

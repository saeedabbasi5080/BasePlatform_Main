using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForgotPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCodes_UserId_ConsumedAt",
                table: "EmailVerificationCodes");

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "EmailVerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCodes_UserId_Purpose_ConsumedAt",
                table: "EmailVerificationCodes",
                columns: new[] { "UserId", "Purpose", "ConsumedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCodes_UserId_Purpose_ConsumedAt",
                table: "EmailVerificationCodes");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "EmailVerificationCodes");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCodes_UserId_ConsumedAt",
                table: "EmailVerificationCodes",
                columns: new[] { "UserId", "ConsumedAt" });
        }
    }
}

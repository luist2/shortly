using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortly_API.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokensInSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sessions are intentionally invalidated in this migration.
            // Existing values are plaintext tokens and cannot be safely transformed
            // without storing token material in migration code.
            migrationBuilder.Sql("""DELETE FROM "UserSessions";""");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "UserSessions",
                newName: "RefreshTokenHash");

            migrationBuilder.RenameIndex(
                name: "UX_UserSessions_RefreshToken",
                table: "UserSessions",
                newName: "UX_UserSessions_RefreshTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenHash",
                table: "UserSessions",
                newName: "RefreshToken");

            migrationBuilder.RenameIndex(
                name: "UX_UserSessions_RefreshTokenHash",
                table: "UserSessions",
                newName: "UX_UserSessions_RefreshToken");
        }
    }
}

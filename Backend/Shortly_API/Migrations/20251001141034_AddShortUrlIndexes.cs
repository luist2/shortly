using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortly_API.Migrations
{
    /// <inheritdoc />
    public partial class AddShortUrlIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_IsActive_ShortCode",
                table: "ShortUrls",
                columns: new[] { "IsActive", "ShortCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_IsActive_ShortCode",
                table: "ShortUrls");
        }
    }
}

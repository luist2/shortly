using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortly_API.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsDeletedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isDeleted",
                table: "ShortUrls",
                newName: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "ShortUrls",
                newName: "isDeleted");
        }
    }
}

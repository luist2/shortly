using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortly_API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToShortUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ShortUrls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "ShortUrls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ShortUrls");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "ShortUrls");
        }
    }
}

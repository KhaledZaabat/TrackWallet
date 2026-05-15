using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker.Infrastructure.Migrations
{
    /// <summary>
    /// Adds the SHA-256 <c>ContentHash</c> column to <c>UploadedFiles</c> and a
    /// covering index for dedup lookups.
    /// </summary>
    public partial class UploadedFileContentHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "UploadedFiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_ContentHash",
                table: "UploadedFiles",
                column: "ContentHash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_ContentHash",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "UploadedFiles");
        }
    }
}

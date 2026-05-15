using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker.Infrastructure.Migrations
{
    /// <summary>
    /// Replaces the case-sensitive unique index on <c>Users.UserName</c> with a unique
    /// index on a new <c>NormalizedUserName</c> column (upper-invariant of UserName).
    /// </summary>
    /// <remarks>
    /// Migration order is deliberate: the column is added <em>nullable</em>, backfilled
    /// from the existing <c>UserName</c> via <c>UPPER()</c>, then promoted to
    /// <c>NOT NULL</c> before the unique index is created. Adding a non-null column with
    /// a literal default and then creating a unique index in one shot would violate the
    /// uniqueness constraint on any table that already holds more than one row.
    /// </remarks>
    public partial class UserNormalizedUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old case-sensitive unique index first; the column it covers
            // stays in place for the rest of the migration.
            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            // Add the column as nullable so existing rows survive without a placeholder
            // that would later collide with the unique index.
            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Backfill from the existing data. UPPER() with the default Postgres
            // collation matches what User.Normalize(...) produces for ASCII usernames,
            // which is the only charset the validator allows.
            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""NormalizedUserName"" = UPPER(""UserName"");");

            // Now that every row has a value, lock the column down to NOT NULL.
            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            // Finally create the unique index that the SPA's "is this name available?"
            // check will hit. Index seek instead of seq-scan + LOWER() per row.
            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUserName",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedUserName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }
    }
}

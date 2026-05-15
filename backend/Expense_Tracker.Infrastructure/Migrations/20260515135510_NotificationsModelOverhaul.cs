using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker.Infrastructure.Migrations
{
    /// <summary>
    /// Reshapes the <c>Notifications</c> table to the typed-payload model
    /// (Title/Body/Severity/Category/IconKey/ResourceUri/Payload).
    /// </summary>
    /// <remarks>
    /// The pre-refactor <c>Data</c> column held a free-form
    /// <c>Dictionary&lt;string,string&gt;</c>. Those rows cannot be losslessly
    /// translated to the new strongly-typed <c>NotificationPayload</c> shape,
    /// so we truncate the table on UP. Notifications are ephemeral — losing
    /// them on schema change is acceptable and far safer than leaving the
    /// table in a state where reads fail mid-deserialisation.
    /// </remarks>
    public partial class NotificationsModelOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean cutover. Anything created before this migration cannot fit
            // the new payload contract, so we drop everything before reshaping
            // the columns. CASCADE keeps any future FK referers consistent.
            migrationBuilder.Sql(@"TRUNCATE TABLE ""Notifications"" CASCADE;");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Data_GIN",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_Type_CreatedAtUtc",
                table: "Notifications");

            // Old loose Data column → typed Payload column.
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Notifications",
                newName: "Payload");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAtUtc",
                table: "Notifications",
                newName: "IX_Notifications_UserId_Unread");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            // New required presentation columns. Defaults exist only to satisfy
            // the NOT NULL constraint at column-add time; the table is empty
            // (TRUNCATE above) so nothing actually carries the empty default.
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Notifications",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "Notifications",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Info");

            migrationBuilder.AddColumn<string>(
                name: "IconKey",
                table: "Notifications",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<string>(
                name: "ResourceUri",
                table: "Notifications",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadAtUtc",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            // Hot-path index — list newest-first per user.
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAtUtc" },
                descending: new[] { false, true });

            // Filter-by-category for the SPA's tabs.
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Category_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "UserId", "Category", "CreatedAtUtc" });

            // Plain ActorUserId index (no partial filter — the column already
            // accepts NULLs and Postgres skips NULLs in btree by default).
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications",
                column: "ActorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_Category_CreatedAtUtc",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAtUtc",
                table: "Notifications");

            migrationBuilder.DropColumn(name: "Category", table: "Notifications");
            migrationBuilder.DropColumn(name: "IconKey", table: "Notifications");
            migrationBuilder.DropColumn(name: "ReadAtUtc", table: "Notifications");
            migrationBuilder.DropColumn(name: "ResourceUri", table: "Notifications");
            migrationBuilder.DropColumn(name: "Severity", table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "Notifications",
                newName: "Data");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId_Unread",
                table: "Notifications",
                newName: "IX_Notifications_UserId_IsRead_CreatedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications",
                column: "ActorUserId",
                filter: "\"ActorUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Data_GIN",
                table: "Notifications",
                column: "Data")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Type_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "UserId", "Type", "CreatedAtUtc" });
        }
    }
}

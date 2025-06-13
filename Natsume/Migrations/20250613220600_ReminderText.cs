using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class ReminderText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReminderText",
                table: "Reminders",
                type: "TEXT",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderText",
                table: "Reminders");
        }
    }
}

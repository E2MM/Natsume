using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class RemindedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "DiscordUserId",
                table: "Reminders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordUserId",
                table: "Reminders");
        }
    }
}

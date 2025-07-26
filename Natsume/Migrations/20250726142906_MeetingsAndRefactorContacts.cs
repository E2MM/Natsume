using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class MeetingsAndRefactorContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityFriendship",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MessageFriendship",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "Contacts",
                newName: "DiscordNickname");

            migrationBuilder.RenameColumn(
                name: "AvailableFavor",
                table: "Contacts",
                newName: "CurrentFavor");

            migrationBuilder.RenameColumn(
                name: "MessageCount",
                table: "Contacts",
                newName: "TotalInteractions");

            migrationBuilder.RenameColumn(
                name: "LastMessageOn",
                table: "Contacts",
                newName: "LastInteraction");

            migrationBuilder.RenameColumn(
                name: "FriendsSince",
                table: "Contacts",
                newName: "MetOn");

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeetingName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    IsRandomMeeting = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.RenameColumn(
                name: "TotalInteractions",
                table: "Contacts",
                newName: "MessageCount");

            migrationBuilder.RenameColumn(
                name: "MetOn",
                table: "Contacts",
                newName: "FriendsSince");

            migrationBuilder.RenameColumn(
                name: "LastInteraction",
                table: "Contacts",
                newName: "LastMessageOn");

            migrationBuilder.RenameColumn(
                name: "DiscordNickname",
                table: "Contacts",
                newName: "Nickname");

            migrationBuilder.RenameColumn(
                name: "CurrentFavor",
                table: "Contacts",
                newName: "AvailableFavor");

            migrationBuilder.AddColumn<decimal>(
                name: "ActivityFriendship",
                table: "Contacts",
                type: "TEXT",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MessageFriendship",
                table: "Contacts",
                type: "TEXT",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }
    }
}

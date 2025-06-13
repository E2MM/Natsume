using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class Renaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalFriendshipExpended",
                table: "Contacts",
                newName: "TotalFavorExpended");

            migrationBuilder.RenameColumn(
                name: "IsNatsumeFriend",
                table: "Contacts",
                newName: "IsFriend");

            migrationBuilder.RenameColumn(
                name: "CurrentFriendship",
                table: "Contacts",
                newName: "AvailableFavor");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Contacts",
                newName: "DiscordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalFavorExpended",
                table: "Contacts",
                newName: "TotalFriendshipExpended");

            migrationBuilder.RenameColumn(
                name: "IsFriend",
                table: "Contacts",
                newName: "IsNatsumeFriend");

            migrationBuilder.RenameColumn(
                name: "AvailableFavor",
                table: "Contacts",
                newName: "CurrentFriendship");

            migrationBuilder.RenameColumn(
                name: "DiscordId",
                table: "Contacts",
                newName: "Id");
        }
    }
}

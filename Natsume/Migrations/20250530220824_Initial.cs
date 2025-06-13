using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsNatsumeFriend = table.Column<bool>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CurrentFriendship = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    TimeFriendship = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    ActivityFriendship = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MessageFriendship = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    TotalFriendshipExpended = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    LastMessageOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MessageCount = table.Column<ulong>(type: "INTEGER", nullable: false),
                    FriendsSince = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts");
        }
    }
}

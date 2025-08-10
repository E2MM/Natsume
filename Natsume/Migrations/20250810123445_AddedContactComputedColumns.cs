using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class AddedContactComputedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MessageFriendship",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "(LOG(1 + [TotalInteractions]) * LOG(1 + [TotalInteractions]) / 100.0)",
                stored: false);
            
            migrationBuilder.AddColumn<decimal>(
                name: "MaximumFavor",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "(1 + [TimeFriendship] + [MessageFriendship])",
                stored: false);
            
            migrationBuilder.AddColumn<decimal>(
                name: "Friendship",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "(100 * [TotalFavorExpended] * (1 + [TimeFriendship] + [MessageFriendship]))",
                stored: false);
            
            migrationBuilder.AddColumn<decimal>(
                name: "DailyAverageFavorExpended",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "CASE WHEN (JULIANDAY(COALESCE([LastInteraction], [MetOn])) - JULIANDAY([MetOn])) > 0 \nTHEN [TotalFavorExpended] / (JULIANDAY([LastInteraction]) - JULIANDAY([MetOn])) \nELSE 0 END",
                stored: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyAverageFavorExpended",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Friendship",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MaximumFavor",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MessageFriendship",
                table: "Contacts");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class ChangedLog10toLn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MessageFriendship",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "(LN(1 + [TotalInteractions]) * LN(1 + [TotalInteractions]) / 100.0)",
                stored: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldComputedColumnSql: "(LOG(1 + [TotalInteractions]) * LOG(1 + [TotalInteractions]) / 100.0)",
                oldStored: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MessageFriendship",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "(LOG(1 + [TotalInteractions]) * LOG(1 + [TotalInteractions]) / 100.0)",
                stored: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldComputedColumnSql: "(LN(1 + [TotalInteractions]) * LN(1 + [TotalInteractions]) / 100.0)",
                oldStored: false);
        }
    }
}

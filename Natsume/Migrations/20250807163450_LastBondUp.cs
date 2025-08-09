using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Natsume.Migrations
{
    /// <inheritdoc />
    public partial class LastBondUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastBondUp",
                table: "Contacts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastBondUp",
                table: "Contacts");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EITSTMS.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketNumberCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketNumberCounters",
                columns: table => new
                {
                    CounterDate = table.Column<DateTime>(type: "date", nullable: false),
                    CurrentNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketNumberCounters", x => x.CounterDate);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketNumberCounters");
        }
    }
}

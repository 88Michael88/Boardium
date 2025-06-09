using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boardium.Migrations
{
    /// <inheritdoc />
    public partial class ADD_Remove_GameAvailableCopy_from_the_context : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameAvailableCopies",
                columns: table => new
                {
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GameCopyID = table.Column<int>(type: "int", nullable: false),
                    GameID = table.Column<int>(type: "int", nullable: false),
                    InventoryNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RentalFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}

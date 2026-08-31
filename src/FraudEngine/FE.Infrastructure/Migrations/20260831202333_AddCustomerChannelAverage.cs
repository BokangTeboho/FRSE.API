using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerChannelAverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageTransactionAmount",
                table: "Customers");

            migrationBuilder.CreateTable(
                name: "CustomerChannelAverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentChannel = table.Column<string>(type: "text", nullable: false),
                    AverageAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerChannelAverages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChannelAverages_CustomerId_PaymentChannel",
                table: "CustomerChannelAverages",
                columns: new[] { "CustomerId", "PaymentChannel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerChannelAverages");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageTransactionAmount",
                table: "Customers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}

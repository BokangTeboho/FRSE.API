using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentChannelAndMerchantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_CustomerId_CreatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BeneficiaryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryAccountNumber",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountNumber_CreatedAt",
                table: "Transactions",
                columns: new[] { "AccountNumber", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccountNumber",
                table: "Customers",
                column: "AccountNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountNumber_CreatedAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AccountNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BeneficiaryAccountNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryId",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerId_CreatedAt",
                table: "Transactions",
                columns: new[] { "CustomerId", "CreatedAt" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceIdToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceId",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReferenceId_AccountNumber",
                table: "Transactions",
                columns: new[] { "ReferenceId", "AccountNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReferenceId_AccountNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "Transactions");
        }
    }
}

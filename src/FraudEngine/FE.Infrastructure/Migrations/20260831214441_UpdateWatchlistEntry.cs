using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWatchlistEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddedByIdentifier",
                table: "WatchlistEntries",
                newName: "ModifiedByIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedByIdentifier",
                table: "WatchlistEntries",
                newName: "AddedByIdentifier");
        }
    }
}

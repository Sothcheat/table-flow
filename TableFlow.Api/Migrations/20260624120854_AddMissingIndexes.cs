using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_ClosedAt",
                table: "TableSessions",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TableSessions_ClosedAt",
                table: "TableSessions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders");
        }
    }
}

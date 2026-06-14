using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderSessionForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_TableSessions_TableSessionId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TableSessionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableSessionId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_TableSessions_SessionId",
                table: "Orders",
                column: "SessionId",
                principalTable: "TableSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_TableSessions_SessionId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "TableSessionId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableSessionId",
                table: "Orders",
                column: "TableSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_TableSessions_TableSessionId",
                table: "Orders",
                column: "TableSessionId",
                principalTable: "TableSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

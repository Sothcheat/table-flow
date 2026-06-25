using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAmountReceivedToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountReceived",
                table: "TableSessions",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountReceived",
                table: "TableSessions");
        }
    }
}

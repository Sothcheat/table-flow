using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStationRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StationName",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE ""Orders"" SET ""StationName"" = 'Kitchen' WHERE ""StationName"" = '';");

            migrationBuilder.AddColumn<string>(
                name: "StationName",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE ""Categories"" SET ""StationName"" = 'Kitchen' WHERE ""StationName"" = '';"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StationName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StationName",
                table: "Categories");
        }
    }
}

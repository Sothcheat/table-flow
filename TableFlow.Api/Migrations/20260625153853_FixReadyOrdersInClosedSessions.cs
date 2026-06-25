using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixReadyOrdersInClosedSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OrderStatus: Ready=3, Served=4 | SessionStatus: Closed=1
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""OrderStatus"" = 4
                WHERE ""OrderStatus"" = 3
                AND ""SessionId"" IN (
                    SELECT ""Id"" FROM ""TableSessions"" WHERE ""SessionStatus"" = 1
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

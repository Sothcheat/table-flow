using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOpenSessionPerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX "IX_TableSessions_TableId_Open_Unique"
                ON "TableSessions" ("TableId")
                WHERE "SessionStatus" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """DROP INDEX "IX_TableSessions_TableId_Open_Unique";""");
        }
    }
}

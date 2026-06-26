using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePublicToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicToken",
                table: "Tables",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Backfill existing rows with distinct tokens before the unique index is created,
            // otherwise every pre-existing table would share Guid.Empty and violate uniqueness.
            migrationBuilder.Sql(@"UPDATE ""Tables"" SET ""PublicToken"" = gen_random_uuid();");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_PublicToken",
                table: "Tables",
                column: "PublicToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_PublicToken",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PublicToken",
                table: "Tables");
        }
    }
}

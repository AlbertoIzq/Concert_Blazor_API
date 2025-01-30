using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concert.DataAccess.API.Migrations.ConcertAuthDb
{
    /// <inheritdoc />
    public partial class SeedTablesDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5fb21249-08bc-4585-ae71-48392889955f",
                column: "ConcurrencyStamp",
                value: "186c8464-85f9-4bbe-b86c-750bc2f4494d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9fc5f185-c6c3-4bcd-90c0-74e35304d69c",
                column: "ConcurrencyStamp",
                value: "1ca67214-8307-43fb-8a79-07b3e96e08d1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f3a2308a-3774-4882-8cab-e1b52ce0b48a",
                column: "ConcurrencyStamp",
                value: "636df8f9-2a8b-420c-aa70-0744ea4b4b69");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5fb21249-08bc-4585-ae71-48392889955f",
                column: "ConcurrencyStamp",
                value: "5fb21249-08bc-4585-ae71-48392889955f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9fc5f185-c6c3-4bcd-90c0-74e35304d69c",
                column: "ConcurrencyStamp",
                value: "9fc5f185-c6c3-4bcd-90c0-74e35304d69c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f3a2308a-3774-4882-8cab-e1b52ce0b48a",
                column: "ConcurrencyStamp",
                value: "f3a2308a-3774-4882-8cab-e1b52ce0b48a");
        }
    }
}

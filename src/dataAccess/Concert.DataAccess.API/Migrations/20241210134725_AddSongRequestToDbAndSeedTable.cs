using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Concert.DataAccess.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSongRequestToDbAndSeedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SongRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Artist = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongRequests", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SongRequests",
                columns: new[] { "Id", "Artist", "CreatedAt", "Genre", "Language", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Ace of base", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reggae", "English", "All that she wants", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "And One", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "EBM", "English", "Military fashion show", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "Ascendant Vierge", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "EDM", "French", "Influenceur", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "Boys", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Disco polo", "Polish", "Szalona", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "Charles Aznavour", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chanson française", "-", "For me Formidable", new DateTime(2024, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongRequests");
        }
    }
}

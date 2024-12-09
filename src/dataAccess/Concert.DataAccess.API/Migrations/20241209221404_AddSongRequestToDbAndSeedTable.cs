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
                    Language = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongRequests", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SongRequests",
                columns: new[] { "Id", "Artist", "Genre", "Language", "Title" },
                values: new object[,]
                {
                    { 1, "Ace of base", "Reggae", "English", "All that she wants" },
                    { 2, "And One", "EBM", "English", "Military fashion show" },
                    { 3, "Ascendant Vierge", "EDM", "French", "Influenceur" },
                    { 4, "Boys", "Disco polo", "Polish", "Szalona" },
                    { 5, "Charles Aznavour", "Chanson française", "-", "For me Formidable" }
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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Migrations
{
    public partial class AddingDataToTheOfferingPage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhotoUrls",
                columns: table => new
                {
                    PhotoUrlId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoUrls", x => x.PhotoUrlId);
                });

            migrationBuilder.CreateTable(
                name: "OfferingPhotoUrlMappings",
                columns: table => new
                {
                    OfferingPhotoUrlMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    PhotoUrlId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferingPhotoUrlMappings", x => x.OfferingPhotoUrlMappingId);
                    table.ForeignKey(
                        name: "FK_OfferingPhotoUrlMappings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "OfferingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferingPhotoUrlMappings_PhotoUrls_PhotoUrlId",
                        column: x => x.PhotoUrlId,
                        principalTable: "PhotoUrls",
                        principalColumn: "PhotoUrlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfferingPhotoUrlMappings_OfferingId",
                table: "OfferingPhotoUrlMappings",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferingPhotoUrlMappings_PhotoUrlId",
                table: "OfferingPhotoUrlMappings",
                column: "PhotoUrlId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfferingPhotoUrlMappings");

            migrationBuilder.DropTable(
                name: "PhotoUrls");
        }
    }
}

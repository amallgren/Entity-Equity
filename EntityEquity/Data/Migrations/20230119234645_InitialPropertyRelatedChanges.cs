using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Data.Migrations
{
    public partial class InitialPropertyRelatedChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Properties");
        }
    }
}

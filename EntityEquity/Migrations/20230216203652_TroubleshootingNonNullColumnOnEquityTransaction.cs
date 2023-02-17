using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Migrations
{
    public partial class TroubleshootingNonNullColumnOnEquityTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EquityOfferId",
                table: "EquityTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EquityOfferId",
                table: "EquityTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

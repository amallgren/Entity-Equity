using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Migrations
{
    public partial class RemovingPaymentDetailsFromOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDetails",
                table: "Orders");

            migrationBuilder.AddColumn<bool>(
                name: "MustShip",
                table: "Offerings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustShip",
                table: "Offerings");

            migrationBuilder.AddColumn<string>(
                name: "PaymentDetails",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

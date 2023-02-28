using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Migrations
{
    public partial class MadeBillingAddressOptionalForOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BillingAddresses_BillingAddressId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "BillingAddressId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BillingAddresses_BillingAddressId",
                table: "Orders",
                column: "BillingAddressId",
                principalTable: "BillingAddresses",
                principalColumn: "BillingAddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BillingAddresses_BillingAddressId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "BillingAddressId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BillingAddresses_BillingAddressId",
                table: "Orders",
                column: "BillingAddressId",
                principalTable: "BillingAddresses",
                principalColumn: "BillingAddressId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Migrations
{
    public partial class ChangingSameAsBillingAddressColumnInShippingAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SameAsBillingAddress",
                table: "ShippingAddresses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SameAsBillingAddress",
                table: "ShippingAddresses");
        }
    }
}

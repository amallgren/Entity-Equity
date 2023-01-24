﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityEquity.Data.Migrations
{
    public partial class FixedOfferingIssueWithDecimalPrecisionAndScale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Offerings",
                type: "decimal(15, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Offerings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15, 2");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificaPix.Infrastructure.Migrations;

public partial class BankApiIntegrationFlags : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsTested",
            table: "BankApiIntegrations",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastTestedAt",
            table: "BankApiIntegrations",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "ProductionEnabled",
            table: "BankApiIntegrations",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsTested",
            table: "BankApiIntegrations");

        migrationBuilder.DropColumn(
            name: "LastTestedAt",
            table: "BankApiIntegrations");

        migrationBuilder.DropColumn(
            name: "ProductionEnabled",
            table: "BankApiIntegrations");
    }
}

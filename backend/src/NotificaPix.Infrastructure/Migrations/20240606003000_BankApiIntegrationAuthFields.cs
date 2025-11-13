using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificaPix.Infrastructure.Migrations;

public partial class BankApiIntegrationAuthFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AccountIdentifier",
            table: "BankApiIntegrations",
            type: "varchar(64)",
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "ApiKey",
            table: "BankApiIntegrations",
            type: "varchar(64)",
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AccountIdentifier",
            table: "BankApiIntegrations");

        migrationBuilder.DropColumn(
            name: "ApiKey",
            table: "BankApiIntegrations");
    }
}

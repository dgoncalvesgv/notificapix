using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificaPix.Infrastructure.Migrations;

public partial class BankApiIntegrationServiceUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ServiceUrl",
            table: "BankApiIntegrations",
            type: "varchar(512)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ServiceUrl",
            table: "BankApiIntegrations");
    }
}

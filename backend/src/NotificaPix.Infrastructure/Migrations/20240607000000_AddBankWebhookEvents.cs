using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificaPix.Infrastructure.Migrations;

public partial class AddBankWebhookEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BankWebhookEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                BankApiIntegrationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                OrganizationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Bank = table.Column<string>(type: "varchar(64)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                EventId = table.Column<string>(type: "varchar(128)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                EventType = table.Column<string>(type: "varchar(64)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Signature = table.Column<string>(type: "varchar(256)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Payload = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ReceivedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankWebhookEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_BankWebhookEvents_BankApiIntegrations_BankApiIntegrationId",
                    column: x => x.BankApiIntegrationId,
                    principalTable: "BankApiIntegrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_BankWebhookEvents_BankApiIntegrationId_EventId",
            table: "BankWebhookEvents",
            columns: new[] { "BankApiIntegrationId", "EventId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BankWebhookEvents");
    }
}

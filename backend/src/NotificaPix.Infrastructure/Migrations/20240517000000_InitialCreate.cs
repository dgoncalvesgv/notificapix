using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificaPix.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                Slug = table.Column<string>(maxLength: 255, nullable: false),
                Plan = table.Column<string>(maxLength: 32, nullable: false),
                StripeCustomerId = table.Column<string>(nullable: true),
                StripeSubscriptionId = table.Column<string>(nullable: true),
                StripePriceId = table.Column<string>(nullable: true),
                UsageMonth = table.Column<DateTime>(nullable: false),
                UsageCount = table.Column<int>(nullable: false),
                BillingEmail = table.Column<string>(maxLength: 256, nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Organizations", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                Email = table.Column<string>(maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(nullable: false),
                IsActive = table.Column<bool>(nullable: false),
                LastLoginAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "NotificationSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                EmailsCsv = table.Column<string>(nullable: false),
                WebhookUrl = table.Column<string>(nullable: true),
                WebhookSecret = table.Column<string>(nullable: true),
                Enabled = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                table.ForeignKey(
                    name: "FK_NotificationSettings_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BankConnections",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                Provider = table.Column<string>(maxLength: 32, nullable: false),
                ConsentId = table.Column<string>(nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false),
                ConnectedAt = table.Column<DateTime>(nullable: true),
                MetaJson = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankConnections", x => x.Id);
                table.ForeignKey(
                    name: "FK_BankConnections_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Memberships",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                Role = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Memberships", x => x.Id);
                table.ForeignKey(
                    name: "FK_Memberships_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Memberships_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PixTransactions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                TxId = table.Column<string>(nullable: false),
                EndToEndId = table.Column<string>(nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                OccurredAt = table.Column<DateTime>(nullable: false),
                PayerName = table.Column<string>(nullable: false),
                PayerKey = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                RawJson = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PixTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PixTransactions_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Alerts",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                PixTransactionId = table.Column<Guid>(nullable: false),
                Channel = table.Column<string>(maxLength: 32, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false),
                Attempts = table.Column<int>(nullable: false),
                LastAttemptAt = table.Column<DateTime>(nullable: true),
                PayloadJson = table.Column<string>(nullable: false),
                ErrorMessage = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Alerts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Alerts_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Alerts_PixTransactions_PixTransactionId",
                    column: x => x.PixTransactionId,
                    principalTable: "PixTransactions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Invites",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                Email = table.Column<string>(nullable: false),
                Role = table.Column<string>(maxLength: 32, nullable: false),
                Token = table.Column<string>(nullable: false),
                ExpiresAt = table.Column<DateTime>(nullable: false),
                AcceptedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Invites", x => x.Id);
                table.ForeignKey(
                    name: "FK_Invites_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ApiKeys",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(nullable: false),
                KeyHash = table.Column<string>(nullable: false),
                LastUsedAt = table.Column<DateTime>(nullable: true),
                IsActive = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeys", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApiKeys_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                ActorUserId = table.Column<Guid>(nullable: false),
                Action = table.Column<string>(nullable: false),
                DataJson = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuditLogs_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Alerts_OrganizationId", table: "Alerts", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_Alerts_PixTransactionId", table: "Alerts", column: "PixTransactionId");
        migrationBuilder.CreateIndex(name: "IX_ApiKeys_OrganizationId", table: "ApiKeys", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_ApiKeys_OrganizationId_Name", table: "ApiKeys", columns: new[] { "OrganizationId", "Name" });
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_OrganizationId", table: "AuditLogs", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_BankConnections_OrganizationId", table: "BankConnections", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_Invites_OrganizationId", table: "Invites", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_Invites_Token", table: "Invites", column: "Token", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Memberships_OrganizationId_UserId", table: "Memberships", columns: new[] { "OrganizationId", "UserId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Memberships_UserId", table: "Memberships", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_NotificationSettings_OrganizationId", table: "NotificationSettings", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_Organizations_Slug", table: "Organizations", column: "Slug", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PixTransactions_OrganizationId", table: "PixTransactions", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Alerts");
        migrationBuilder.DropTable(name: "ApiKeys");
        migrationBuilder.DropTable(name: "AuditLogs");
        migrationBuilder.DropTable(name: "BankConnections");
        migrationBuilder.DropTable(name: "Invites");
        migrationBuilder.DropTable(name: "Memberships");
        migrationBuilder.DropTable(name: "NotificationSettings");
        migrationBuilder.DropTable(name: "PixTransactions");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.DropTable(name: "Organizations");
    }
}

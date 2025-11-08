using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NotificaPix.Infrastructure.Persistence;

#nullable disable

namespace NotificaPix.Infrastructure.Migrations;

[DbContext(typeof(NotificaPixDbContext))]
partial class NotificaPixDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0");

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.Organization", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<string>("BillingEmail").IsRequired().HasMaxLength(256);
            b.Property<DateTime>("CreatedAt");
            b.Property<string>("Name").IsRequired().HasMaxLength(255);
            b.Property<string>("Plan").IsRequired().HasMaxLength(32);
            b.Property<string>("Slug").IsRequired().HasMaxLength(255);
            b.Property<string>("StripeCustomerId");
            b.Property<string>("StripePriceId");
            b.Property<string>("StripeSubscriptionId");
            b.Property<int>("UsageCount");
            b.Property<DateTime>("UsageMonth");
            b.HasKey("Id");
            b.HasIndex("Slug").IsUnique();
            b.ToTable("Organizations");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.User", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTime>("CreatedAt");
            b.Property<string>("Email").IsRequired().HasMaxLength(256);
            b.Property<bool>("IsActive");
            b.Property<DateTime?>("LastLoginAt");
            b.Property<string>("PasswordHash").IsRequired();
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.Membership", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTime>("CreatedAt");
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Role").IsRequired().HasMaxLength(32);
            b.Property<Guid>("UserId");
            b.HasKey("Id");
            b.HasIndex("UserId");
            b.HasIndex("OrganizationId", "UserId").IsUnique();
            b.ToTable("Memberships");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.NotificationSettings", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("Enabled");
            b.Property<string>("EmailsCsv").IsRequired();
            b.Property<Guid>("OrganizationId");
            b.Property<string>("WebhookSecret");
            b.Property<string>("WebhookUrl");
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.ToTable("NotificationSettings");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.BankConnection", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<string>("ConsentId").IsRequired();
            b.Property<DateTime>("CreatedAt");
            b.Property<DateTime?>("ConnectedAt");
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Provider").IsRequired().HasMaxLength(32);
            b.Property<string>("Status").IsRequired().HasMaxLength(32);
            b.Property<string>("MetaJson");
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.ToTable("BankConnections");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.PixTransaction", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<DateTime>("CreatedAt");
            b.Property<string>("Description").IsRequired();
            b.Property<string>("EndToEndId").IsRequired();
            b.Property<DateTime>("OccurredAt");
            b.Property<Guid>("OrganizationId");
            b.Property<string>("PayerKey").IsRequired();
            b.Property<string>("PayerName").IsRequired();
            b.Property<string>("RawJson").IsRequired();
            b.Property<string>("TxId").IsRequired();
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.ToTable("PixTransactions");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.Alert", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<int>("Attempts");
            b.Property<string>("Channel").IsRequired().HasMaxLength(32);
            b.Property<DateTime>("CreatedAt");
            b.Property<string>("ErrorMessage");
            b.Property<DateTime?>("LastAttemptAt");
            b.Property<Guid>("OrganizationId");
            b.Property<string>("PayloadJson").IsRequired();
            b.Property<Guid>("PixTransactionId");
            b.Property<string>("Status").IsRequired().HasMaxLength(32);
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.HasIndex("PixTransactionId");
            b.ToTable("Alerts");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.Invite", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTime?>("AcceptedAt");
            b.Property<string>("Email").IsRequired();
            b.Property<DateTime>("ExpiresAt");
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Role").IsRequired().HasMaxLength(32);
            b.Property<string>("Token").IsRequired();
            b.Property<DateTime>("CreatedAt");
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.HasIndex("Token").IsUnique();
            b.ToTable("Invites");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.ApiKey", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Name").IsRequired();
            b.Property<string>("KeyHash").IsRequired();
            b.Property<DateTime?>("LastUsedAt");
            b.Property<bool>("IsActive");
            b.Property<DateTime>("CreatedAt");
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.HasIndex("OrganizationId", "Name");
            b.ToTable("ApiKeys");
        });

        modelBuilder.Entity("NotificaPix.Core.Domain.Entities.AuditLog", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<Guid>("ActorUserId");
            b.Property<string>("Action").IsRequired();
            b.Property<string>("DataJson").IsRequired();
            b.Property<Guid>("OrganizationId");
            b.Property<DateTime>("CreatedAt");
            b.HasKey("Id");
            b.HasIndex("OrganizationId");
            b.ToTable("AuditLogs");
        });
#pragma warning restore 612, 618
    }
}

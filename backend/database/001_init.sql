-- NotificaPix schema bootstrap
-- Run with: mysql -h 193.203.175.133 -u u360528542_notificapix -pX278g113 u360528542_notificapix < backend/database/001_init.sql

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE IF NOT EXISTS Organizations (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    Name VARCHAR(255) NOT NULL,
    Slug VARCHAR(255) NOT NULL,
    Plan VARCHAR(32) NOT NULL,
    StripeCustomerId VARCHAR(100),
    StripeSubscriptionId VARCHAR(100),
    StripePriceId VARCHAR(100),
    UsageMonth DATE NOT NULL,
    UsageCount INT NOT NULL DEFAULT 0,
    BillingEmail VARCHAR(256) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Organizations_Slug (Slug)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Users (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    Email VARCHAR(256) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    LastLoginAt DATETIME(6),
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Users_Email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Memberships (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    UserId CHAR(36) NOT NULL,
    Role VARCHAR(32) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Memberships_OrgUser (OrganizationId, UserId),
    KEY IX_Memberships_UserId (UserId),
    CONSTRAINT FK_Memberships_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Memberships_User FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS NotificationSettings (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    EmailsCsv TEXT NOT NULL,
    WebhookUrl VARCHAR(512),
    WebhookSecret VARCHAR(256),
    Enabled TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (Id),
    KEY IX_NotificationSettings_Org (OrganizationId),
    CONSTRAINT FK_NotifSettings_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS BankConnections (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    Provider VARCHAR(32) NOT NULL,
    ConsentId VARCHAR(200) NOT NULL,
    Status VARCHAR(32) NOT NULL,
    ConnectedAt DATETIME(6),
    MetaJson JSON,
    PRIMARY KEY (Id),
    KEY IX_BankConnections_Org (OrganizationId),
    CONSTRAINT FK_BankConnections_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS PixTransactions (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    TxId VARCHAR(64) NOT NULL,
    EndToEndId VARCHAR(64) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    OccurredAt DATETIME(6) NOT NULL,
    PayerName VARCHAR(255) NOT NULL,
    PayerKey VARCHAR(255) NOT NULL,
    Description VARCHAR(512) NOT NULL,
    RawJson JSON NOT NULL,
    PRIMARY KEY (Id),
    KEY IX_PixTransactions_Org (OrganizationId),
    CONSTRAINT FK_PixTransactions_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Alerts (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    PixTransactionId CHAR(36) NOT NULL,
    Channel VARCHAR(32) NOT NULL,
    Status VARCHAR(32) NOT NULL,
    Attempts INT NOT NULL DEFAULT 0,
    LastAttemptAt DATETIME(6),
    PayloadJson JSON NOT NULL,
    ErrorMessage TEXT,
    PRIMARY KEY (Id),
    KEY IX_Alerts_Org (OrganizationId),
    KEY IX_Alerts_Pix (PixTransactionId),
    CONSTRAINT FK_Alerts_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Alerts_Pix FOREIGN KEY (PixTransactionId) REFERENCES PixTransactions (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Invites (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    Email VARCHAR(256) NOT NULL,
    Role VARCHAR(32) NOT NULL,
    Token VARCHAR(128) NOT NULL,
    ExpiresAt DATETIME(6) NOT NULL,
    AcceptedAt DATETIME(6),
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Invites_Token (Token),
    KEY IX_Invites_Org (OrganizationId),
    CONSTRAINT FK_Invites_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ApiKeys (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    Name VARCHAR(128) NOT NULL,
    KeyHash VARCHAR(128) NOT NULL,
    LastUsedAt DATETIME(6),
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (Id),
    KEY IX_ApiKeys_Org (OrganizationId),
    KEY IX_ApiKeys_Org_Name (OrganizationId, Name),
    CONSTRAINT FK_ApiKeys_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS AuditLogs (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    ActorUserId CHAR(36) NOT NULL,
    Action VARCHAR(255) NOT NULL,
    DataJson JSON NOT NULL,
    PRIMARY KEY (Id),
    KEY IX_AuditLogs_Org (OrganizationId),
    CONSTRAINT FK_AuditLogs_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS = 1;

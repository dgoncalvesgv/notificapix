ALTER TABLE Organizations
    ADD COLUMN DefaultPixKeyId CHAR(36) NULL;

CREATE TABLE IF NOT EXISTS PixKeys (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    Label VARCHAR(128) NOT NULL,
    KeyType VARCHAR(32) NOT NULL,
    KeyValue VARCHAR(128) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_PixKeys_Org_Value (OrganizationId, KeyValue),
    CONSTRAINT FK_PixKeys_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PixStaticQrCodes (
    Id CHAR(36) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    OrganizationId CHAR(36) NOT NULL,
    PixKeyId CHAR(36) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Payload TEXT NOT NULL,
    TxId VARCHAR(64) NOT NULL,
    PRIMARY KEY (Id),
    KEY IX_PixStaticQrCodes_Org (OrganizationId),
    KEY IX_PixStaticQrCodes_Key (PixKeyId),
    CONSTRAINT FK_PixStaticQrCodes_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE,
    CONSTRAINT FK_PixStaticQrCodes_Key FOREIGN KEY (PixKeyId) REFERENCES PixKeys (Id) ON DELETE RESTRICT
);

ALTER TABLE Organizations
    ADD CONSTRAINT FK_Organizations_PixKey FOREIGN KEY (DefaultPixKeyId) REFERENCES PixKeys (Id) ON DELETE SET NULL;

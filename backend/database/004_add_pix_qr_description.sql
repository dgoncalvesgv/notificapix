ALTER TABLE PixStaticQrCodes
    ADD COLUMN Description VARCHAR(256) NOT NULL DEFAULT '' AFTER Amount,
    ADD KEY IX_PixStaticQrCodes_CreatedAt (CreatedAt);

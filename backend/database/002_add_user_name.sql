ALTER TABLE Users
    ADD COLUMN Name VARCHAR(128) NOT NULL DEFAULT '' AFTER CreatedAt;

UPDATE Users
SET Name = CASE
    WHEN LOCATE('@', Email) > 0 THEN SUBSTRING(Email, 1, LOCATE('@', Email) - 1)
    ELSE Email
END
WHERE Name = '' OR Name IS NULL;

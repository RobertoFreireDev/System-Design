CREATE TABLE items (
    id SERIAL PRIMARY KEY,
    value INT NOT NULL,
    version INT NOT NULL DEFAULT 0
);

CREATE TABLE item_updates_log (
    log_id SERIAL PRIMARY KEY,
    item_id INT NOT NULL,
    old_value INT,
    new_value INT,
    old_version INT,
    new_version INT,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE item_updates_log (
    log_id SERIAL PRIMARY KEY,
    item_id INT NOT NULL,
    old_value INT,
    new_value INT,
    old_version INT,
    new_version INT,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE OR REPLACE FUNCTION log_item_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO item_updates_log (
        item_id,
        old_value,
        new_value,
        old_version,
        new_version,
        updated_at
    )
    VALUES (
        OLD.id,
        OLD.value,
        NEW.value,
        OLD.version,
        NEW.version,
        NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_log_item_updates
AFTER UPDATE ON items
FOR EACH ROW
EXECUTE FUNCTION log_item_update();

INSERT INTO items (value) VALUES (10);

UPDATE items SET value = value + 1, version = version + 1 WHERE id = 2;

Delete from items;
Delete from item_updates_log;
SELECT * FROM items;
SELECT * FROM item_updates_log;


-- OptimisticConcurrencyUpdateItem
CREATE OR REPLACE FUNCTION check_version()
RETURNS trigger AS $$
BEGIN
    IF OLD.version <> NEW.version THEN
        RAISE EXCEPTION 'Version mismatch for id %', OLD.id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_version
BEFORE UPDATE ON items
FOR EACH ROW
EXECUTE FUNCTION check_version();
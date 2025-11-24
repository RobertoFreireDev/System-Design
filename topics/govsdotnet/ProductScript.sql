CREATE TABLE products (
    id SERIAL PRIMARY KEY,        -- Auto-incrementing integer ID
    name TEXT NOT NULL,           -- Product name
    number INT NOT NULL           -- Some numeric value
);

INSERT INTO products (name, number)
VALUES 
    ('Laptop', 1);

SELECT * FROM products;

DELETE FROM products

drop table products
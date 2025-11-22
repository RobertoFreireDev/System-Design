# System-Design

## Configuration

docker compose up -d

PGADMIN: http://localhost:16543

```
PGADMIN_DEFAULT_EMAIL: "simha@yahoo.com.br"
PGADMIN_DEFAULT_PASSWORD: "PgAdmin2019!"
Server name: postgres-db
Host Name: postgres-db
Maintenance database: weather
Port: 5432
Username: simha
Password: Postgres2019!
```

## Testing Concurrency Issue

Run scripts from topics\Script.sql

Run ConcurrencyDemoConsoleApp

SELECT * FROM items;

SELECT * FROM item_updates_log;

### General concurrency issue

All concurrent requests are completed but using the initial value, leading to wrong calculation if the update depends on previous value

Example: Two requests read value=10 at the same time, both add 1, and write back 11. The correct final value should be 12 instead of 11.

### Optimistic concurrency control

Only the first concurrent request is completed. All the other requests fail (or retry, depending on your logic) if the version has changed.

### Pessimistic concurrency control (PCC)

All concurrent requests are completed because the database locks each request. No lost updates, but requests may be delayed due to waiting for the lock.


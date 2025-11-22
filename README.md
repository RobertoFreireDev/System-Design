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

Concurrency issue:

| log_id | item_id | old_value | new_value | old_version | new_version | updated_at               |
|--------|---------|-----------|-----------|-------------|-------------|-------------------------|
| 5      | 1       | 0         | 6         | 0           | 1           | 2025-11-22 14:17:51.798969 |
| 6      | 1       | 6         | 2         | 1           | 1           | 2025-11-22 14:17:51.902468 |
| 7      | 1       | 2         | 1         | 1           | 1           | 2025-11-22 14:17:52.055698 |
| 8      | 1       | 1         | 3         | 1           | 1           | 2025-11-22 14:17:52.101554 |



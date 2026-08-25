# Users Table Schema

The `users` table stores application login credentials and role assignments for the ASID system's role-based access control (RBAC) layer.

Passwords are never stored in plaintext. Each password is hashed with PBKDF2 (SHA-256, 100,000 iterations, 16-byte salt, 32-byte hash) and stored as a single self-describing string in the format:

```text
{iterations}.{saltBase64}.{hashBase64}
```

---

## 📜 SQL DDL Script

```sql
CREATE TABLE users
(
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username      TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role          TEXT NOT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_username
    ON users(username);
```

---

## 📋 Column Specifications

| Column Name | Data Type | Nullable | Constraints / Default | Description |
| :--- | :--- | :---: | :--- | :--- |
| `id` | `UUID` | No | `PRIMARY KEY`, `gen_random_uuid()` | Unique user surrogate key. |
| `username` | `TEXT` | No | `UNIQUE` | Normalized (lowercase) login identifier. |
| `password_hash` | `TEXT` | No | - | PBKDF2 hash string in `{iterations}.{saltBase64}.{hashBase64}` format. |
| `role` | `TEXT` | No | - | Access role: `operator`, `qa`, or `supervisor`. |
| `created_at` | `TIMESTAMP` | Yes | `CURRENT_TIMESTAMP` | Timestamp when the user was created. |
| `updated_at` | `TIMESTAMP` | Yes | `CURRENT_TIMESTAMP` | Timestamp when the user was last updated. |

---

## ⚡ Indexes

- **`idx_users_username`**: Single-column index on `username` for fast login lookups (the `UNIQUE` constraint also enforces normalized-name uniqueness).

---

## 👥 Seed Users (dev-only)

| Username | Role | Plaintext (dev-only) |
| :--- | :--- | :--- |
| `rpingkian` | `operator` | `1234` |
| `vsendrijas` | `qa` | `5678` |
| `cordonez` | `supervisor` | `4567` |

Only the PBKDF2 hashes are committed; the plaintext values above appear solely for local development and testing. Regenerate hashes with `dotnet run --project tools/SeedHashGenerator`.

## 🔐 Role / Access Matrix

| Capability | operator | qa | supervisor |
| :--- | :---: | :---: | :---: |
| Open application | ✅ | ✅ | ✅ |
| Flag Non-Conformance | ❌ | ✅ | ✅ |
| QA Review NC | ❌ | ❌ | ✅ |
| Import Daily Demand (DeleteAll override) | ❌ | ❌ | ✅ |

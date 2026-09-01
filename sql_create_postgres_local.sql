/*
===============================================================================
 ASID - PostgreSQL Schema (local instance)
===============================================================================
 Purpose : Create the ASID tables on a LOCAL PostgreSQL instance.
 Source  : Mirrors the live ASID PostgreSQL schema (asid_db) on Neon.
           Table & column names are kept identical.
 Run     : Execute against your local database, e.g.:
             psql -U <user> -d <database> -f sql_create_postgres_local.sql
===============================================================================
*/

-- ===========================================================================
-- transactions
-- ===========================================================================
DROP TABLE IF EXISTS transactions;

CREATE TABLE transactions
(
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),

    data_matrix     TEXT        NOT NULL UNIQUE,
    serial_no       TEXT        NOT NULL,

    model           TEXT        NOT NULL,
    part_no         TEXT        NOT NULL,
    quantity        INTEGER     NOT NULL,
    kanban_no       TEXT        NOT NULL,

    operator_id     TEXT,
    line_no         TEXT,
    lane_no         TEXT,
    trolley_no      TEXT,

    station         TEXT        NOT NULL,
    status          TEXT        NOT NULL,

    created_at      TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
    withdrawn_at    TIMESTAMP,
    forpickup_at    TIMESTAMP,
    received_at     TIMESTAMP,
    consumed_at     TIMESTAMP,

    is_suspected_nc BOOLEAN     NOT NULL DEFAULT FALSE,
    is_nc_confirmed BOOLEAN     NOT NULL DEFAULT FALSE,
    is_nc_rejected  BOOLEAN     NOT NULL DEFAULT FALSE,
    nc_quantity     INTEGER     NOT NULL DEFAULT 0
);

-- Performance indexes
CREATE INDEX idx_transaction_datamatrix
    ON transactions(data_matrix);

CREATE INDEX idx_transaction_status
    ON transactions(status);

-- ===========================================================================
-- daily_demand
-- ===========================================================================
DROP TABLE IF EXISTS daily_demand;

CREATE TABLE daily_demand
(
    id              BIGSERIAL   PRIMARY KEY,
    production_date DATE        NOT NULL,
    shift           SMALLINT    NOT NULL,
    model           VARCHAR(100),
    part_no         VARCHAR(100) NOT NULL,
    quantity        INTEGER     NOT NULL,
    scrapped        INTEGER     NOT NULL DEFAULT 0,
    imported_at     TIMESTAMP   DEFAULT CURRENT_TIMESTAMP
);

-- ===========================================================================
-- users
-- ===========================================================================
DROP TABLE IF EXISTS users;

CREATE TABLE users
(
    id            UUID      PRIMARY KEY DEFAULT gen_random_uuid(),
    username      TEXT      NOT NULL UNIQUE,
    password_hash TEXT      NOT NULL,
    role          TEXT      NOT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_username
    ON users(username);

-- Seed users (dev-only). Passwords are stored as PBKDF2 (SHA-256, 100,000 iterations)
-- in the format {iterations}.{saltBase64}.{hashBase64}.
--
-- rpingkian / 1234  → Operator  (stations only)
-- vsendrijas / 5678 → QA        (stations + NC)
-- cordonez / 4567   → Supervisor (stations + NC + override)
-- vsendrijas.p / 7845 → Planner  (dashboard + import production plan)
INSERT INTO users (username, password_hash, role) VALUES
 ('rpingkian',   '100000./PzwRW3iWSQO6ocYyOAmqg==.Qi5DSaA2N6KsFkFHBTRv4qLqr0LmgFPezE3XEZHPFks=', 'operator'),
 ('vsendrijas',  '100000.7gP6pxaWtEshcZDqHig5eQ==.qi4uZgj6uPjiLL/DJBKuVd89i5HDhjLlgRlN5Da4M3s=', 'qa'),
 ('cordonez',    '100000.BPGX/TrkXhsabeg3clB4WA==.+iRnOiyGJT52s0zPSWpxTCBd1PgisUgp7DwJp5Rx6H0=', 'supervisor'),
 ('vsendrijas.p','100000.0swiOwjI3eKKwLGNP5dmXQ==.I4hbwy+svxcKjTXKWwma1PFHwfwaiPE/516bAv2jWWE=', 'planner');

-- ===========================================================================
-- lane_management
-- ===========================================================================
DROP TABLE IF EXISTS lane_management;

CREATE TABLE lane_management
(
    id                SERIAL PRIMARY KEY,
    lane_no           TEXT NOT NULL UNIQUE,
    part_no           TEXT NOT NULL DEFAULT 'Not Assigned',
    max_qty_stored    INTEGER NOT NULL DEFAULT 100,
    actual_stored_qty INTEGER NOT NULL DEFAULT 0,
    withdrawn_qty     INTEGER NOT NULL DEFAULT 0,
    lane_status       TEXT NOT NULL DEFAULT 'Not Assigned',
    color_status      TEXT NOT NULL DEFAULT 'Gray',
    created_at        TIMESTAMP DEFAULT NOW(),
    updated_at        TIMESTAMP DEFAULT NOW()
);

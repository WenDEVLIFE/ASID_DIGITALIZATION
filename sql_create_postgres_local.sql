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

    is_suspected_nc BOOLEAN     NOT NULL DEFAULT FALSE
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
    quantity        INTEGER     NOT NULL
);

/*
===============================================================================
 Migration: Seed all 4 users with correct roles
===============================================================================
 Purpose : Ensures the 4 users exist with correct roles and password hashes.
 Run     : Execute against your live PostgreSQL database.
===============================================================================
*/

-- Operator: rpingkian / 1234
INSERT INTO users (username, password_hash, role)
VALUES ('rpingkian', '100000./PzwRW3iWSQO6ocYyOAmqg==.Qi5DSaA2N6KsFkFHBTRv4qLqr0LmgFPezE3XEZHPFks=', 'operator')
ON CONFLICT (username) DO UPDATE
SET role = 'planner',
    password_hash = '100000.0swiOwjI3eKKwLGNP5dmXQ==.I4hbwy+svxcKjTXKWwma1PFHwfwaiPE/516bAv2jWWE=',
    updated_at = CURRENT_TIMESTAMP;

-- QA: vsendrijas / 5678
INSERT INTO users (username, password_hash, role)
VALUES ('vsendrijas', '100000.7gP6pxaWtEshcZDqHig5eQ==.qi4uZgj6uPjiLL/DJBKuVd89i5HDhjLlgRlN5Da4M3s=', 'qa')
ON CONFLICT (username) DO UPDATE
SET role = 'qa',
    password_hash = '100000.7gP6pxaWtEshcZDqHig5eQ==.qi4uZgj6uPjiLL/DJBKuVd89i5HDhjLlgRlN5Da4M3s=',
    updated_at = CURRENT_TIMESTAMP;

-- Supervisor: cordonez / 4567
INSERT INTO users (username, password_hash, role)
VALUES ('cordonez', '100000.BPGX/TrkXhsabeg3clB4WA==.+iRnOiyGJT52s0zPSWpxTCBd1PgisUgp7DwJp5Rx6H0=', 'supervisor')
ON CONFLICT (username) DO UPDATE
SET role = 'supervisor',
    password_hash = '100000.BPGX/TrkXhsabeg3clB4WA==.+iRnOiyGJT52s0zPSWpxTCBd1PgisUgp7DwJp5Rx6H0=',
    updated_at = CURRENT_TIMESTAMP;

-- Planner: vsendrijas.p / 7845
INSERT INTO users (username, password_hash, role)
VALUES ('vsendrijas.p', '100000.0swiOwjI3eKKwLGNP5dmXQ==.I4hbwy+svxcKjTXKWwma1PFHwfwaiPE/516bAv2jWWE=', 'planner')
ON CONFLICT (username) DO UPDATE
SET role = 'planner',
    password_hash = '100000.0swiOwjI3eKKwLGNP5dmXQ==.I4hbwy+svxcKjTXKWwma1PFHwfwaiPE/516bAv2jWWE=',
    updated_at = CURRENT_TIMESTAMP;

-- Verify
SELECT username, role, created_at, updated_at FROM users ORDER BY role, username;

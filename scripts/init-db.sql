-- PostgreSQL extensions used by the application.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "unaccent";

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_ts_config
        WHERE cfgname = 'polish'
    ) THEN
        CREATE TEXT SEARCH CONFIGURATION polish (COPY = simple);
    END IF;
END $$;

ALTER DATABASE blazorapp SET statement_timeout = '30s';
ALTER DATABASE blazorapp SET lock_timeout = '10s';
ALTER DATABASE blazorapp SET idle_in_transaction_session_timeout = '60s';

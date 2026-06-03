-- Rozszerzenia PostgreSQL
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "unaccent";

-- Konfiguracja full-text search
CREATE TEXT SEARCH CONFIGURATION IF NOT EXISTS polish (COPY = simple);

-- Timeouty ochronne
ALTER DATABASE blazorapp SET statement_timeout = '30s';
ALTER DATABASE blazorapp SET lock_timeout = '10s';
ALTER DATABASE blazorapp SET idle_in_transaction_session_timeout = '60s';

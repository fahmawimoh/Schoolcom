-- Creates the SchoolCom application database.
-- Run as a superuser (e.g. postgres):
--   psql -h localhost -p 5400 -U postgres -f 00-create-database.sql

SELECT 'CREATE DATABASE schoolcom'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'schoolcom')\gexec

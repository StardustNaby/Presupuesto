-- Script de ejemplo para Presupuesto Familiar Mensual
-- Estructura y datos de ejemplo (100 registros por tabla principal)
-- PostgreSQL

-- 1. Estructura de tablas principales

DROP TABLE IF EXISTS "Expenses" CASCADE;
DROP TABLE IF EXISTS "BudgetCategories" CASCADE;
DROP TABLE IF EXISTS "Budgets" CASCADE;
DROP TABLE IF EXISTS "FamilyMembers" CASCADE;
DROP TABLE IF EXISTS "Months" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;

CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "Email" VARCHAR(100) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(200) NOT NULL,
    "FirstName" VARCHAR(50) NOT NULL,
    "LastName" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "LastLoginAt" TIMESTAMP
);

CREATE TABLE "FamilyMembers" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(100),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "UserId" INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL
);

CREATE TABLE "Months" (
    "Id" SERIAL PRIMARY KEY,
    "MonthNumber" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "Name" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX idx_month_unique ON "Months" ("MonthNumber", "Year");

CREATE TABLE "Budgets" (
    "Id" SERIAL PRIMARY KEY,
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "FamilyMemberId" INTEGER NOT NULL REFERENCES "FamilyMembers"("Id") ON DELETE RESTRICT,
    "MonthId" INTEGER NOT NULL REFERENCES "Months"("Id") ON DELETE RESTRICT,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    UNIQUE ("FamilyMemberId", "MonthId")
);

CREATE TABLE "BudgetCategories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Limit" DECIMAL(18,2) NOT NULL,
    "BudgetId" INTEGER NOT NULL REFERENCES "Budgets"("Id") ON DELETE CASCADE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    UNIQUE ("BudgetId", "Name")
);

CREATE TABLE "Expenses" (
    "Id" SERIAL PRIMARY KEY,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Description" VARCHAR(200) NOT NULL,
    "BudgetCategoryId" INTEGER NOT NULL REFERENCES "BudgetCategories"("Id") ON DELETE RESTRICT,
    "FamilyMemberId" INTEGER NOT NULL REFERENCES "FamilyMembers"("Id") ON DELETE RESTRICT,
    "MonthId" INTEGER NOT NULL REFERENCES "Months"("Id") ON DELETE RESTRICT,
    "Date" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL
);

-- 2. Datos de ejemplo

-- Usuarios
INSERT INTO "Users" ("Username", "Email", "PasswordHash", "FirstName", "LastName", "IsActive", "CreatedAt")
SELECT 
    'usuario' || i, 
    'usuario' || i || '@mail.com', 
    '$2a$11$EjemploHashDePassword1234567890123456789012345678901234567890',
    'Nombre' || i, 
    'Apellido' || i, 
    TRUE, 
    NOW() - (i || ' days')::interval
FROM generate_series(1, 100) AS s(i);

-- Miembros de familia
INSERT INTO "FamilyMembers" ("Name", "Email", "IsActive", "CreatedAt", "UserId")
SELECT 
    'Familiar' || i, 
    'familiar' || i || '@mail.com', 
    TRUE, 
    NOW() - (i || ' days')::interval,
    ((i - 1) % 100) + 1
FROM generate_series(1, 100) AS s(i);

-- Meses (12 meses de 2024, 2023, 2022, 2021, 2020, 2019, 2018, 2017, 2016)
INSERT INTO "Months" ("MonthNumber", "Year", "Name", "CreatedAt")
SELECT 
    ((i - 1) % 12) + 1,
    2024 - ((i - 1) / 12),
    TO_CHAR(DATE '2024-01-01' + ((i - 1) % 12) * INTERVAL '1 month', 'Month'),
    NOW() - (i || ' days')::interval
FROM generate_series(1, 100) AS s(i);

-- Presupuestos
INSERT INTO "Budgets" ("TotalAmount", "FamilyMemberId", "MonthId", "CreatedAt")
SELECT 
    (1000 + (i * 10))::DECIMAL(18,2),
    ((i - 1) % 100) + 1,
    ((i - 1) % 100) + 1,
    NOW() - (i || ' days')::interval
FROM generate_series(1, 100) AS s(i);

-- Categorías de presupuesto
INSERT INTO "BudgetCategories" ("Name", "Limit", "BudgetId", "CreatedAt")
SELECT 
    'Categoria' || i,
    (100 + (i * 2))::DECIMAL(18,2),
    ((i - 1) % 100) + 1,
    NOW() - (i || ' days')::interval
FROM generate_series(1, 100) AS s(i);

-- Gastos
INSERT INTO "Expenses" ("Amount", "Description", "BudgetCategoryId", "FamilyMemberId", "MonthId", "Date", "CreatedAt")
SELECT 
    (10 + (i % 50))::DECIMAL(18,2),
    'Gasto de ejemplo ' || i,
    ((i - 1) % 100) + 1,
    ((i - 1) % 100) + 1,
    ((i - 1) % 100) + 1,
    NOW() - ((i % 30) || ' days')::interval,
    NOW() - (i || ' days')::interval
FROM generate_series(1, 100) AS s(i);

-- Fin del script 
-- Script de inicialización para SQL Server (T-SQL)

-- 1. Crear la base de datos
-- Si ya la creaste manualmente, puedes omitir esta línea
CREATE DATABASE PumpDB;
GO

-- Cambiar el contexto a la nueva base de datos
USE PumpDB;
GO

-- 2. Crear la tabla de bitácora de eventos
CREATE TABLE EventLog (
    Id INT PRIMARY KEY IDENTITY(1,1),         -- Auto-incremental
    Timestamp DATETIME DEFAULT GETDATE(),     -- Registra la fecha y hora exacta del servidor automáticamente
    EventType VARCHAR(50) NOT NULL,           -- Clasificación (Ej: 'INFO', 'ALERTA', 'SISTEMA')
    Description VARCHAR(255) NOT NULL,        -- Descripción humana del suceso
    CurrentLevel FLOAT NOT NULL               -- Porcentaje del tanque en el momento exacto del log
);
GO

-- 3. (Recomendación Senior) Crear un índice para búsquedas rápidas futuras
-- Al buscar en bitácoras industriales normalmente buscarás registros recientes, esto acelera ese proceso.
CREATE NONCLUSTERED INDEX IX_EventLog_Timestamp ON EventLog(Timestamp DESC);
GO

-- Esquema de la base de trazabilidad (SQLite), compartido por la app VB.NET y el módulo Python.
-- Es idempotente: se ejecuta completo cada vez que alguna de las dos apps abre la base.
-- Fechas en ISO 8601 local: 'yyyy-MM-dd HH:mm:ss'.
-- Cambiar una tabla o columna afecta a ambas apps: acordarlo y subir user_version.

PRAGMA journal_mode = WAL;

-- Una fila por etiqueta impresa (antes Registros.txt / Quality_print_records).
CREATE TABLE IF NOT EXISTS etiquetas (
    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    serial                  TEXT    NOT NULL UNIQUE COLLATE NOCASE,  -- R{aa}{letraMes}S{000000}
    consecutivo_global      INTEGER NOT NULL,
    consecutivo_individual  INTEGER NOT NULL,
    area                    TEXT    NOT NULL,
    spot                    TEXT,
    asset                   TEXT,                                    -- nombre de la máquina que imprimió
    wo                      TEXT    NOT NULL,
    item                    TEXT    NOT NULL,                        -- M/I
    revision                TEXT,                                    -- Rv, solo en PIPING
    supervisor              TEXT,
    no_id                   TEXT,
    turno                   TEXT,
    copias                  INTEGER,
    fecha_impresion         TEXT    NOT NULL,
    estado                  TEXT    NOT NULL DEFAULT 'IMPRESA'
                                    CHECK (estado IN ('IMPRESA', 'LIBERADA', 'RECHAZADA')),
    liberado_por            TEXT,
    fecha_liberacion        TEXT,
    modelo_final            TEXT
);

CREATE INDEX IF NOT EXISTS ix_etiquetas_wo ON etiquetas (wo);

-- Usuarios de QA (reemplazará PassQA.txt). Hash PBKDF2-SHA256 en Base64.
CREATE TABLE IF NOT EXISTS usuarios (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    usuario         TEXT    NOT NULL UNIQUE COLLATE NOCASE,
    password_hash   TEXT    NOT NULL,
    salt            TEXT    NOT NULL,
    rol             TEXT    NOT NULL DEFAULT 'QA',
    creado          TEXT    NOT NULL
);

-- Bitácora de cada lectura de etiqueta y la decisión del motor de inferencia (la escribe Python).
CREATE TABLE IF NOT EXISTS inspecciones (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha           TEXT    NOT NULL,
    usuario         TEXT,
    imagen_ruta     TEXT,
    metodo          TEXT    NOT NULL CHECK (metodo IN ('barcode', 'ocr', 'manual')),
    serial_leido    TEXT,
    item_leido      TEXT,
    wo_leido        TEXT,
    revision_leida  TEXT,
    fecha_leida     TEXT,
    confianza_ocr   REAL,
    nitidez         REAL,                                            -- varianza del Laplaciano
    decision        TEXT    NOT NULL
                            CHECK (decision IN ('LIBERAR', 'RECHAZAR', 'REVISION_MANUAL')),
    motivos         TEXT,                                            -- JSON: [{"codigo": ..., "mensaje": ...}]
    etiqueta_id     INTEGER REFERENCES etiquetas (id)
);

CREATE INDEX IF NOT EXISTS ix_inspecciones_serial ON inspecciones (serial_leido);

PRAGMA user_version = 1;

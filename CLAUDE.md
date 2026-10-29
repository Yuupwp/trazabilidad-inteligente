# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Windows Forms desktop app (VB.NET, .NET 8, `net8.0-windows`) for printing quality-traceability labels on a production floor, plus a Python module that reads a printed label with a camera (barcodes + OCR) and decides with a rule engine whether it can be released. Code, comments, and docs are in Spanish; most on-screen labels are in English (*Print*, *Reprint*, *Release*, *Reject*, *Register*). Solution: `EtiquetasV4.sln`; single VB project `EtiquetasV4/Etiquetas.vbproj` (assembly name `Etiquetas`).

## Commands

```
dotnet build EtiquetasV4.sln
dotnet run --project EtiquetasV4/Etiquetas.vbproj
dotnet publish EtiquetasV4/Etiquetas.vbproj -p:PublishProfile=FolderProfile   # -> bin\Release\ActualizacionEtiquetas

# Python module (from EtiquetasV4/OCR)
python -m unittest test_motor -v                                 # rule-engine tests, no camera/DB needed
python verificar_etiqueta.py imagen.png <ruta.db> [--una-vez]    # verify one image
python verificar_etiqueta.py --camara <ruta.db> [--una-vez]      # webcam: SPACE verifies, Q quits
```

There are no VB tests or linters. Publish profiles (Folder and ClickOnce) live in `EtiquetasV4/My Project/PublishProfiles/`. Python deps: `opencv-python`, `pyzbar`, `pytesseract`, plus Tesseract OCR for Windows (auto-detected under `C:\Program Files\Tesseract-OCR` if not on PATH); pyzbar may need the VC++ redistributable.

## Architecture

- **Entry point**: `My Project/Application.myapp` sets `MainForm = Form1`. `Form1` holds nearly all logic; the other forms are small dialogs.
- **Form1 has two panels that swap positions** (`IntercambiarPosiciones`) rather than being shown/hidden:
  - *Trazabilidad*: `Print` prints a label via `PrintDocument1_PrintPage`, which generates the serial number and inserts a record. `Reprint` goes through the same handler, so it **creates a new serial and record**.
  - *Liberación* (QA release), gated behind the `Liberacion` login dialog: `Search` looks up a serial (and warns if it is no longer `IMPRESA`); `Print` first calls `BaseSqlite.Liberar` and only prints the release label (`PrintDocument2_PrintPage`, drawing only) if that succeeded; `Reprint` reprints the last release label of the session without touching the DB; `Reject` marks the label `RECHAZADA`.
  - `Register` uses the same login, then opens the `Registro` form to add users. If the `usuarios` table is empty it opens `Registro` directly (first-user bootstrap).
- **OCR verification**: a "Verify label (OCR)" button is created in code (`AgregarBotonVerificacion`). `VerificadorOCR.vb` runs `python OCR\verificar_etiqueta.py ... <BaseSqlite.RutaBase> --una-vez` (the `OCR\*.py` files are copied to the output dir) and maps its exit code: 0 `LIBERAR`, 1 `RECHAZAR`, 3 `REVISION_MANUAL`, 2 error. `VentanaResultadoOCR.vb` (code-only form, no designer) shows the result with a colored header. Verification is advisory: it writes a row in `inspecciones` but never changes `etiquetas.estado`; release is still done in the *Liberación* panel.
- **Python module** (`EtiquetasV4/OCR/`): `verificar_etiqueta.py` is vision (OpenCV preprocessing variants, pyzbar Code 39, Tesseract OCR, Laplacian-variance sharpness); `motor.py` is the rule engine (each rule returns problems with severity `critica`/`advertencia`/`aviso`; any `critica` → RECHAZAR, any `advertencia` → REVISION_MANUAL, else LIBERAR). Thresholds `UMBRAL_NITIDEZ` and `UMBRAL_SIMILITUD_WO` are at the top of `motor.py`.
- **The station mode comes from `Etiquetas/Area.txt`**. If it is `PIPING`, an extra "Rv" field is shown, printed and stored in `revision`. If it is `EMBARQUES`, Form1 hides the traceability UI and embeds the `Embarques` form (a shipping label, not stored in the DB) inside `panelTrazabilidad`.
- **Serial format**: `R{yy}{monthLetter}S{consecutivoGlobal:000000}`. Month letters run A–M and skip `I`. The global consecutive is `MAX(consecutivo_global) + 1` read from the DB before each print; it never resets (the `mesAnterior` reset in `PrintDocument1_PrintPage` is dead code, immediately overwritten). Barcodes are text drawn with the **"Libre Barcode 39"** font (`TextBox7.Font`), wrapped in `*...*`, so that font must be installed on the machine.
- **Label layout is hard-coded pixel coordinates** in the `PrintPage` handlers.

### Persistence

Records live in a **SQLite database shared by the VB app and the Python module**, accessed from VB only through `EtiquetasV4/BaseSqlite.vb`:

- `db/schema.sql` is the single source of truth for the schema (`etiquetas`, `usuarios`, `inspecciones`). It is idempotent and embedded in the VB assembly as resource `schema.sql`; `BaseSqlite` applies it on first connection. Schema changes affect both apps: bump `PRAGMA user_version`. Because it only uses `CREATE ... IF NOT EXISTS`, adding a column to an existing table needs a migration step, not just an edit.
- DB location: env var `TRAZA_DB` → else `<repo root>/datos/trazabilidad.db` (found by walking up to `EtiquetasV4.sln`) → else `<exe dir>/datos/trazabilidad.db`. `datos/` is git-ignored. Camera captures go to `capturas/` next to the DB.
- Dates are stored as ISO `yyyy-MM-dd HH:mm:ss`; the printed label still shows `dd/MM/yyyy`. `serial` is `UNIQUE COLLATE NOCASE`.
- `estado` goes `IMPRESA` → `LIBERADA` or `RECHAZADA`, once. `BaseSqlite.Liberar`/`Rechazar` update with `WHERE estado = 'IMPRESA'` and return `ResultadoCierre` (`Hecho`, `NoExiste`, `YaCerrada`). `liberado_por`/`fecha_liberacion` record who/when closed the label in both cases.
- `usuarios`: passwords are PBKDF2-HMAC-SHA256, 100 000 iterations, 16-byte salt, 32-byte hash, both Base64 (`CrearUsuario`, `ValidarUsuario`). Python equivalent: `hashlib.pbkdf2_hmac("sha256", pwd.encode("utf-8"), salt, 100000, 32)`. Changing the parameters invalidates stored passwords.

Station config is still in text files under `EtiquetasV4/Etiquetas/`, copied to the output dir and read from `AppContext.BaseDirectory\Etiquetas\`:

| File | Purpose |
|---|---|
| `Area.txt` | Station area/mode (`PIPING`, `EMBARQUES`, ...) |
| `Spot.txt` | Station spot name |
| `No_borrar_ni_abrir.txt` | Per-station individual label counter |

`PassQA.txt` (plain-text users) is gone and git-ignored; logins use only the `usuarios` table. The original backend was SQL Server table `Quality_print_records`. `System.Data.SqlClient` is still referenced and leftover SQL Server code remains (`ActualizarAreaEnBaseDeDatos`, unused `dbConn*` fields, `Imports` in `Embarques.vb`).

## Repo notes

- `bin/`, `obj/`, `.vs/`, `*.user` and `*.7z` are git-ignored and untracked. In `.gitignore`, keep path patterns anchored with `/` when they could match `EtiquetasV4/Etiquetas/` (Windows paths are case-insensitive).
- Designer files (`*.Designer.vb`, `*.resx`) are generated by the VS WinForms designer. Keep manual edits in them minimal and consistent with what the designer would produce.
- `docs/PLAN.md` records the phase plan and what was actually built.

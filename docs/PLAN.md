# Plan y estado: SQLite + Visión/OCR + Motor de inferencia para EtiquetasV4

## Contexto
Proyecto escolar (Temas Selectos de Sistemas Inteligentes, FIME UANL). La app VB.NET (WinForms, hoy .NET 8) imprime etiquetas de trazabilidad con serial `R{aa}{letraMes}S{000000}`. Originalmente guardaba en SQL Server (`Quality_print_records`) y después, para la demostración, en un `.txt` (`BaseTxt.vb`). Objetivo del proyecto: base SQLite compartida VB/Python, módulo Python que lea la etiqueta (código de barras + OCR), motor de reglas que decida LIBERAR / RECHAZAR / REVISIÓN MANUAL con explicación, e integración con la app.

Supuestos: 2 personas (A: VB.NET/SQLite, B: Python), 2–4 semanas, solo webcam/celular, sin impresora de etiquetas.

## Estado actual (resumen)

| Fase | Estado |
|---|---|
| 1. Base SQLite compartida (`db/schema.sql`, `BaseSqlite.vb`) | ✅ Hecha |
| 3. Visión/OCR (`EtiquetasV4/OCR/verificar_etiqueta.py`) | ✅ Hecha, con Tesseract en lugar de EasyOCR |
| 4. Motor de inferencia (`EtiquetasV4/OCR/motor.py` + `test_motor.py`) | ✅ Hecho |
| Integración con la app VB | ✅ Botón "Verify label (OCR)" que ejecuta el script; resultado en ventana con color |
| Usuarios con contraseña cifrada (tabla `usuarios`) | ✅ Hecho; `PassQA.txt` eliminado |
| Correcciones finales (rama `correcciones-finales`) | ✅ Ver sección "Correcciones" |
| Servicio FastAPI, `DibujoEtiqueta.vb`/exportar PNG, modelo opcional | ❌ No se hicieron (ver "Cambios respecto al plan") |
| Manuales de usuario y técnico (`docs/manual-usuario.md`, `docs/manual-tecnico.md`) | ✅ Hechos; faltan las capturas en `docs/img/` |

## Arquitectura implementada
**Base SQLite compartida + script Python que la app ejecuta como proceso.**
- VB.NET es dueño de la impresión y de escribir `etiquetas` (impresión, liberación, rechazo) y `usuarios`, con `Microsoft.Data.Sqlite`.
- Python es dueño de cámara, visión, OCR e inferencia; lee `etiquetas` e `inspecciones` y escribe solo `inspecciones`. **No cambia `etiquetas.estado`**: la liberación la confirma QA en la app.
- `VerificadorOCR.vb` ejecuta `python OCR\verificar_etiqueta.py (--camara | imagen) <RutaBase> --una-vez` y lee el código de salida: 0 = LIBERAR, 1 = RECHAZAR, 2 = error o sin captura, 3 = REVISIÓN MANUAL. Los `.py` se copian a la carpeta de salida (`OCR\`).
- SQLite en modo WAL para que ambos procesos lean y escriban a la vez.
- Ruta de la BD: variable `TRAZA_DB` → `<raíz del repo>\datos\trazabilidad.db` → `<carpeta del exe>\datos\trazabilidad.db`. Las capturas de cámara se guardan en `capturas\` junto a la base.

### Cambios respecto al plan original
- **Sin FastAPI.** El servicio se planeó para no recargar EasyOCR en cada lectura. Al usar Tesseract, la app ejecuta el script una vez por verificación, sin servicio intermedio.
- **Tesseract en lugar de EasyOCR.** Dependencias: `opencv-python`, `pyzbar`, `pytesseract` + Tesseract OCR para Windows.
- **Sin paquete `vision/`.** El módulo quedó en dos archivos dentro de `EtiquetasV4/OCR/`.
- **La verificación no llena los campos de Release** ni habilita botones: es una consulta independiente cuyo resultado se muestra en una ventana. QA libera o rechaza desde el panel Release.

## Estructura de carpetas
```
trazabilidad-inteligente/
  EtiquetasV4/                 app VB.NET
    Form1.vb                   casi toda la lógica (Trazabilidad y Release)
    BaseSqlite.vb              acceso a datos: etiquetas, estado, usuarios
    VerificadorOCR.vb          ejecuta el script Python y traduce el código de salida
    VentanaResultadoOCR.vb     ventana del resultado (encabezado de color)
    Liberacion.vb / Registro.vb  login y alta de usuarios
    Embarques.vb               etiqueta de embarque (estación EMBARQUES)
    Etiquetas/                 Area.txt, Spot.txt, No_borrar_ni_abrir.txt
    OCR/
      verificar_etiqueta.py    Fase 3: lectura (barras + OCR) y bitácora
      motor.py                 Fase 4: reglas y decisión
      test_motor.py            pruebas del motor (unittest)
  db/schema.sql                única fuente del esquema
  docs/                        este plan y los manuales
  datos/                       trazabilidad.db y capturas/ (en .gitignore)
```

## Esquema de base de datos (`db/schema.sql`, `user_version = 1`)
- **etiquetas**: `serial` UNIQUE NOCASE, consecutivos global e individual, `area`, `spot`, `asset`, `wo`, `item`, `revision` (solo PIPING), `supervisor`, `no_id`, `turno`, `copias`, `fecha_impresion` (ISO), `estado` (`IMPRESA` → `LIBERADA` o `RECHAZADA`, una sola vez), `liberado_por` y `fecha_liberacion` (quién y cuándo cerró la etiqueta, al liberar o al rechazar), `modelo_final`.
- **usuarios**: `usuario` UNIQUE NOCASE, `password_hash`, `salt`, `rol` (`QA`), `creado`. PBKDF2-HMAC-SHA256, 100 000 iteraciones, sal de 16 bytes, hash de 32 bytes, ambos en Base64.
- **inspecciones**: bitácora de cada verificación (fecha, imagen, método, campos leídos, nitidez, decisión, motivos en JSON, `etiqueta_id`). Hoy no se llenan `usuario` ni `confianza_ocr`.

## Módulo de visión (Fase 3) — implementado
1. Variantes de la imagen con OpenCV: gris, ampliada ×2/×3/×5, CLAHE, Otsu y enfoque.
2. Códigos de barras Code 39 con pyzbar sobre todas las variantes. El que parece serial (`R\d{2}[A-Z]S\d{6}`) es el SN; el otro es el M/I.
3. Si el serial existe en la base: OCR con Tesseract (`--psm 6`) hasta encontrar la WO esperada; se extraen también `Rv` y `Fecha`.
4. Nitidez = varianza del Laplaciano.
5. Cámara: al pulsar ESPACIO analiza durante ~8 s y se queda con la mejor lectura; guarda la captura en `capturas\`.

## Motor de inferencia (Fase 4) — implementado
Reglas en orden; cada una devuelve problemas con severidad. Decisión: alguna **crítica** → RECHAZAR; si no, alguna **advertencia** → REVISIÓN MANUAL; si no → LIBERAR (los **avisos** solo informan).

| Código | Severidad | Condición |
|---|---|---|
| ILEGIBLE_FOTO | advertencia | No se leyó el serial y la imagen está borrosa |
| ILEGIBLE_ETIQUETA | advertencia | No se leyó el serial aunque la imagen es nítida |
| FORMATO_INVALIDO | crítica | El serial no cumple el formato o la letra de mes |
| NO_EXISTE | crítica | El serial no está en `etiquetas` |
| YA_LIBERADA / YA_RECHAZADA | crítica | `estado` ≠ IMPRESA |
| DUPLICADO | advertencia | El serial ya tuvo una inspección con decisión LIBERAR |
| SALTO_SECUENCIA | aviso | No existe el consecutivo global anterior |
| SERIAL_FECHA_INCOHERENTE | advertencia | Año o letra del serial no corresponden a la fecha de impresión |
| ITEM_ILEGIBLE | advertencia | No se leyó el M/I ni por barras ni por OCR |
| ITEM_NO_COINCIDE | crítica | M/I leído ≠ `item` |
| WO_ILEGIBLE | advertencia | El OCR no encontró la WO |
| WO_NO_COINCIDE | crítica | WO leída muy distinta de la registrada |
| WO_DUDOSO | advertencia | WO leída parecida pero no igual (posible error de OCR) |
| REVISION_NO_COINCIDE | crítica | Rv leída ≠ `revision` |
| FECHA_INCOHERENTE | aviso | Fecha leída ≠ fecha de impresión |

Umbrales ajustables al inicio de `motor.py`: `UMBRAL_NITIDEZ = 400`, `UMBRAL_SIMILITUD_WO = 0.8`.

## Correcciones (rama `correcciones-finales`)
- ✅ Etiqueta de liberación con el serial que se libera; Rv guardada; fecha tomada al imprimir (Fase 1).
- ✅ **Reprint de Release** reimprime la última liberación de la sesión (antes leía el control equivocado y fallaba) y ya no toca la base.
- ✅ **Liberar solo una vez**: `Print` de Release libera en la base *antes* de imprimir y solo si la etiqueta sigue en IMPRESA; `Search` avisa si ya está liberada o rechazada.
- ✅ **Botón Reject**: marca la etiqueta como RECHAZADA (con confirmación).
- ✅ **La app distingue LIBERAR, RECHAZAR, REVISIÓN MANUAL y error** (antes REVISIÓN MANUAL salía como RECHAZAR).
- ✅ **Login con la tabla `usuarios`** y contraseña cifrada; el primer usuario se crea con Register cuando la tabla está vacía.
- ✅ `bin/`, `obj/`, `.vs/`, `*.user` y `*.7z` fuera del repositorio; `.gitignore` corregido.

## Pendiente / deuda conocida
- **Consecutivo global por estación**: el sistema original usaba una base SQL Server central, así que todas las estaciones compartían el consecutivo. Con una base SQLite por máquina, dos estaciones pueden generar el mismo serial. Para varias estaciones hay que apuntar `TRAZA_DB` a una base común o volver a un servidor.
- El reinicio mensual del consecutivo en `PrintDocument1_PrintPage` es código muerto (nunca tuvo efecto).
- Reprint de Trazabilidad genera un serial y un registro nuevos en cada reimpresión.
- Restos de SQL Server (`ActualizarAreaEnBaseDeDatos`, `dbConn*`, `System.Data.SqlClient`).
- Ideas no realizadas: exportar la etiqueta a PNG (`DibujoEtiqueta.vb`), servicio FastAPI, modelo de legibilidad con scikit-learn, llenar `inspecciones.usuario` y `confianza_ocr`.

## Verificación
- **Motor**: `python -m unittest test_motor -v` desde `EtiquetasV4/OCR`.
- **VB/SQLite**: imprimir etiquetas y revisar `sqlite3 datos/trazabilidad.db "select serial, estado, liberado_por from etiquetas"`.
- **De punta a punta**: imprimir → verificar con cámara o imagen → LIBERAR → liberar en Release → volver a verificar la misma etiqueta → debe dar RECHAZAR con `YA_LIBERADA`.

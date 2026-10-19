# Plan: SQLite + Visión/OCR + Motor de inferencia para EtiquetasV4

## Contexto
Proyecto escolar (Temas Selectos de Sistemas Inteligentes, FIME UANL). La app VB.NET (.NET 6, WinForms) imprime etiquetas con serial `R{aa}{letraMes}S{000000}` y guarda en `.txt` vía `EtiquetasV4/BaseTxt.vb`. Faltan Fase 3 (visión/OCR, atrasada), Fase 4 (motor de inferencia), pruebas y despliegue. Objetivo: base SQLite compartida VB/Python, módulo Python que lea la etiqueta (pyzbar + EasyOCR), motor de reglas que decida LIBERAR/RECHAZAR con explicación, e integración con la pestaña Release usando cámara.

Supuestos (confirmados): 2 personas (A: VB.NET/SQLite, B: Python), 2–4 semanas, solo webcam/celular (sin impresora de etiquetas), EasyOCR.

## Hallazgos del código que afectan el plan
- `BaseTxt.vb` ya es una capa de datos limpia con 4 funciones (`MaxConsecutivoGlobal`, `Insertar`, `BuscarPorSerial`, `Liberar`) y solo 4 llamadas en `Form1.vb` (líneas ~192, 309, 397, 480, 524). Se reemplaza por un módulo SQLite con **las mismas firmas** → cambio mínimo en Form1.
- `Registros.txt` del repo está vacío; la copia en `bin/Debug` tiene 3 registros de prueba (versionados en git). **No se migran**: la base SQLite empieza vacía.
- ✅ **Corregido** — la etiqueta de liberación (`PrintDocument2_PrintPage`) imprimía como código de barras el *último serial generado* (`etiquetaGeneral`); ahora imprime el serial que se libera (`Serial_SQL`).
- ✅ **Corregido** — la revisión "Rv" (TextBox2, área PIPING) se imprimía pero no se guardaba. Se guarda en la columna `revision` de `etiquetas` (solo en PIPING) y se conserva al reimprimir (`tempTextBox2`).
- ✅ **Corregido** — `fechaActual`/`anoActual` eran campos fijados al abrir la app; ahora se calculan en cada `PrintDocument1_PrintPage`, así que serial, fecha impresa y fecha guardada siempre corresponden al momento de impresión.
- `ReimpresionQA_Click` tiene bugs (`Reimpirmir` vs `Reimprimir`, `Item_SQL.Text = tempItem =`).
- "Reimprimir" de trazabilidad pasa por `PrintDocument1_PrintPage` → genera un **serial nuevo** en cada reimpresión (relevante para la regla de duplicados).
- En la etiqueta: código de barras M/I (item) y SN; WO, SV, turno, fecha, # identificación y Rv son **texto** → OCR solo es imprescindible para esos campos; SN e item salen del barcode y el OCR sirve de verificación cruzada.
- Las fechas se guardan como `dd/MM/yyyy` (texto); en SQLite usar ISO `yyyy-MM-dd HH:mm:ss`.
- Sin impresora: el dibujo de la etiqueta está en `PrintPage` con coordenadas fijas. Hay que poder **renderizarla a PNG** (mismo código de dibujo sobre un `Bitmap`) para generar dataset y probar con celular sobre pantalla o impresión en hoja (fuente "Libre Barcode 39" debe estar instalada).

## Arquitectura recomendada
**Base SQLite compartida + servicio local FastAPI en Python.**
- VB.NET sigue siendo dueño de la impresión y de escribir registros (impresión y liberación) con `Microsoft.Data.Sqlite`.
- Python es dueño de cámara, visión, OCR e inferencia; lee la BD y escribe solo la bitácora de inspecciones.
- VB llama a `http://127.0.0.1:8000` con `HttpClient` (incluido en .NET 6, sin paquetes extra).
- Por qué no "llamar un script": EasyOCR tarda varios segundos en cargar el modelo; un servicio lo carga una vez. Además Python con OpenCV maneja la webcam mucho más fácil que WinForms.
- Antes de la API existe un **CLI** (`python -m trazabilidad.cli imagen.jpg`) → entregable temprano de Fase 3 y respaldo si la API se complica.
- SQLite en modo WAL (`PRAGMA journal_mode=WAL`) para lectura/escritura concurrente de ambos procesos.
- Ruta de la BD: `datos/trazabilidad.db` en la raíz del repo, configurable (VB: `My.Settings` o archivo de config; Python: variable de entorno `TRAZA_DB`). Hoy VB usa `AppContext.BaseDirectory` (carpeta bin), que Python no conoce — por eso hay que configurarla.

## Estructura de carpetas
```
trazabilidad-inteligente/
  EtiquetasV4/                 VB.NET (existente)
    BaseSqlite.vb              reemplaza BaseTxt.vb (mismas firmas)
    ClienteVision.vb           HttpClient → API Python
    DibujoEtiqueta.vb          lógica de dibujo extraída de PrintPage (impresora o PNG)
  db/
    schema.sql                 única fuente del esquema (la aplican VB y Python con IF NOT EXISTS)
  vision/                      paquete Python
    requirements.txt           opencv-python, pyzbar, easyocr, fastapi, uvicorn, (scikit-learn opcional), pytest
    trazabilidad/
      db.py                    conexión y consultas
      lectura/                 preprocesamiento.py, barcodes.py, ocr.py, parser.py, camara.py
      inferencia/              reglas.py, motor.py, modelo.py (opcional)
      api.py                   FastAPI
      cli.py
    tests/
      imagenes/                etiquetas sintéticas y fotos reales con su JSON esperado
  datos/                       trazabilidad.db, capturas/ (en .gitignore)
```

## Esquema de base de datos (`db/schema.sql`)
- **etiquetas**: `id` PK, `serial` TEXT UNIQUE, `consecutivo_global`, `consecutivo_individual`, `area`, `spot`, `asset`, `wo`, `item`, `revision` (nuevo), `supervisor`, `no_id`, `turno`, `copias`, `fecha_impresion` (ISO), `estado` ('IMPRESA' | 'LIBERADA' | 'RECHAZADA'), `liberado_por`, `fecha_liberacion`, `modelo_final`. Índices en `serial` y `wo`.
- **usuarios**: `id`, `usuario` UNIQUE, `password_hash`, `salt`, `rol`. Reemplaza `PassQA.txt`. Usar PBKDF2 (VB `Rfc2898DeriveBytes`, Python `hashlib.pbkdf2_hmac`, mismos parámetros).
- **inspecciones** (bitácora del sistema inteligente): `id`, `fecha`, `usuario`, `imagen_ruta`, `metodo` ('barcode' | 'ocr' | 'manual'), campos leídos (`serial_leido`, `item_leido`, `wo_leido`, `revision_leida`, `fecha_leida`), `confianza_ocr`, `nitidez` (varianza del Laplaciano), `decision` ('LIBERAR' | 'RECHAZAR' | 'REVISION_MANUAL'), `motivos` (JSON con lista de códigos y mensajes), `etiqueta_id` FK nullable. Sirve como evidencia en el reporte y como dataset del modelo opcional.
- `PRAGMA user_version` para versionar el esquema.
- `Area.txt`, `Spot.txt` y `No_borrar_ni_abrir.txt` se quedan como configuración local de la estación (fuera de alcance; moverlos después a una tabla `estaciones` si sobra tiempo).

## Módulo de visión (Fase 3)
1. Preprocesamiento (OpenCV): escala de grises, recorte/enderezado opcional, CLAHE, umbral adaptativo; métrica de nitidez para detectar fotos borrosas.
2. Barcodes (pyzbar, Code 39): sobre la imagen original y la preprocesada; probar rotaciones 0/90/180/270. Clasificar cada valor: si cumple `^R\d{2}[A-HJ-M]S\d{6}$` → SN; si no → M/I.
3. OCR (EasyOCR `['es','en']`, reader cargado una sola vez): extraer texto con su confianza.
4. Parser: regex por etiqueta impresa (`WO:`, `SV:`, `Turno:`, `Fecha:`, `Rv:`, `# Identificacion:`); corrección de confusiones típicas en el serial (O↔0, I↔1, S↔5 según posición).
5. Salida: objeto `LecturaEtiqueta` (JSON) con cada campo, su fuente (barcode/ocr) y su confianza.
6. `camara.py`: `cv2.VideoCapture`, lee frames hasta decodificar el SN o hasta un timeout y guarda la mejor captura en `datos/capturas/`.
- Nota Windows: pyzbar trae su DLL, pero puede requerir *Visual C++ 2013 Redistributable*. Verificarlo el día 1 en las máquinas de todos.

## Motor de inferencia (Fase 4)
Sistema experto basado en reglas, con encadenamiento hacia adelante y explicación. Cada regla es una función que recibe (lectura, registro de BD) y devuelve `(código, severidad, mensaje)`. Las reglas se evalúan en orden y la decisión se toma así: cualquier regla crítica → **RECHAZAR**; solo advertencias o baja confianza → **REVISION_MANUAL**; nada → **LIBERAR**.

| Código | Severidad | Condición |
|---|---|---|
| ILEGIBLE | crítica (o revisión si la nitidez es baja) | Sin SN ni por barcode ni por OCR |
| FORMATO_INVALIDO | crítica | El SN no cumple el regex o la letra de mes es inválida |
| SN_BARCODE_VS_OCR | advertencia | El SN del barcode difiere del SN leído por OCR |
| NO_EXISTE | crítica | Serial no está en `etiquetas` |
| DUPLICADO | crítica | El mismo serial ya fue inspeccionado y liberado (etiqueta física repetida) |
| YA_LIBERADO / YA_RECHAZADO | crítica | `estado` ≠ 'IMPRESA' |
| WO_NO_COINCIDE | crítica | WO leído ≠ `wo` en BD |
| ITEM_NO_COINCIDE | crítica | Barcode M/I ≠ `item` en BD |
| REVISION_NO_COINCIDE | crítica (solo PIPING) | Rv leído ≠ `revision` |
| FECHA_INCOHERENTE | advertencia | La fecha de la etiqueta ≠ `fecha_impresion`, o el año/letra del serial no corresponden a la fecha |
| CONFIANZA_BAJA | advertencia | Confianza OCR menor al umbral en un campo usado por una regla |

**Modelo opcional** (solo si hay tiempo, semana 3–4): árbol de decisión (scikit-learn) que prediga si una captura es "legible" o "ilegible" usando como características nitidez, brillo, contraste, número de barcodes decodificados y confianza media del OCR. Entrenarlo con capturas reales y sintéticas aumentadas (blur, rotación, ruido). Se usa antes del OCR para pedir "vuelve a capturar" en lugar de rechazar. Es fácil de explicar en el reporte (se puede mostrar el árbol).

## API (FastAPI)
- `GET /salud`: verifica que el servicio esté vivo y el modelo cargado.
- `POST /analizar`: recibe una imagen (multipart) y devuelve lectura + decisión + motivos.
- `POST /escanear`: captura con la webcam del servidor y devuelve lo mismo más la ruta de la imagen.
- Escribe la fila en `inspecciones`. **No** cambia `etiquetas.estado`: la liberación la confirma el usuario en VB.

## Integración en la pestaña Release (VB)
- Botón "Escanear" junto a `Buscar`: llama a `/escanear` y muestra la captura en un PictureBox.
- Llena `Serial_SQL`, `WorkOrder_SQL` e `Item_SQL` con lo leído y muestra la decisión con color y la lista de motivos.
- Si la decisión es LIBERAR, se habilita `ImprimirQA` (flujo actual, que llama a `BaseSqlite.Liberar` y pone `estado`='LIBERADA'). Si es RECHAZAR, se ofrece "Marcar rechazada". Si es REVISION_MANUAL, el usuario decide.
- La captura manual del serial (`Buscar`) se conserva como respaldo si el servicio no responde.

## Orden de trabajo (2–4 semanas, 2 personas)
Prioridad: tener lo antes posible algo demostrable de la Fase 3, que es la atrasada. El contrato entre ambas personas es `db/schema.sql` (tablas y columnas) y el JSON de la API; cualquier cambio a esos dos se acuerda entre las dos.

- **Persona A — VB.NET / SQLite**: todo lo de `EtiquetasV4/` y `db/`.
- **Persona B — Python**: todo lo de `vision/` (visión, OCR, motor, API).

**Semana 1**
- Persona A:
  - ✅ Rama `fase1-sqlite`: `db/schema.sql` y `BaseSqlite.vb` (reemplaza `BaseTxt.vb`), llamadas de Form1 cambiadas, columna `revision` y fechas ISO.
  - ✅ Corregidos el serial de la etiqueta de liberación, la revisión que no se guardaba y la fecha fija al abrir la app.
  - Extraer el dibujo de la etiqueta a `DibujoEtiqueta.vb` y agregar "Guardar como PNG"; generar las primeras 20–50 etiquetas PNG para B.
  - Corregir `ReimpresionQA_Click`.
- Persona B:
  - Entorno (`requirements.txt`; probar pyzbar y EasyOCR en Windows, incluido el VC++ 2013 redist).
  - `barcodes.py` + `parser.py` + `cli.py` sobre los PNG de A. **Entregable Fase 3 mínimo: CLI que lee SN y M/I por barcode.**
  - Fotos con celular/webcam de esas etiquetas (pantalla y hoja impresa), cada una con su JSON esperado.

**Semana 2**
- Persona B: `ocr.py` (EasyOCR) y `motor.py`/`reglas.py` con pruebas pytest por regla (casos sintéticos sin imágenes). **Entregable Fase 4.** Luego `db.py` y `api.py` (`/salud`, `/analizar`).
- Persona A: tabla `usuarios` con hash (reemplaza `PassQA.txt`), `ClienteVision.vb` contra `/analizar` y UI de Release (botón Escanear, decisión y motivos).

**Semana 3**
- B: `camara.py` + `/escanear`; A: integración de punta a punta en la pestaña Release.
- Ambos: medir la precisión de lectura sobre el dataset (SN, campos OCR y decisiones correctas) y ajustar umbrales.

**Semana 4 (pruebas y despliegue)**
- B: modelo opcional y script de arranque de `uvicorn`.
- A: la app VB avisa si la API no responde; publicar con `FolderProfile`.
- Ambos: README con instalación (Python, fuente Libre Barcode 39, VC++ redist) y reporte de métricas.

Si el tiempo se acorta, lo que se puede recortar sin perder las fases: el modelo opcional, `/escanear` (usar `/analizar` subiendo una foto del celular), y la tabla `usuarios`. Con dos personas, la ruta crítica es la de B; si A termina antes, toma el dataset y las pruebas del motor.

## Verificación
- **VB/SQLite**: imprimir (o guardar PNG) varias etiquetas, revisar las filas con `sqlite3 datos/trazabilidad.db "select * from etiquetas"`, y comprobar que `Buscar` y la liberación funcionan igual que con txt.
- **Visión**: `pytest vision/tests` compara la salida de la CLI contra el JSON esperado de cada imagen; reportar la tasa de aciertos de SN (barcode), M/I y campos OCR.
- **Motor**: una prueba por cada código de la tabla de reglas (serial inexistente, ya liberado, WO alterado, imagen borrosa, etc.).
- **De punta a punta**: con `uvicorn trazabilidad.api:app` corriendo y la app VB abierta, imprimir → PNG → mostrarlo a la webcam → "Escanear" → LIBERAR → imprimir liberación → volver a escanear la misma etiqueta → debe dar YA_LIBERADO.

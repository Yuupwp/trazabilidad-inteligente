# Índice de los manuales

Índice acordado para `docs/manual-usuario.md` y `docs/manual-tecnico.md`, ya actualizado con las correcciones finales (Reject, ventana de resultado OCR con tres decisiones y login con la tabla `usuarios`). Los manuales describen solo lo que está implementado en el código.

## Manual de usuario

Para operador y QA, sin código. Los botones se citan en inglés como aparecen en pantalla y se explican en español.

1. Introducción: para qué sirve la app (imprimir etiquetas de trazabilidad, liberarlas o rechazarlas por QA y verificarlas con cámara o imagen) y quién usa cada parte.
2. Conceptos básicos: partes de la etiqueta (WO, M/I, # Identificación, SV, Turno, Fecha, SN, Rv); cómo leer el serial R26KS000123 (año, letra del mes A–M sin I, consecutivo); estados Impresa, Liberada y Rechazada; tipos de estación normal, PIPING (agrega Revisión) y EMBARQUES.
3. Pantalla principal: barra superior, área de la estación, botones Traceability, Release y Register, cuadro Consecutives (Individual y Global).
4. Imprimir etiquetas (operador): llenar campos paso a paso, Print, Reprint y No. of Reprints (advertir que cada reimpresión genera serial y registro nuevos).
5. Liberación de calidad (QA): iniciar sesión, Search por serial, completar Model final, Released by, Shift y copias, Print (etiqueta "Calidad Liberación", estado Liberada), Reject para marcarla como Rechazada, Reprint en Liberación. Solo se pueden liberar o rechazar etiquetas en estado Impresa.
6. Verificar una etiqueta (Verify label (OCR)): cámara (ESPACIO captura, Q sale) o archivo de imagen; consejos de captura; ventana de resultado con LIBERAR, RECHAZAR y REVISIÓN MANUAL; tabla de mensajes en lenguaje simple y qué hacer; aclarar que verificar no libera, la liberación se hace en Release.
7. Registrar usuarios de QA (Register): requiere un usuario existente; usuario y contraseña de 3 a 8 caracteres.
8. Estación de Embarques: Sales Order, Delivery Number, Ship to Address, Tracking, Carrier, copias, impresión y reimpresión.
9. Mensajes de error frecuentes y qué hacer.
10. Buenas prácticas.
11. Glosario: WO, M/I, SV, Rv, SN, QA, poka-yoke.

## Manual técnico

Instalación y mantenimiento.

1. Descripción general: app VB.NET (WinForms, .NET 8) y módulo Python de visión y reglas, que comparten una base SQLite; diagrama Mermaid.
2. Estructura del repositorio (EtiquetasV4/, OCR/, db/, docs/, Img/).
3. Requisitos: Windows, .NET 8 Desktop Runtime (SDK para desarrollo), fuente Libre Barcode 39, impresora, Python 3 con opencv-python, pyzbar y pytesseract, Tesseract OCR, VC++ Redistributable, webcam.
4. Instalación: desarrollo (clonar, dotnet build, dotnet run), producción (publicación y qué copiar), Python y OCR con su verificación.
5. Configuración de la estación: Area.txt, Spot.txt, No_borrar_ni_abrir.txt, variable TRAZA_DB y orden de búsqueda de la base, y cómo se crea el primer usuario de QA.
6. Arquitectura de la app VB.NET: formularios y paneles; flujo de impresión, generación del serial y consecutivos; BaseSqlite; VerificadorOCR y sus códigos de salida 0 a 3; ventana de resultado.
7. Base de datos: tablas etiquetas, usuarios (contraseñas cifradas) e inspecciones; estados; WAL; user_version; cómo cambiar el esquema.
8. Módulo de visión: preprocesamiento, Code 39, OCR y extracción de campos, nitidez, uso por línea de comandos.
9. Motor de inferencia: tabla de reglas (código, severidad, condición), cómo se decide, umbrales y cómo agregar una regla.
10. Pruebas: dotnet build, python -m unittest test_motor -v y prueba manual de punta a punta.
11. Mantenimiento: respaldos de la base y datos/capturas/, cambiar el formato de la etiqueta, agregar un área, administrar usuarios.
12. Problemas conocidos y trabajo futuro: el consecutivo global es local de cada estación (en producción se necesita una base compartida en red), restos de código de SQL Server si siguen, la cámara no se probó en esta entrega, y cualquier otro que se encuentre en el código.
13. Solución de problemas: no hay Python o Tesseract, la base no existe, no hay cámara, falta la fuente.

## Imágenes

En `docs/img/`. Los manuales llevan el marcador aunque la imagen todavía no exista.

| Archivo | Manual y sección | Contenido |
|---|---|---|
| `01-principal-trazabilidad.png` | Usuario §3 | Pantalla principal vacía |
| `02-trazabilidad-llena.png` | Usuario §4 | Campos llenos antes de Print |
| `03-piping-revision.png` | Usuario §2/§4 | Modo PIPING con campo Revision |
| `04-etiqueta-trazabilidad.jpg` | Usuario §2 | Etiqueta de trazabilidad |
| `05-login-qa.png` | Usuario §5 | Inicio de sesión |
| `06-liberacion-busqueda.png` | Usuario §5 | Release después de Search |
| `07-liberacion-exito.png` | Usuario §5 | Mensaje de liberado con éxito |
| `08-etiqueta-liberacion.jpg` | Usuario §5 | Etiqueta "Calidad Liberación" |
| `09-registro-usuario.png` | Usuario §7 | Ventana Register |
| `10-ocr-pregunta.png` | Usuario §6 | Diálogo para elegir cámara o archivo |
| `11-ocr-camara.png` | Usuario §6 | Ventana de la cámara |
| `12-ocr-liberar.png` | Usuario §6 | Ventana de resultado LIBERAR |
| `13-ocr-rechazar.png` | Usuario §6 | Ventana de resultado RECHAZAR |
| `14-ocr-revision.png` | Usuario §6 | Ventana de resultado REVISIÓN MANUAL |
| `15-embarques.png` | Usuario §8 | Pantalla de Embarques |
| `16-etiqueta-embarques.jpg` | Usuario §8 | Etiqueta de Embarques (opcional) |
| `17-error-campos.png` | Usuario §9 | Error de campos (opcional) |
| `18-rechazo-reject.png` | Usuario §5 | Resultado del botón Reject (nueva, si aplica) |

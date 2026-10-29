"""
Modulo de Percepcion Visual y OCR (Fase 3) - prototipo conectado a la base SQLite compartida.

Valida una etiqueta impresa (imagen o camara) contra la tabla `etiquetas`.
Fase 3 = lectura (barras + OCR) en este archivo. Fase 4 = decision con reglas y heuristicas en motor.py.
Decide:
  LIBERAR  -> la etiqueta coincide con lo registrado.
  RECHAZAR -> hay un dato que no coincide (poka-yoke).
Cada inspeccion se guarda en la tabla `inspecciones` (la bitacora definida en db/schema.sql).

Uso:
  python verificar_etiqueta.py imagen.png  trazabilidad.db [--una-vez]
  python verificar_etiqueta.py --camara    trazabilidad.db [--una-vez]
  (camara: ESPACIO verifica, Q sale.  --una-vez termina tras la primera verificacion)

Decision: LIBERAR (todo coincide) | RECHAZAR (un dato se leyo y no coincide) | REVISION_MANUAL (no se pudo leer bien).
Codigos de salida (los usa la app VB.NET): 0 = LIBERAR, 1 = RECHAZAR, 2 = error / sin captura, 3 = REVISION_MANUAL.

Instalacion:
  pip install opencv-python pyzbar pytesseract
  + Tesseract OCR para Windows (https://github.com/UB-Mannheim/tesseract/wiki)
"""
import json
import os
import re
import shutil
import sqlite3
import sys
import time
from datetime import datetime

import cv2
import pytesseract
from pyzbar.pyzbar import decode

import motor  # Fase 4: motor de inferencia (reglas y heuristicas)

# Tesseract: si no esta en el PATH de Windows, se busca en las carpetas de instalacion por defecto.
if shutil.which("tesseract") is None:
    for _ruta in (r"C:\Program Files\Tesseract-OCR\tesseract.exe",
                  r"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe"):
        if os.path.isfile(_ruta):
            pytesseract.pytesseract.tesseract_cmd = _ruta
            break



# ---------------------------------------------------------------- base de datos
def conectar(ruta_db):
    if not os.path.isfile(ruta_db):
        raise FileNotFoundError(
            f"No existe la base de datos: {ruta_db}\nAbre la app e imprime al menos una etiqueta para crearla.")
    con = sqlite3.connect(ruta_db)
    con.row_factory = sqlite3.Row
    return con


def buscar_etiqueta(con, serial):
    fila = con.execute(
        "SELECT id, serial, consecutivo_global, wo, item, revision, estado, fecha_impresion "
        "FROM etiquetas WHERE serial = ? COLLATE NOCASE", (serial,)).fetchone()
    return dict(fila) if fila else None


def armar_contexto(con, reg):
    """Historial que usan las heuristicas del motor: aprobaciones previas del serial y consecutivo anterior."""
    if reg is None:
        return {}
    try:
        previas = [r["fecha"] for r in con.execute(
            "SELECT fecha FROM inspecciones WHERE etiqueta_id = ? AND decision = 'LIBERAR' ORDER BY id DESC",
            (reg["id"],))]
        n = reg["consecutivo_global"]
        existe = n <= 1 or con.execute(
            "SELECT 1 FROM etiquetas WHERE consecutivo_global = ?", (n - 1,)).fetchone() is not None
        return {"liberaciones_previas": previas, "existe_anterior": existe}
    except sqlite3.Error:
        return {}


def guardar_inspeccion(con, res, imagen_ruta):
    """Escribe la bitacora en `inspecciones`. Si falla, no interrumpe la verificacion."""
    try:
        con.execute(
            "INSERT INTO inspecciones (fecha, imagen_ruta, metodo, serial_leido, item_leido, wo_leido, "
            "revision_leida, nitidez, decision, motivos, etiqueta_id) VALUES (?,?,?,?,?,?,?,?,?,?,?)",
            (datetime.now().strftime("%Y-%m-%d %H:%M:%S"), imagen_ruta,
             "barcode" if res["serial"] else "ocr", res["serial"], res["item"], res["wo_leido"],
             res["rv_leida"], res["nitidez"], res["decision"], json.dumps(res["problemas"], ensure_ascii=False),
             res["etiqueta_id"]))
        con.commit()
    except sqlite3.Error as e:
        print(f"(No se pudo guardar la inspeccion: {e})")


# ---------------------------------------------------------------- vision
def variantes(img):
    """Versiones de la imagen para leer mejor con camara: gris, ampliada, contraste (CLAHE) y binarizada."""
    gris = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img
    g2 = cv2.resize(gris, None, fx=2, fy=2, interpolation=cv2.INTER_CUBIC)
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8)).apply(g2)
    _, otsu = cv2.threshold(g2, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    return [gris, g2, clahe, otsu]


def variantes_barras(img):
    """Mas versiones para los codigos de barras: en camara las barras salen finas y borrosas,
    asi que se prueban ampliaciones con contraste y enfoque."""
    gris, g2, clahe, otsu = variantes(img)
    g3 = cv2.resize(gris, None, fx=3, fy=3, interpolation=cv2.INTER_CUBIC)
    g5 = cv2.resize(gris, None, fx=5, fy=5, interpolation=cv2.INTER_CUBIC)
    clahe3 = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8)).apply(g3)
    clahe5 = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8)).apply(g5)
    nitido2 = cv2.addWeighted(g2, 2.0, cv2.GaussianBlur(g2, (0, 0), 3), -1.0, 0)
    return [gris, g2, clahe, otsu, nitido2, clahe3, clahe5]


def leer_barras(img):
    """Lee los Code 39 probando varias versiones de la imagen y junta los resultados."""
    valores = []
    for v in variantes_barras(img):
        for d in decode(v):
            if d.type == "CODE39":
                txt = d.data.decode()
                if txt not in valores:
                    valores.append(txt)
    return valores


def ocr_textos(img):
    """Generador: texto OCR de cada version de la imagen (se detiene en cuanto el que llama ya no necesita mas)."""
    gris, g2, clahe, otsu = variantes(img)
    for v in (g2, clahe, otsu):
        yield pytesseract.image_to_string(v, config="--psm 6").upper()


def nitidez(img):
    """Varianza del Laplaciano: valores bajos = imagen borrosa."""
    gris = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img
    return float(cv2.Laplacian(gris, cv2.CV_64F).var())


# ---------------------------------------------------------------- lectura + motor de inferencia
def verificar(img, con):
    """Fase 3 (vision) lee la etiqueta; Fase 4 (motor.py) decide con reglas y heuristicas."""
    barras = leer_barras(img)
    serial = next((b for b in barras if motor.PATRON_PARECE_SERIAL.fullmatch(b)), None)
    item_barcode = next((b for b in barras if b != serial), None)
    reg = buscar_etiqueta(con, serial) if serial else None

    lectura = {"serial": serial, "item_barcode": item_barcode, "item_ocr_ok": False, "wo_ok": False,
               "wo_leido": None, "rv_leida": None, "fecha_leida": None, "nitidez": nitidez(img)}
    texto_1a_linea = []

    if reg is not None:
        # OCR: se prueban varias versiones de la imagen hasta encontrar la orden de trabajo.
        # Coincidencia exacta (solo se tolera O/0): un valor casi igual no se acepta, puede ser un error real.
        esperado_wo = motor.normalizar(reg["wo"])
        esperado_item = motor.normalizar(reg["item"])
        for texto in ocr_textos(img):
            if not texto_1a_linea:
                texto_1a_linea = texto.strip().splitlines()[:1]
            m_wo = re.search(r"WO\s*:\s*([A-Z0-9]+)-", texto)
            m_rv = re.search(r"RV\s*[:.]\s*([A-Z0-9]+)", texto)
            m_fe = re.search(r"FECHA\s*:\s*(\d{2}/\d{2}/\d{4})", texto)
            if m_wo and not lectura["wo_leido"]:
                lectura["wo_leido"] = m_wo.group(1)
            if m_rv and not lectura["rv_leida"]:
                lectura["rv_leida"] = m_rv.group(1)
            if m_fe and not lectura["fecha_leida"]:
                lectura["fecha_leida"] = m_fe.group(1)
            norm = motor.normalizar(texto)
            lectura["wo_ok"] = lectura["wo_ok"] or (esperado_wo in norm)
            lectura["item_ocr_ok"] = lectura["item_ocr_ok"] or (esperado_item in norm)
            if lectura["wo_ok"]:
                break

    ev = motor.evaluar(lectura, reg, armar_contexto(con, reg))
    item = item_barcode or (reg["item"] if reg and lectura["item_ocr_ok"] else None)
    return {"serial": serial, "item": item, "wo_leido": lectura["wo_leido"], "rv_leida": lectura["rv_leida"],
            "nitidez": lectura["nitidez"], "etiqueta_id": reg["id"] if reg else None,
            "problemas": ev["problemas"], "texto": texto_1a_linea, "decision": ev["decision"]}


def mostrar(res):
    print(f"Serial (barras): {res['serial']}")
    print(f"Item (barras):   {res['item']}")
    print(f"Texto OCR (1a linea): {res['texto']}")
    print(f"Nitidez: {res['nitidez']:.0f}")
    if res["decision"] == "LIBERAR":
        print("RESULTADO: LIBERAR - etiqueta aprobada")
    elif res["decision"] == "RECHAZAR":
        print("RESULTADO: RECHAZAR  <-- poka-yoke")
    else:
        print("RESULTADO: REVISION MANUAL - no se pudo leer bien o hay algo sospechoso, repite la captura")
    for p in res["problemas"]:
        marca = "(aviso) " if p["severidad"] == "aviso" else ""
        print(f"  - {marca}[{p['codigo']}] {p['mensaje']}")


CODIGOS_SALIDA = {"LIBERAR": 0, "RECHAZAR": 1, "REVISION_MANUAL": 3}


def analizar_con_camara(cam, con, segundos=8):
    """Toma frames durante unos segundos y se queda con la mejor lectura (la camara rara vez sale bien al primer intento)."""
    inicio = time.time()
    ultimo = 0.0
    mejor = mejor_frame = None
    mejor_punt = None
    while time.time() - inicio < segundos:
        ok, frame = cam.read()
        if not ok:
            break
        resto = segundos - (time.time() - inicio)
        vista = frame.copy()
        cv2.putText(vista, f"Analizando... {resto:.0f}s (manten la etiqueta quieta)", (10, 30),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
        cv2.imshow("Camara - etiqueta (ESPACIO = verificar, Q = salir)", vista)
        cv2.waitKey(1)
        if time.time() - ultimo < 0.4:
            continue
        ultimo = time.time()
        res = verificar(frame, con)
        punt = (res["decision"] != "REVISION_MANUAL", bool(res["serial"]) + bool(res["item"]), res["nitidez"])
        if mejor is None or punt > mejor_punt:
            mejor, mejor_frame, mejor_punt = res, frame.copy(), punt
        if res["decision"] != "REVISION_MANUAL":
            break
    return mejor, mejor_frame


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    args = [a for a in sys.argv[1:] if a != "--una-vez"]
    una_vez = "--una-vez" in sys.argv
    if len(args) < 2:
        print(__doc__)
        return 2
    try:
        con = conectar(args[1])
    except Exception as e:
        print(e)
        return 2

    carpeta_capturas = os.path.join(os.path.dirname(os.path.abspath(args[1])), "capturas")

    if args[0] == "--camara":
        cam = cv2.VideoCapture(0)
        if not cam.isOpened():
            print("No se pudo abrir la camara.")
            return 2
        codigo = 2
        while True:
            ok, frame = cam.read()
            if not ok:
                break
            cv2.imshow("Camara - etiqueta (ESPACIO = verificar, Q = salir)", frame)
            tecla = cv2.waitKey(1) & 0xFF
            if tecla == ord(" "):
                res, mejor_frame = analizar_con_camara(cam, con)
                if res is None:
                    continue
                os.makedirs(carpeta_capturas, exist_ok=True)
                ruta_img = os.path.join(carpeta_capturas, datetime.now().strftime("%Y%m%d_%H%M%S") + ".png")
                cv2.imwrite(ruta_img, mejor_frame)
                mostrar(res)
                guardar_inspeccion(con, res, ruta_img)
                codigo = CODIGOS_SALIDA[res["decision"]]
                if una_vez:
                    break
            elif tecla in (ord("q"), ord("Q")):
                if una_vez and codigo == 2:
                    print("No se capturo ninguna etiqueta.")
                break
        cam.release()
        cv2.destroyAllWindows()
        con.close()
        return codigo

    img = cv2.imread(args[0])
    if img is None:
        print("No se pudo abrir la imagen.")
        return 2
    res = verificar(img, con)
    mostrar(res)
    guardar_inspeccion(con, res, os.path.abspath(args[0]))
    con.close()
    return CODIGOS_SALIDA[res["decision"]]


if __name__ == "__main__":
    try:
        sys.exit(main())
    except pytesseract.TesseractNotFoundError:
        print("ERROR: no se encontro Tesseract OCR. Instalalo desde https://github.com/UB-Mannheim/tesseract/wiki "
              "(ruta por defecto: C:\\Program Files\\Tesseract-OCR) o agregalo al PATH de Windows.")
        sys.exit(2)
    except Exception as e:  # cualquier otro fallo: error (2), nunca se confunde con RECHAZAR (1)
        print(f"ERROR inesperado en el modulo de OCR: {type(e).__name__}: {e}")
        sys.exit(2)

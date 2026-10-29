"""
Motor de inferencia (Fase 4): sistema experto basado en reglas.

Recibe tres cosas, ya preparadas por el modulo de vision (Fase 3):
  lectura  -> lo que se leyo de la etiqueta (serial, item, WO, revision, fecha, nitidez ...)
  registro -> la fila de la tabla `etiquetas` con lo que SE IMPRIMIO (o None si no existe)
  contexto -> historial de la base (inspecciones previas, consecutivo anterior)
y devuelve una decision EXPLICADA: LIBERAR, RECHAZAR o REVISION_MANUAL, con la lista de motivos.

Cada regla es una funcion pequena  regla(lectura, registro, contexto) -> [problema, ...]  y se evalua en orden
(encadenamiento hacia adelante). La severidad de cada problema decide la decision final:

  critica      -> RECHAZAR         el dato se leyo y NO coincide: la etiqueta es incorrecta (poka-yoke)
  advertencia  -> REVISION_MANUAL  no se pudo leer bien, o algo es sospechoso: que decida una persona
  aviso        -> no cambia la decision; solo queda anotado (heuristicas informativas)

Este archivo no depende de OpenCV ni de la base de datos, asi que se puede probar solo (ver test_motor.py).
"""
import re
from datetime import datetime
from difflib import SequenceMatcher

# ------------------------------------------------------------------ constantes (ajustables)
LETRAS_MES = "ABCDEFGHJKLM"          # enero..diciembre; se salta la I (igual que la app)
PATRON_SERIAL = re.compile(r"R(\d{2})([A-HJKLM])S(\d{6})")         # serial valido
PATRON_PARECE_SERIAL = re.compile(r"R\d{2}[A-Z]S\d{6}")            # "parece" serial (aunque la letra no sea valida)
UMBRAL_NITIDEZ = 400.0               # varianza del Laplaciano; por debajo = foto borrosa (ajustar con capturas reales)
UMBRAL_SIMILITUD_WO = 0.8            # WO leido parecido al esperado (probable error de lectura, no etiqueta distinta)


# ------------------------------------------------------------------ utilidades
def letra_mes(mes):
    return LETRAS_MES[mes - 1]


def normalizar(texto):
    """El OCR confunde O/0, asi que se comparan con ambas letras unificadas."""
    return re.sub(r"[^A-Z0-9-]", "", texto.upper()).replace("O", "0")


def similitud(a, b):
    return SequenceMatcher(None, a, b).ratio()


def problema(codigo, mensaje, severidad="critica"):
    return {"codigo": codigo, "mensaje": mensaje, "severidad": severidad}


def _fecha_registro(registro):
    """fecha_impresion viene como 'yyyy-MM-dd HH:mm:ss'."""
    try:
        return datetime.strptime(registro["fecha_impresion"][:10], "%Y-%m-%d")
    except (TypeError, ValueError, KeyError):
        return None


# ------------------------------------------------------------------ reglas
def regla_ilegible(lectura, registro, contexto):
    """Sin serial: distingue foto mala (borrosa) de etiqueta mala (imagen nitida pero sin serial legible)."""
    if lectura.get("serial"):
        return []
    if lectura["nitidez"] < UMBRAL_NITIDEZ:
        return [problema("ILEGIBLE_FOTO",
                         f"La imagen esta borrosa (nitidez {lectura['nitidez']:.0f}). "
                         "Repite la captura con mas luz y la etiqueta quieta.", "advertencia")]
    return [problema("ILEGIBLE_ETIQUETA",
                     "La imagen es nitida pero no se pudo leer el serial: puede ser una etiqueta mal impresa "
                     "o muy lejos de la camara. Revisala a simple vista.", "advertencia")]


def regla_formato(lectura, registro, contexto):
    """El serial debe ser R + aa + letra de mes valida (A-M sin I) + S + 6 digitos."""
    serial = lectura.get("serial")
    if serial and not PATRON_SERIAL.fullmatch(serial):
        return [problema("FORMATO_INVALIDO",
                         f"El serial {serial} no tiene un formato valido (R + año + letra de mes A-M sin I + S + 6 digitos).")]
    return []


def regla_existe(lectura, registro, contexto):
    serial = lectura.get("serial")
    if serial and PATRON_SERIAL.fullmatch(serial) and registro is None:
        return [problema("NO_EXISTE", f"El serial {serial} no existe en la base de datos.")]
    return []


def regla_estado(lectura, registro, contexto):
    if registro and registro["estado"] != "IMPRESA":
        return [problema("YA_" + registro["estado"],
                         f"La etiqueta ya fue procesada (estado: {registro['estado']}).")]
    return []


def regla_duplicado(lectura, registro, contexto):
    """Heuristica: el mismo serial ya fue verificado y aprobado antes -> posible etiqueta fisica repetida."""
    previas = contexto.get("liberaciones_previas", [])
    if registro and previas:
        return [problema("DUPLICADO",
                         f"Este serial ya fue verificado y aprobado antes ({previas[0]}). "
                         "Puede ser una etiqueta repetida: confirma a simple vista.", "advertencia")]
    return []


def regla_secuencia(lectura, registro, contexto):
    """Heuristica: si existe el consecutivo N, debe existir el N-1 (un hueco indica una etiqueta perdida o no registrada)."""
    if registro and registro["consecutivo_global"] > 1 and not contexto.get("existe_anterior", True):
        n = registro["consecutivo_global"]
        return [problema("SALTO_SECUENCIA",
                         f"Falta el consecutivo {n - 1:06d} antes de este ({n:06d}): puede haberse perdido una etiqueta.",
                         "aviso")]
    return []


def regla_serial_fecha(lectura, registro, contexto):
    """El año y la letra del mes del serial deben corresponder a la fecha en que se imprimio."""
    if not registro:
        return []
    m = PATRON_SERIAL.fullmatch(registro["serial"])
    fecha = _fecha_registro(registro)
    if not m or fecha is None:
        return []
    if int(m.group(1)) != fecha.year % 100 or m.group(2) != letra_mes(fecha.month):
        return [problema("SERIAL_FECHA_INCOHERENTE",
                         f"El serial {registro['serial']} no corresponde a la fecha de impresion registrada "
                         f"({fecha:%d/%m/%Y}).", "advertencia")]
    return []


def regla_item(lectura, registro, contexto):
    if not registro:
        return []
    item = lectura.get("item_barcode")
    if item is None:
        if lectura.get("item_ocr_ok"):      # el codigo no se leyo, pero el texto impreso lo confirma exacto
            return []
        return [problema("ITEM_ILEGIBLE", "No se pudo leer el item (M/I) de la etiqueta.", "advertencia")]
    if item != registro["item"]:
        return [problema("ITEM_NO_COINCIDE",
                         f"El item de la etiqueta ({item}) no coincide con el registrado ({registro['item']}).")]
    return []


def regla_wo(lectura, registro, contexto):
    """Coincidencia exacta; un valor casi igual no se acepta (puede ser un error real) -> revision manual."""
    if not registro or lectura.get("wo_ok"):
        return []
    leido = lectura.get("wo_leido")
    if not leido:
        return [problema("WO_ILEGIBLE", "No se pudo leer el texto de la orden de trabajo (OCR).", "advertencia")]
    if similitud(normalizar(leido), normalizar(registro["wo"])) < UMBRAL_SIMILITUD_WO:
        return [problema("WO_NO_COINCIDE",
                         f"El WO impreso ({leido}) no coincide con el registrado ({registro['wo']}).")]
    return [problema("WO_DUDOSO",
                     f"Se leyo WO {leido} y se esperaba {registro['wo']}: puede ser un error de lectura "
                     "o una etiqueta incorrecta. Verificalo a simple vista.", "advertencia")]


def regla_revision(lectura, registro, contexto):
    if registro and registro["revision"] and lectura.get("rv_leida") \
            and lectura["rv_leida"].upper() != registro["revision"].upper():
        return [problema("REVISION_NO_COINCIDE",
                         f"La revision impresa ({lectura['rv_leida']}) no coincide con la registrada ({registro['revision']}).")]
    return []


def regla_fecha_leida(lectura, registro, contexto):
    """Aviso informativo: el OCR de digitos es poco fiable, por eso no bloquea."""
    fecha = _fecha_registro(registro) if registro else None
    leida = lectura.get("fecha_leida")
    if fecha and leida and leida != f"{fecha:%d/%m/%Y}":
        return [problema("FECHA_INCOHERENTE",
                         f"La fecha leida en la etiqueta ({leida}) difiere de la registrada ({fecha:%d/%m/%Y}).", "aviso")]
    return []


# Orden de evaluacion
REGLAS = [
    regla_ilegible,
    regla_formato,
    regla_existe,
    regla_estado,
    regla_duplicado,
    regla_secuencia,
    regla_serial_fecha,
    regla_item,
    regla_wo,
    regla_revision,
    regla_fecha_leida,
]


# ------------------------------------------------------------------ decision
def decidir(problemas):
    if any(p["severidad"] == "critica" for p in problemas):
        return "RECHAZAR"
    if any(p["severidad"] == "advertencia" for p in problemas):
        return "REVISION_MANUAL"
    return "LIBERAR"


def evaluar(lectura, registro, contexto=None):
    """Evalua todas las reglas y devuelve {'decision': ..., 'problemas': [...]}."""
    contexto = contexto or {}
    problemas = []
    for regla in REGLAS:
        problemas.extend(regla(lectura, registro, contexto))
    return {"decision": decidir(problemas), "problemas": problemas}

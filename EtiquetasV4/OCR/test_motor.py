"""
Pruebas del motor de inferencia (Fase 4). No necesitan camara, imagenes ni base de datos.

Ejecutar (desde la carpeta OCR):
    python test_motor.py          o          python -m unittest test_motor -v
"""
import unittest

import motor

# Registro tal como esta en la tabla `etiquetas` (lo que se imprimio)
REG = {"serial": "R26KS000002", "consecutivo_global": 2, "wo": "WO100300", "item": "VALV-4IN-B12",
       "revision": "B", "estado": "IMPRESA", "fecha_impresion": "2026-10-27 10:00:00"}


def lectura(**cambios):
    """Lectura 'perfecta' de la etiqueta del registro REG; cada prueba cambia solo lo que necesita."""
    base = {"serial": "R26KS000002", "item_barcode": "VALV-4IN-B12", "item_ocr_ok": True,
            "wo_ok": True, "wo_leido": "WO100300", "rv_leida": "B", "fecha_leida": "27/10/2026", "nitidez": 2000.0}
    base.update(cambios)
    return base


def registro(**cambios):
    r = dict(REG)
    r.update(cambios)
    return r


CTX_OK = {"liberaciones_previas": [], "existe_anterior": True}


def codigos(res):
    return [p["codigo"] for p in res["problemas"]]


class PruebasMotor(unittest.TestCase):

    def test_todo_coincide_libera(self):
        res = motor.evaluar(lectura(), registro(), CTX_OK)
        self.assertEqual(res["decision"], "LIBERAR")
        self.assertEqual(res["problemas"], [])

    # ---- lectura ----
    def test_sin_serial_foto_borrosa(self):
        res = motor.evaluar(lectura(serial=None, nitidez=100.0), None, CTX_OK)
        self.assertEqual(res["decision"], "REVISION_MANUAL")
        self.assertEqual(codigos(res), ["ILEGIBLE_FOTO"])

    def test_sin_serial_imagen_nitida_es_etiqueta_mala(self):
        res = motor.evaluar(lectura(serial=None, nitidez=3000.0), None, CTX_OK)
        self.assertEqual(res["decision"], "REVISION_MANUAL")
        self.assertEqual(codigos(res), ["ILEGIBLE_ETIQUETA"])

    # ---- formato / existencia / estado ----
    def test_formato_invalido_letra_de_mes(self):
        res = motor.evaluar(lectura(serial="R26IS000002"), None, CTX_OK)     # la I no existe
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertEqual(codigos(res), ["FORMATO_INVALIDO"])                  # sin NO_EXISTE duplicado

    def test_serial_inexistente(self):
        res = motor.evaluar(lectura(), None, CTX_OK)
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertEqual(codigos(res), ["NO_EXISTE"])

    def test_ya_liberada(self):
        res = motor.evaluar(lectura(), registro(estado="LIBERADA"), CTX_OK)
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertIn("YA_LIBERADA", codigos(res))

    def test_ya_rechazada(self):
        res = motor.evaluar(lectura(), registro(estado="RECHAZADA"), CTX_OK)
        self.assertIn("YA_RECHAZADA", codigos(res))

    # ---- heuristicas ----
    def test_duplicado_pide_revision(self):
        ctx = {"liberaciones_previas": ["2026-10-27 11:00:00"], "existe_anterior": True}
        res = motor.evaluar(lectura(), registro(), ctx)
        self.assertEqual(res["decision"], "REVISION_MANUAL")
        self.assertEqual(codigos(res), ["DUPLICADO"])

    def test_salto_de_secuencia_solo_avisa(self):
        ctx = {"liberaciones_previas": [], "existe_anterior": False}
        res = motor.evaluar(lectura(), registro(), ctx)
        self.assertEqual(res["decision"], "LIBERAR")                          # no bloquea
        self.assertEqual(codigos(res), ["SALTO_SECUENCIA"])

    def test_primer_consecutivo_no_es_salto(self):
        ctx = {"liberaciones_previas": [], "existe_anterior": False}
        res = motor.evaluar(lectura(serial="R26KS000001"), registro(serial="R26KS000001", consecutivo_global=1), ctx)
        self.assertEqual(res["problemas"], [])

    def test_serial_no_corresponde_a_la_fecha(self):
        res = motor.evaluar(lectura(), registro(fecha_impresion="2026-03-05 09:00:00"), CTX_OK)   # K = octubre
        self.assertEqual(res["decision"], "REVISION_MANUAL")
        self.assertIn("SERIAL_FECHA_INCOHERENTE", codigos(res))

    def test_letras_de_mes(self):
        self.assertEqual([motor.letra_mes(m) for m in range(1, 13)], list("ABCDEFGHJKLM"))

    # ---- item ----
    def test_item_distinto_rechaza(self):
        res = motor.evaluar(lectura(item_barcode="VALV-8IN-C99"), registro(), CTX_OK)
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertEqual(codigos(res), ["ITEM_NO_COINCIDE"])

    def test_item_sin_codigo_pero_confirmado_por_ocr(self):
        res = motor.evaluar(lectura(item_barcode=None, item_ocr_ok=True), registro(), CTX_OK)
        self.assertEqual(res["decision"], "LIBERAR")

    def test_item_ilegible_pide_revision(self):
        res = motor.evaluar(lectura(item_barcode=None, item_ocr_ok=False), registro(), CTX_OK)
        self.assertEqual(res["decision"], "REVISION_MANUAL")
        self.assertEqual(codigos(res), ["ITEM_ILEGIBLE"])

    # ---- WO ----
    def test_wo_muy_distinto_rechaza(self):
        res = motor.evaluar(lectura(wo_ok=False, wo_leido="WO999999"), registro(), CTX_OK)
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertEqual(codigos(res), ["WO_NO_COINCIDE"])

    def test_wo_casi_igual_no_pasa_pero_no_rechaza(self):
        res = motor.evaluar(lectura(wo_ok=False, wo_leido="WO100301"), registro(), CTX_OK)
        self.assertEqual(res["decision"], "REVISION_MANUAL")
        self.assertEqual(codigos(res), ["WO_DUDOSO"])

    def test_wo_sin_leer(self):
        res = motor.evaluar(lectura(wo_ok=False, wo_leido=None), registro(), CTX_OK)
        self.assertEqual(codigos(res), ["WO_ILEGIBLE"])

    # ---- revision y fecha ----
    def test_revision_distinta_rechaza(self):
        res = motor.evaluar(lectura(rv_leida="C"), registro(), CTX_OK)
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertEqual(codigos(res), ["REVISION_NO_COINCIDE"])

    def test_revision_no_leida_no_bloquea(self):
        res = motor.evaluar(lectura(rv_leida=None), registro(), CTX_OK)
        self.assertEqual(res["decision"], "LIBERAR")

    def test_fecha_leida_distinta_solo_avisa(self):
        res = motor.evaluar(lectura(fecha_leida="27/10/2025"), registro(), CTX_OK)
        self.assertEqual(res["decision"], "LIBERAR")
        self.assertEqual(codigos(res), ["FECHA_INCOHERENTE"])

    # ---- combinaciones ----
    def test_critica_gana_sobre_advertencia(self):
        res = motor.evaluar(lectura(item_barcode="VALV-8IN-C99", wo_ok=False, wo_leido=None), registro(), CTX_OK)
        self.assertEqual(res["decision"], "RECHAZAR")
        self.assertEqual(sorted(codigos(res)), ["ITEM_NO_COINCIDE", "WO_ILEGIBLE"])


if __name__ == "__main__":
    unittest.main(verbosity=2)

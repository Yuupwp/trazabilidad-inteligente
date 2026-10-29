Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Text

' Conecta la app con el modulo de Percepcion Visual y OCR (OCR\verificar_etiqueta.py).
' Ejecuta el script de Python sobre la base SQLite compartida (BaseSqlite.RutaBase),
' espera su resultado y lo regresa a la app.
Public Module VerificadorOCR

    Public Class ResultadoOCR
        Public Property Aprobada As Boolean
        Public Property Detalle As String
    End Class

    Public Function Verificar(usarCamara As Boolean, Optional rutaImagen As String = "") As ResultadoOCR
        Dim script As String = Path.Combine(AppContext.BaseDirectory, "OCR", "verificar_etiqueta.py")

        If Not File.Exists(script) Then
            Return New ResultadoOCR With {.Aprobada = False, .Detalle = "No se encontró el script: " & script}
        End If

        Dim fuente As String = If(usarCamara, "--camara", """" & rutaImagen & """")
        Dim psi As New ProcessStartInfo("python", """" & script & """ " & fuente & " """ & BaseSqlite.RutaBase & """ --una-vez") With {
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True,
            .StandardOutputEncoding = Encoding.UTF8,
            .StandardErrorEncoding = Encoding.UTF8
        }
        psi.Environment("PYTHONIOENCODING") = "utf-8"

        Try
            Using p As Process = Process.Start(psi)
                Dim tareaError = p.StandardError.ReadToEndAsync()
                Dim salida As String = p.StandardOutput.ReadToEnd()
                p.WaitForExit()
                Dim errores As String = tareaError.Result

                Dim detalle As String = salida.Trim()
                If p.ExitCode = 2 AndAlso errores.Trim() <> "" Then
                    detalle &= vbCrLf & errores.Trim()
                End If
                If detalle = "" Then detalle = "El módulo de OCR no devolvió resultado." & vbCrLf & errores.Trim()

                ' Código de salida: 0 = LIBERAR, 1 = RECHAZAR, 2 = error
                Return New ResultadoOCR With {.Aprobada = (p.ExitCode = 0), .Detalle = detalle}
            End Using
        Catch ex As Win32Exception
            Return New ResultadoOCR With {.Aprobada = False,
                .Detalle = "No se encontró Python. Instálalo y agrégalo al PATH, y ejecuta: pip install opencv-python pyzbar pytesseract"}
        Catch ex As Exception
            Return New ResultadoOCR With {.Aprobada = False, .Detalle = "Error al ejecutar el módulo de OCR: " & ex.Message}
        End Try
    End Function

End Module

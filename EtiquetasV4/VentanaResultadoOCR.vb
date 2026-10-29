' Ventana con el resultado de la verificación OCR: encabezado de color según la decisión y el detalle del motor.
' Se construye por código (sin diseñador) porque solo tiene tres controles.
Public Class VentanaResultadoOCR
    Inherits Form

    Public Sub New(resultado As VerificadorOCR.ResultadoOCR)
        Dim titulo As String
        Dim fondo As Color
        Dim texto As Color = Color.White

        Select Case resultado.Decision
            Case VerificadorOCR.DecisionOCR.Liberar
                titulo = "LIBERAR - etiqueta aprobada"
                fondo = Color.FromArgb(46, 125, 50)       ' verde
            Case VerificadorOCR.DecisionOCR.Rechazar
                titulo = "RECHAZAR - etiqueta no válida"
                fondo = Color.FromArgb(198, 40, 40)       ' rojo
            Case VerificadorOCR.DecisionOCR.RevisionManual
                titulo = "REVISIÓN MANUAL - revisa la etiqueta a simple vista"
                fondo = Color.FromArgb(255, 179, 0)       ' ámbar
                texto = Color.Black
            Case Else
                titulo = "ERROR - no se pudo verificar"
                fondo = Color.FromArgb(97, 97, 97)        ' gris
        End Select

        Text = "Verificar etiqueta (OCR)"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ClientSize = New Size(560, 340)

        Dim encabezado As New Label With {
            .Text = titulo,
            .Dock = DockStyle.Top,
            .Height = 56,
            .BackColor = fondo,
            .ForeColor = texto,
            .Font = New Font("Segoe UI Semibold", 13.0F, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        Dim detalle As New TextBox With {
            .Text = resultado.Detalle,
            .Multiline = True,
            .ReadOnly = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Consolas", 9.5F),
            .Location = New Point(12, 68),
            .Size = New Size(536, 216),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }

        Dim aceptar As New Button With {
            .Text = "Aceptar",
            .DialogResult = DialogResult.OK,
            .Size = New Size(96, 32),
            .Location = New Point(452, 296),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        }

        Controls.Add(detalle)
        Controls.Add(encabezado)
        Controls.Add(aceptar)
        AcceptButton = aceptar
        CancelButton = aceptar
    End Sub

End Class

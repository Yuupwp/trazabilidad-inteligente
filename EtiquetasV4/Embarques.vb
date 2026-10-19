Imports System.Data.SqlClient
Imports System.Drawing.Printing
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Embarques

    Dim tempSO As String
    Dim tempDN As String
    Dim tempSA As String
    Dim tempTracking As String
    Dim tempCarrier As String
    Dim tempCantidad As String


    Public Property DatosImpresion As Dictionary(Of String, String)
    Private Sub Embarques_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Fuente.Visible = False
        Barras.Visible = False
        fuenteParrafo.Visible = False
        fuenteAddress.Visible = False
    End Sub
    Private Sub PrintDocument1_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintDocument3.PrintPage
        e.Graphics.DrawString("Exportation", Fuente.Font, Brushes.Black, 40, 0)
        e.Graphics.DrawString("Sales Order", Fuente.Font, Brushes.Black, 40, 0)
        e.Graphics.DrawString(SO.Text, fuenteParrafo.Font, Brushes.Black, 40, 56)
        e.Graphics.DrawString("*" & SO.Text & "*", Barras.Font, Brushes.Black, 230, 65)
        e.Graphics.DrawString("Delivery Number", Fuente.Font, Brushes.Black, 40, 130)
        e.Graphics.DrawString(DN.Text, fuenteParrafo.Font, Brushes.Black, 40, 186)
        e.Graphics.DrawString("*" & DN.Text & "*", Barras.Font, Brushes.Black, 230, 195)
        e.Graphics.DrawString("Ship to Address", Fuente.Font, Brushes.Black, 40, 270)

        ' Divide el texto del campo "Ship to Address" en líneas si excede 20 caracteres por línea
        Dim lineasSA() As String = DividirTexto(SA.Text, 33)
        Dim y As Integer = 320
        For Each linea As String In lineasSA
            e.Graphics.DrawString(linea, fuenteAddress.Font, Brushes.Black, 40, y)
            y += fuenteAddress.Height + 15 ' Incrementa la posición Y para la siguiente línea
        Next

        e.Graphics.DrawString("Tracking: ", Fuente.Font, Brushes.Black, 40, y + 30)
        e.Graphics.DrawString(Tracking.Text, fuenteParrafo.Font, Brushes.Black, 40, y + 85)
        e.Graphics.DrawString("*" & Tracking.Text & "*", Barras.Font, Brushes.Black, 230, y + 95)
        e.Graphics.DrawString("Carrier: ", Fuente.Font, Brushes.Black, 40, y + 140)
        e.Graphics.DrawString(Carrier.Text, fuenteParrafo.Font, Brushes.Black, 40, y + 190)
        e.Graphics.DrawString("*" & Carrier.Text & "*", Barras.Font, Brushes.Black, 230, y + 190)
    End Sub


    ' Función para dividir el texto en líneas
    Private Function DividirTexto(texto As String, caracteresPorLinea As Integer) As String()
        Dim lineas As New List(Of String)()
        Dim startIndex As Integer = 0

        While startIndex < texto.Length
            Dim endIndex As Integer = Math.Min(startIndex + caracteresPorLinea, texto.Length)
            Dim linea As String = texto.Substring(startIndex, endIndex - startIndex)
            lineas.Add(linea)
            startIndex += caracteresPorLinea
        End While

        Return lineas.ToArray()
    End Function


    'e.Graphics.DrawString("*" & "R" & anoActual.ToString().Substring(2) & letra & "S" & etiquetaGeneral.ToString("000000") & "*", TextBox7.Font, Brushes.Black, 80, 101)

    Private Sub Imprimir_Click(sender As Object, e As EventArgs) Handles Imprimir.Click
        ' Verifica si todos los campos necesarios están completos
        If SO.Text <> "" AndAlso DN.Text <> "" AndAlso SA.Text <> "" AndAlso Tracking.Text <> "" AndAlso Carrier.Text <> "" Then
            ' Verifica si la cantidad ingresada es un número válido
            Dim numberOfCopies As Integer
            If Not Integer.TryParse(Cantidad.Text, numberOfCopies) Then
                MessageBox.Show("Ingrese un número válido para la cantidad.", "Error")
                Return
            End If

            ' Guarda los valores actuales en variables temporales
            tempSO = SO.Text
            tempDN = DN.Text
            tempSA = SA.Text
            tempTracking = Tracking.Text
            tempCarrier = Carrier.Text
            tempCantidad = Cantidad.Text

            ' Realiza la impresión del documento
            For k As Integer = 1 To numberOfCopies
                PrintPreviewDialog3.Document = PrintDocument3
                PrintDocument3.PrinterSettings = PrintDialog3.PrinterSettings
                PrintDocument3.Print()
            Next

            ' Limpia los campos después de la impresión
            SO.Clear()
            DN.Clear()
            SA.Clear()
            Tracking.Clear()
            Cantidad.Clear()
            Carrier.Clear()
        Else
            MsgBox("Complete todos los campos.")
        End If
    End Sub

    Private Sub Reimprimir_Click(sender As Object, e As EventArgs) Handles Reimprimir.Click
        ' Verifica si todos los campos necesarios están completos
        If TextBox1.Text <> "" Then
            ' Verifica si la cantidad ingresada es un número válido
            Dim numberOfCopies As Integer
            If Not Integer.TryParse(TextBox1.Text, numberOfCopies) Then
                MessageBox.Show("Ingrese un número válido para la cantidad.", "Error")
                Return
            End If


            ' Utiliza los valores almacenados en las variables temporales para la impresión
            SO.Text = tempSO
            DN.Text = tempDN
            SA.Text = tempSA
            Tracking.Text = tempTracking
            Carrier.Text = tempCarrier

            ' Realiza la impresión del documento
            For k As Integer = 1 To numberOfCopies
                PrintPreviewDialog3.Document = PrintDocument3
                PrintDocument3.PrinterSettings = PrintDialog3.PrinterSettings
                PrintDocument3.Print()
            Next

            ' Limpia los campos después de la impresión
            SO.Clear()
            DN.Clear()
            SA.Clear()
            Tracking.Clear()
            TextBox1.Clear()
            Carrier.Clear()
        Else
            MsgBox("Complete todos los campos.")
        End If
    End Sub

End Class
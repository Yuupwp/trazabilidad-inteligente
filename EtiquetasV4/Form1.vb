Imports System.Runtime.InteropServices
Imports System.Data.SqlClient
Imports System.Drawing.Printing
Imports System.IO
Imports System.Reflection.Emit
Imports System.Security.Cryptography
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports Microsoft.SqlServer
Imports Microsoft.VisualBasic.FileIO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Devices

Public Class Form1

    Dim dbConn11 As SqlClient.SqlConnection
    Dim dbConn12 As SqlClient.SqlConnection
    Dim adapter2 As SqlClient.SqlDataAdapter
    Dim ds2 As DataSet
    Dim dbConn13 As SqlClient.SqlConnection
    Dim dbConn14 As SqlClient.SqlConnection
    Dim etiquetaActual As Integer = 0
    Dim etiquetaGeneral As Integer = 0 ' Consecutivo general
    Private rutaArchivo As String
    Dim spot As String = ""
    Dim systemName As String = System.Environment.MachineName
    Dim etiquetaAnterior As Integer = 0
    Dim areaAnterior As String = ""
    Dim tempTextBox1 As String
    Dim tempTextBox2 As String
    Dim tempTextBox4 As String
    Dim tempTextBox5 As String
    Dim tempTextBox7 As String
    Dim tempTextBox11 As String
    Dim tempItem As String
    Dim tempLiberado As String
    Dim tempWorkOrder As String
    Dim tempSecuencia As String
    Dim tempCantidad As String
    Dim tempSerial As String
    Dim drag As Boolean
    Dim mouseX As Integer
    Dim mouseY As Integer
    Dim wo As String
    Dim item As String
    Dim secuencia As String
    Dim letra As String = ""
    Private ultimoBotonClicado As System.Windows.Forms.Button = Nothing

    Public Sub New()
        InitializeComponent()
        AddHandler Me.Load, AddressOf Form1_Load
    End Sub
    ' Importar funciones de la API de Windows para el dibujo
    <DllImport("Gdi32.dll", EntryPoint:="CreateRoundRectRgn")>
    Private Shared Function CreateRoundRectRgn(ByVal nLeftRect As Integer, ByVal nTopRect As Integer, ByVal nRightRect As Integer, ByVal nBottomRect As Integer, ByVal nWidthEllipse As Integer, ByVal nHeightEllipse As Integer) As IntPtr
    End Function

    ' Método para redondear los bordes del formulario
    Private Sub RedondearBordesFormulario()
        ' Definir los parámetros para crear una región redondeada
        Dim radius As Integer = 25 ' El radio de redondez, ajusta según tus necesidades
        Dim regionHandle As IntPtr = CreateRoundRectRgn(0, 0, Me.Width, Me.Height, radius, radius)

        ' Aplicar la región redondeada al formulario
        Me.Region = Region.FromHrgn(regionHandle)
    End Sub
    Private Sub RedondearBoton(ByVal boton As System.Windows.Forms.Button, ByVal radio As Integer)
        ' Crear una región redondeada con el radio especificado
        Dim regionHandle As IntPtr = CreateRoundRectRgn(0, 0, boton.Width, boton.Height, radio, radio)

        ' Aplicar la región redondeada al botón
        boton.Region = Region.FromHrgn(regionHandle)
    End Sub
    Private Sub RedondearControles(ByVal control As Control, ByVal radio As Integer)
        ' Crear una región redondeada con el radio especificado
        Dim regionHandle As IntPtr = CreateRoundRectRgn(0, 0, control.Width, control.Height, radio, radio)

        ' Aplicar la región redondeada al control
        control.Region = Region.FromHrgn(regionHandle)
    End Sub

    Private Sub IntercambiarPosiciones(panel1 As Panel, panel2 As Panel)
        Dim tempLocation As Point = panel1.Location
        panel1.Location = panel2.Location
        panel2.Location = tempLocation
    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs)
        RedondearBoton(Button3, 7)
        RedondearBoton(Reimprimir, 7)
        'RedondearControles(TextBox1, 7)
        RedondearBoton(Trazabilidad, 7)
        RedondearBoton(Liberacion, 7)
        RedondearBoton(Registro, 7)
        Embarques.Visible = False
        Dim rutaArchivoEtiquetaActual As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Etiquetas", "No_borrar_ni_abrir.txt")
        ' Ruta del archivo de texto
        Dim rutaArchivo As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Etiquetas", "Area.txt") ' Reemplaza con la ruta real de tu archivo

        ' Lee el contenido del archivo de texto
        Dim contenido As String = File.ReadAllText(rutaArchivo)
        RedondearBordesFormulario()
        WorkOrder_SQL.Enabled = False
        Item_SQL.Enabled = False

        TextBox7.Visible = False
        TextBox8.Visible = False
        'panelLiberacion.Visible = False

        ' Establecer el último botón clicado como el botón de trazabilidad
        ultimoBotonClicado = Trazabilidad

        ' Verifica si el contenido es diferente de "PIPING"
        If contenido.ToUpper() <> "PIPING" Then ' Convertimos a mayúsculas para evitar problemas de comparación
            TextBox2.Visible = False
            Label3.Visible = False
        End If

        If contenido.ToUpper() = "EMBARQUES" Then
            ' Si el contenido del Label1 es "Embarques", muestra el formulario de Embarques en lugar del panel de Trazabilidad
            Dim formEmbarques As New Embarques()
            formEmbarques.TopLevel = False
            formEmbarques.FormBorderStyle = FormBorderStyle.None
            formEmbarques.Dock = DockStyle.Fill
            panelTrazabilidad.Controls.Add(formEmbarques)
            formEmbarques.Show()

            ' Oculta los botones y controles de Trazabilidad en el formulario principal
            Embarques.Visible = True
            TextBox1.Visible = False
            TextBox11.Visible = False
            TextBox4.Visible = False
            TextBox5.Visible = False
            ComboBox2.Visible = False
            TextBox10.Visible = False
            TextBox2.Visible = False
            TextBox13.Visible = False
            Button3.Visible = False
            Reimpirmir.Visible = False
            Reimprimir.Visible = False
            Trazabilidad.Visible = False
            Liberacion.Visible = False
            Registro.Visible = False
            WorkOrder_SQL.Visible = False
            Item_SQL.Visible = False
            modeloFinal_SQL.Visible = False
            Liberado.Visible = False
            Cantidad.Visible = False
            Serial_SQL.Visible = False
            TextBox7.Visible = False
            ComboBox2.Visible = False
            Label6.Visible = False
            Label2.Visible = False
            Label7.Visible = False
            Label8.Visible = False
            Label9.Visible = False
            Label4.Visible = False
            Label3.Visible = False
            Label11.Visible = False
            Label10.Visible = False
            Label12.Visible = False
            GroupBox1.Visible = False

            ' Deshabilita la impresión en el formulario principal
            PrintPreviewDialog1.Document = Nothing
            PrintDocument1.PrinterSettings = Nothing
        End If


        If File.Exists(rutaArchivoEtiquetaActual) Then
            Dim ContenidoRuta As String = File.ReadAllText(rutaArchivoEtiquetaActual)
            If Integer.TryParse(ContenidoRuta, etiquetaActual) Then
                ' Establece el valor recuperado en la caja de texto (si es necesario)
                TextBox12.Text = etiquetaActual.ToString("000000")
            Else
                MessageBox.Show("No se pudo recuperar el valor de etiquetaActual desde el archivo.", "Error")
            End If
        End If

        Dim rutaArchivoSpot As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Etiquetas", "Spot.txt")

        If File.Exists(rutaArchivoSpot) Then
            spot = File.ReadAllText(rutaArchivoSpot)
        Else
            MessageBox.Show("El archivo Spot.txt no existe.", "Error")
        End If

        Try
            etiquetaGeneral = BaseSqlite.MaxConsecutivoGlobal()
            TextBox14.Text = etiquetaGeneral.ToString("000000")
        Catch ex As Exception
            MessageBox.Show("Error al cargar los valores desde la base de datos: " & ex.Message)
        End Try

        Try
            ' Asigna el contenido al control Label
            Label1.Text = contenido
        Catch ex As Exception
            MessageBox.Show("Error al cargar el archivo: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        AgregarBotonVerificacion()

    End Sub

    ' ---- Modulo de Percepcion Visual y OCR ----
    ' El boton se crea por codigo para no modificar el diseñador.
    Private Sub AgregarBotonVerificacion()
        Dim btnOcr As New System.Windows.Forms.Button()
        btnOcr.Name = "btnVerificarOCR"
        btnOcr.Text = "Verify label (OCR)"
        btnOcr.Size = New Size(150, 28)
        btnOcr.Location = New Point(Me.ClientSize.Width - btnOcr.Width - 12, Me.ClientSize.Height - btnOcr.Height - 8)
        btnOcr.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        AddHandler btnOcr.Click, AddressOf BtnVerificarOCR_Click
        Me.Controls.Add(btnOcr)
        btnOcr.BringToFront()
    End Sub

    Private Async Sub BtnVerificarOCR_Click(sender As Object, e As EventArgs)
        Dim usarCamara As Boolean = False
        Dim rutaImagen As String = ""

        Dim resp As DialogResult = MessageBox.Show(
            "¿Verificar la etiqueta con la cámara?" & vbCrLf & vbCrLf &
            "Sí = cámara (ESPACIO para capturar, Q para salir)" & vbCrLf &
            "No = elegir una imagen",
            "Verificar etiqueta (OCR)", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        If resp = DialogResult.Cancel Then Return

        If resp = DialogResult.Yes Then
            usarCamara = True
        Else
            Using dlg As New OpenFileDialog()
                dlg.Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp"
                If dlg.ShowDialog() <> DialogResult.OK Then Return
                rutaImagen = dlg.FileName
            End Using
        End If

        Dim boton As System.Windows.Forms.Button = DirectCast(sender, System.Windows.Forms.Button)
        boton.Enabled = False
        Try
            Dim res As ResultadoOCR = Await System.Threading.Tasks.Task.Run(Function() VerificadorOCR.Verificar(usarCamara, rutaImagen))
            MessageBox.Show(res.Detalle,
                            If(res.Aprobada, "LIBERAR - etiqueta aprobada", "RECHAZAR - etiqueta no válida"),
                            MessageBoxButtons.OK,
                            If(res.Aprobada, MessageBoxIcon.Information, MessageBoxIcon.Warning))
        Finally
            boton.Enabled = True
        End Try
    End Sub

    Private Sub Ventana_MouseDown(sender As Object, e As MouseEventArgs) Handles Ventana.MouseDown
        drag = True
        mouseX = e.X
        mouseY = e.Y
    End Sub

    Private Sub Ventana_MouseMove(sender As Object, e As MouseEventArgs) Handles Ventana.MouseMove
        If drag Then
            Dim newLeft As Integer = e.X + Me.Left - mouseX
            Dim newTop As Integer = e.Y + Me.Top - mouseY
            Me.Location = New Point(newLeft, newTop)
        End If
    End Sub

    Private Sub Ventana_MouseUp(sender As Object, e As MouseEventArgs) Handles Ventana.MouseUp
        drag = False
    End Sub

    Private Sub Cerrar_Click(sender As Object, e As EventArgs) Handles Cerrar.Click
        ' Cierra el formulario principal y termina la aplicación
        Me.Close()
    End Sub

    Private Sub Minimizar_Click(sender As Object, e As EventArgs) Handles Minimizar.Click
        ' Minimiza la ventana principal
        Me.WindowState = FormWindowState.Minimized
    End Sub

    '-------------------------------------------------------- Codigo de funcionamiemta de etiquetas pestaña de Trazabilidad --------------------------------------------------------

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Reimprimir.Click
        ' Verifica que se haya ingresado un valor en TextBox13
        If Not Integer.TryParse(TextBox13.Text, etiquetaActual) Then
            MessageBox.Show("Ingresa una cantidad válida.", "Error")
            Return
        End If

        ' Reimprime la etiqueta anterior
        For i As Integer = 1 To TextBox13.Text
            etiquetaActual = etiquetaAnterior
            TextBox12.Text = etiquetaAnterior.ToString("000000") ' Actualiza TextBox12

            ' Utiliza los valores almacenados en las variables temporales para la impresión
            TextBox1.Text = tempTextBox1
            TextBox2.Text = tempTextBox2
            TextBox4.Text = tempTextBox4
            TextBox5.Text = tempTextBox5
            TextBox7.Text = tempTextBox7
            TextBox11.Text = tempTextBox11

            TextBox7.Text = "*" & TextBox11.Text & "*"
            TextBox8.Text = TextBox11.Text
            ' Imprime la etiqueta utilizando los valores actualizados
            PrintDocument1.Print()

            ' Limpiar los campos TextBox después de la impresión
            TextBox1.Clear()
            TextBox2.Clear()
            TextBox4.Clear()
            TextBox5.Clear()
            TextBox7.Clear()
            TextBox10.Clear()
            TextBox11.Clear()
            TextBox13.Clear()
            ComboBox2.SelectedIndex = -1
        Next
    End Sub


    Private Sub Command1_Click(sender As Object, e As EventArgs)
        ' Establece el valor de TextBox12 al valor de la etiqueta anterior
        TextBox12.Text = etiquetaAnterior.ToString("000000")

        ' Imprime la etiqueta anterior
        PrintDocument1.Print()

        ' Limpia los TextBox después de la impresión
        TextBox1.Clear()
        TextBox4.Clear()
        TextBox5.Clear()
        TextBox7.Clear()
        TextBox11.Clear()
    End Sub


    Private mesAnterior As Integer = DateTime.Now.Month ' Variable para almacenar el mes anterior

    Private Sub PrintDocument1_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintDocument1.PrintPage
        ' Fecha tomada al momento de imprimir (no al abrir la app) para que serial y fecha no queden desfasados
        Dim fechaActual As DateTime = DateTime.Now
        Dim anoActual As Integer = fechaActual.Year
        Dim mesActual As Integer = fechaActual.Month
        'Dim letra As String = ""
        Dim newFont As New Font(TextBox8.Font.FontFamily, 11) ' Cambia 12 al tamaño de fuente deseado
        'Dim NS As String = "R" & anoActual.ToString().Substring(2) & letra & "S" & etiquetaGeneral.ToString("000000")

        ' Verifica si el mes ha cambiado
        If mesAnterior <> mesActual Then
            etiquetaGeneral = 0 ' Reinicia el contador del consecutivo
            mesAnterior = mesActual ' Actualiza el mes anterior al actual
        End If

        Try
            etiquetaGeneral = BaseSqlite.MaxConsecutivoGlobal()
            TextBox14.Text = etiquetaGeneral.ToString("000000")
        Catch ex As Exception
            MessageBox.Show("Error al cargar los valores desde la base de datos: " & ex.Message)
        End Try

        Dim rutaArchivo As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Etiquetas", "Area.txt") ' Reemplaza con la ruta real de tu archivo
        Dim contenidoArea As String = File.ReadAllText(rutaArchivo)
        ' Actualiza el valor de @Area con el contenido leído del archivo
        Dim area As String = contenidoArea

        Select Case mesActual
            Case 1
                letra = "A"
            Case 2
                letra = "B"
            Case 3
                letra = "C"
            Case 4
                letra = "D"
            Case 5
                letra = "E"
            Case 6
                letra = "F"
            Case 7
                letra = "G"
            Case 8
                letra = "H"
            Case 9
                letra = "J"
            Case 10
                letra = "K"
            Case 11
                letra = "L"
            Case 12
                letra = "M"
            Case Else
                letra = "Error"
        End Select

        ' Lee el contenido del archivo de texto
        Dim contenido As String = File.ReadAllText(rutaArchivo)
        If contenido.ToUpper() <> "PIPING" Then ' Convertimos a mayúsculas para evitar problemas de comparación
            TextBox2.Visible = False
            Label3.Visible = False
        Else
            ' Si el contenido es "PIPING", hacemos visible el texto adicional
            ' Aquí puedes agregar cualquier otro elemento que necesites mostrar cuando el contenido es "PIPING"
            e.Graphics.DrawString("Rv: ", TextBox8.Font, Brushes.Black, 247, -4)
            e.Graphics.DrawString(TextBox2.Text, TextBox8.Font, Brushes.Black, 267, -4)
        End If
        ' Incrementa el contador para etiquetaActual.

        etiquetaAnterior = etiquetaActual
        etiquetaActual += 1
        ' Almacena el valor actual del área para la etiqueta
        areaAnterior = contenidoArea
        'ActualizarAreaEnBaseDeDatos(areaAnterior)

        ' Incrementa el contador para etiquetaGeneral.
        etiquetaGeneral += 1

        Dim NS As String = "R" & anoActual.ToString().Substring(2) & letra & "S" & etiquetaGeneral.ToString("000000")

        For i As Integer = 1 To PrintDocument1.PrinterSettings.Copies
            TextBox12.Text = etiquetaActual.ToString("000000")
            TextBox14.Text = etiquetaGeneral.ToString("000000")

            e.Graphics.DrawString("*" & NS & "*", TextBox7.Font, Brushes.Black, 70, 66)
            e.Graphics.DrawString(NS, newFont, Brushes.Black, 70, 90)
            e.HasMorePages = i < PrintDocument1.PrinterSettings.Copies
            e.Graphics.DrawString("WO" & ": ", TextBox8.Font, Brushes.Black, 40, -4)
            e.Graphics.DrawString(TextBox1.Text & "-" & TextBox11.Text, TextBox8.Font, Brushes.Black, 75, -4)
            e.Graphics.DrawString("SV: " & TextBox4.Text, TextBox8.Font, Brushes.Black, 40, 49)
            e.Graphics.DrawString("SN: ", TextBox8.Font, Brushes.Black, 40, 69)
            e.Graphics.DrawString("Turno: " & ComboBox2.Text, TextBox8.Font, Brushes.Black, 107, 49)
            e.Graphics.DrawString("Fecha: " & fechaActual.ToString("dd/MM/yyyy"), TextBox8.Font, Brushes.Black, 170, 49)
            e.Graphics.DrawString("M/I: ", TextBox8.Font, Brushes.Black, 40, 13)
            e.Graphics.DrawString(TextBox7.Text, TextBox7.Font, Brushes.Black, 75, 14)
            e.Graphics.DrawString("# Identificacion: ", TextBox8.Font, Brushes.Black, 40, 35)
            e.Graphics.DrawString(TextBox5.Text, TextBox8.Font, Brushes.Black, 150, 35)

        Next

        Dim rutaArchivoEtiquetaActual As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Etiquetas", "No_borrar_ni_abrir.txt")
        File.WriteAllText(rutaArchivoEtiquetaActual, etiquetaActual.ToString())

        Try
            ' La revisión (Rv) solo se imprime en PIPING, así que solo ahí se guarda
            Dim revision As String = If(contenido.ToUpper() = "PIPING", TextBox2.Text, "")
            BaseSqlite.Insertar(area, spot, systemName, TextBox1.Text, TextBox11.Text, TextBox4.Text, TextBox5.Text, ComboBox2.Text, TextBox10.Text, NS, etiquetaGeneral, etiquetaActual, fechaActual, revision)
        Catch ex As Exception
            MessageBox.Show("Error al almacenar los datos en la base de datos: " & ex.Message)
        End Try

    End Sub



    Private Sub ActualizarAreaEnBaseDeDatos(nuevaArea As String)
        Try

            dbConn13.Open()

            ' Define la consulta SQL para actualizar la variable 'Area'
            Dim sqlUpdate As String = "UPDATE Quality_print_records SET Area = @NuevaArea"

            ' Crea y ejecuta el comando SQL
            Using cmd As New SqlCommand(sqlUpdate, dbConn13)
                cmd.Parameters.AddWithValue("@NuevaArea", nuevaArea)
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al actualizar la base de datos: " & ex.Message)
        Finally
            ' Cierra la conexión a la base de datos
            dbConn13.Close()
        End Try


    End Sub



    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If TextBox11.Text <> "" AndAlso TextBox10.Text <> "" Then ' Verifica si ambos TextBox11 y TextBox10 no están vacíos
            Dim numberOfCopies As Integer
            If Not Integer.TryParse(TextBox10.Text, numberOfCopies) Then
                MessageBox.Show("Ingrese un número válido en TextBox10.", "Error")
                Return
            End If

            ' Guarda los valores actuales en variables temporales
            tempTextBox1 = TextBox1.Text
            tempTextBox2 = TextBox2.Text
            tempTextBox4 = TextBox4.Text
            tempTextBox5 = TextBox5.Text
            tempTextBox7 = TextBox7.Text
            tempTextBox11 = TextBox11.Text

            For k As Integer = 1 To numberOfCopies
                TextBox7.Text = "*" & TextBox11.Text & "*"
                TextBox8.Text = TextBox11.Text
                PrintPreviewDialog1.Document = PrintDocument1
                PrintDocument1.PrinterSettings = PrintDialog1.PrinterSettings
                PrintDocument1.Print()
            Next

            ' Limpia todos los campos después de la impresión
            TextBox1.Clear()
            TextBox2.Clear()
            TextBox4.Clear()
            TextBox5.Clear()
            TextBox7.Clear()
            TextBox10.Clear()
            TextBox11.Clear()
            TextBox13.Clear()
            ComboBox2.SelectedIndex = -1

        Else
            MsgBox("Complete todos los campos")
        End If
    End Sub

    '-------------------------------------------------------- Codigo de funcionamiemta de etiquetas de la pestaña Liberacion --------------------------------------------------------

    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Dim serial As String = Serial_SQL.Text.Trim()

        ' Verifica si el serial no está vacío
        If serial <> "" Then
            Try
                Dim woEncontrado As String = ""
                Dim itemEncontrado As String = ""
                If BaseSqlite.BuscarPorSerial(serial, woEncontrado, itemEncontrado) Then
                    WorkOrder_SQL.Text = woEncontrado
                    Item_SQL.Text = itemEncontrado
                Else
                    MessageBox.Show("No se encontraron resultados para el serial proporcionado.", "Error")
                End If
            Catch ex As Exception
                MessageBox.Show("Error al buscar en la base de datos: " & ex.Message, "Error")
            End Try
        Else
            MessageBox.Show("Por favor, ingresa un serial antes de realizar la búsqueda.", "Error")
        End If
    End Sub

    Private Sub PrintDocument2_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintDocument2.PrintPage
        Dim newFont As New Font(TextBox3.Font.FontFamily, 11)
        Dim textoModelo As String = "Modelo final: " & modeloFinal_SQL.Text.ToUpper()
        Dim textSerial As String = Serial_SQL.Text.Trim().ToUpper()
        Dim fechaActual As DateTime = DateTime.Now

        ' Obtener los componentes de la fecha
        Dim dia As Integer = fechaActual.Day
        Dim mes As Integer = fechaActual.Month
        Dim año As Integer = fechaActual.Year

        ' Obtener los componentes de la hora
        Dim hora As Integer = fechaActual.Hour
        Dim minuto As Integer = fechaActual.Minute

        ' Luego puedes usar estos valores como necesites en tu impresión
        Dim fechaImpresion As String = $"{dia}/{mes}/{año}   Hora: {hora}:{minuto}"

        e.Graphics.DrawString("Calidad Liberacion", TextBox3.Font, Brushes.Black, 110, 0)
        e.Graphics.DrawString("WO: " & WorkOrder_SQL.Text, TextBox8.Font, Brushes.Black, 40, 22)
        e.Graphics.DrawString("Item: " & Item_SQL.Text, TextBox8.Font, Brushes.Black, 160, 22)
        e.Graphics.DrawString(textoModelo, TextBox8.Font, Brushes.Black, 40, 41)
        e.Graphics.DrawString($"Fecha: {fechaImpresion}", TextBox8.Font, Brushes.Black, 40, 59)
        e.Graphics.DrawString("Turno: " & Turno.Text, TextBox8.Font, Brushes.Black, 240, 59)
        ' Código de barras del serial que se está liberando (antes usaba el último serial generado en esta estación)
        e.Graphics.DrawString("*" & textSerial & "*", TextBox7.Font, Brushes.Black, 80, 101)
        e.Graphics.DrawString("Liberado por: " & Liberado.Text, TextBox8.Font, Brushes.Black, 40, 79)
        e.Graphics.DrawString("Serial: ", TextBox8.Font, Brushes.Black, 40, 101)
        e.Graphics.DrawString(textSerial, TextBox3.Font, Brushes.Black, 115, 125)

        Try
            Dim filas As Integer = BaseSqlite.Liberar(Serial_SQL.Text, Liberado.Text, fechaActual, modeloFinal_SQL.Text)
            If filas > 0 Then
                MessageBox.Show($"Se a liberado la etiqueta con el serial {Serial_SQL.Text}.", "Liberado con exito")
            Else
                MessageBox.Show($"No se encontró ninguna fila para el serial {Serial_SQL.Text} en la base de datos.", "Error")
            End If
        Catch ex As Exception
            ' Manejo de errores
        End Try
    End Sub

    Private Sub ImprimirQA_Click(sender As Object, e As EventArgs) Handles ImprimirQA.Click
        If Liberado.Text <> "" AndAlso Serial_SQL.Text <> "" AndAlso WorkOrder_SQL.Text <> "" AndAlso Item_SQL.Text <> "" AndAlso modeloFinal_SQL.Text <> "" Then ' Verifica si ambos TextBox11 y TextBox10 no están vacíos
            Dim numberOfCopies As Integer
            If Not Integer.TryParse(Cantidad.Text, numberOfCopies) Then
                MessageBox.Show("Ingrese un número válido.", "Error")
                Return
            End If

            ' Guarda los valores actuales en variables temporales
            tempWorkOrder = WorkOrder_SQL.Text
            tempItem = Item_SQL.Text
            tempSecuencia = modeloFinal_SQL.Text
            tempLiberado = Liberado.Text
            tempCantidad = Cantidad.Text
            tempSerial = Serial_SQL.Text

            For k As Integer = 1 To numberOfCopies
                PrintPreviewDialog1.Document = PrintDocument2
                PrintDocument2.PrinterSettings = PrintDialog1.PrinterSettings
                PrintDocument2.Print()
            Next

            ' Limpia todos los campos después de la impresión
            WorkOrder_SQL.Clear()
            Item_SQL.Clear()
            modeloFinal_SQL.Clear()
            Liberado.Clear()
            Cantidad.Clear()
            Serial_SQL.Clear()
            TextBox7.Clear()
            Turno.SelectedIndex = -1

        Else
            MsgBox("Complete todos los campos")
        End If
    End Sub

    Private Sub ReimpresionQA_Click(sender As Object, e As EventArgs) Handles ReimpresionQA.Click
        ' Verifica que se haya ingresado un valor
        If Not IsNumeric(Reimpirmir.Text) Then
            MessageBox.Show("Ingresa un valor valido", "Error")
            Return
        End If

        Dim numberOfCopies As Integer = CInt(Reimprimir.Text)

        ' Ahora puedes continuar con el bucle For
        For i As Integer = 1 To numberOfCopies
            ' Se utilizan los valoires almacenados 
            WorkOrder_SQL.Text = tempWorkOrder
            Item_SQL.Text = tempItem =
            modeloFinal_SQL.Text = tempSecuencia
            Liberado.Text = tempLiberado
            Cantidad.Text = tempCantidad
            Serial_SQL.Text = tempSerial

            ' Imprime la etiqueta utilizando los valores actualizados
            PrintPreviewDialog1.Document = PrintDocument2
            PrintDocument2.PrinterSettings = PrintDialog1.PrinterSettings
            PrintDocument2.Print()

        Next
        'Se limpian los campos
        WorkOrder_SQL.Clear()
        Item_SQL.Clear()
        modeloFinal_SQL.Clear()
        Liberado.Clear()
        Cantidad.Clear()
        Serial_SQL.Clear()
        TextBox7.Clear()
        Turno.SelectedIndex = -1


    End Sub

    Private Sub Trazabilidad_Click(sender As Object, e As EventArgs) Handles Trazabilidad.Click
        ' Verificar si el botón clicado es diferente al último botón clicado
        If ultimoBotonClicado IsNot Trazabilidad Then
            ' Intercambiar los paneles
            IntercambiarPosiciones(panelTrazabilidad, panelLiberacion)
            ' Actualizar el último botón clicado
            ultimoBotonClicado = Trazabilidad
        End If
    End Sub

    Private Sub Liberacion_Click(sender As Object, e As EventArgs) Handles Liberacion.Click
        ' Verificar si el botón clicado es diferente al último botón clicado
        If ultimoBotonClicado IsNot Liberacion Then
            ' Mostrar el formulario de ingreso de contraseña
            Dim formIngresarContraseña As New Liberacion()
            If formIngresarContraseña.ShowDialog() = DialogResult.OK Then
                ' Si la contraseña es correcta, intercambiar los paneles
                IntercambiarPosiciones(panelTrazabilidad, panelLiberacion)
                ' Actualizar el último botón clicado
                ultimoBotonClicado = Liberacion
            End If
        End If
    End Sub

    Private Sub Registro_Click(sender As Object, e As EventArgs) Handles Registro.Click
        ' Mostrar el formulario de ingreso de contraseña (Liberacion)
        Dim formIngresarContraseña As New Liberacion()

        If formIngresarContraseña.ShowDialog() = DialogResult.OK Then
            ' Si la contraseña es correcta, se abre el formulario de Registro
            Dim formularioRegistro As New Registro()
            formularioRegistro.ShowDialog()
        End If
    End Sub

End Class

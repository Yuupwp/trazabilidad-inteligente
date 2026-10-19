Imports System.IO
Imports System.Runtime.InteropServices


Public Class Liberacion

    Public Sub New()
        InitializeComponent()
        AddHandler Me.Load, AddressOf Liberacion_Load
    End Sub
    ' Importar funciones de la API de Windows para el dibujo
    <DllImport("Gdi32.dll", EntryPoint:="CreateRoundRectRgn")>
    Private Shared Function CreateRoundRectRgn(ByVal nLeftRect As Integer, ByVal nTopRect As Integer, ByVal nRightRect As Integer, ByVal nBottomRect As Integer, ByVal nWidthEllipse As Integer, ByVal nHeightEllipse As Integer) As IntPtr
    End Function

    Private Sub RedondearBoton(ByVal boton As System.Windows.Forms.Button, ByVal radio As Integer)
        ' Crear una región redondeada con el radio especificado
        Dim regionHandle As IntPtr = CreateRoundRectRgn(0, 0, boton.Width, boton.Height, radio, radio)

        ' Aplicar la región redondeada al botón
        boton.Region = Region.FromHrgn(regionHandle)
    End Sub
    Private Sub IntercambiarPosiciones(panel1 As Panel, panel2 As Panel)
        Dim tempLocation As Point = panel1.Location
        panel1.Location = panel2.Location
        panel2.Location = tempLocation
    End Sub
    Private Sub btnIngresar_Click(sender As Object, e As EventArgs) Handles Ingresar.Click
        ' Obtener el usuario y la contraseña ingresados por el usuario
        Dim usuarioIngresado As String = Usuario.Text
        Dim contraseñaIngresada As String = Contraseña.Text

        ' Leer el archivo de texto que contiene los usuarios y contraseñas
        Dim rutaArchivo As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Etiquetas", "PassQA.txt") ' Reemplaza con la ruta real del archivo

        If File.Exists(rutaArchivo) Then
            Using lector As New StreamReader(rutaArchivo)
                Dim usuarioGuardado As String = String.Empty
                Dim contraseñaGuardada As String = String.Empty
                Dim encontróUsuario As Boolean = False

                While Not lector.EndOfStream
                    Dim linea As String = lector.ReadLine()

                    If linea.StartsWith("Usuario:") Then
                        ' Extraer el usuario de la línea
                        usuarioGuardado = linea.Replace("Usuario:", "").Trim()
                        encontróUsuario = True
                    ElseIf linea.StartsWith("Contraseña:") AndAlso encontróUsuario Then
                        ' Extraer la contraseña de la línea
                        contraseñaGuardada = linea.Replace("Contraseña:", "").Trim()

                        ' Verificar si el usuario y la contraseña ingresados coinciden con los guardados
                        If usuarioIngresado = usuarioGuardado AndAlso contraseñaIngresada = contraseñaGuardada Then
                            ' Si coinciden, cerrar el formulario con resultado OK
                            DialogResult = DialogResult.OK
                            Return
                        End If

                        ' Reiniciar para el siguiente usuario
                        usuarioGuardado = String.Empty
                        contraseñaGuardada = String.Empty
                        encontróUsuario = False
                    End If
                End While
            End Using
        Else
            MessageBox.Show("El archivo de contraseñas no se encuentra.", "Error de Archivo", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

        ' Si el usuario y/o la contraseña son incorrectos, mostrar un mensaje de error
        MessageBox.Show("Usuario y/o contraseña incorrectos. Intente de nuevo.", "Error de Autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error)
        ' Limpiar los TextBox de usuario y contraseña
        Usuario.Clear()
        Contraseña.Clear()
    End Sub



    Private Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles Cancelar.Click
        ' Cerrar el formulario con resultado Cancelar
        DialogResult = DialogResult.Cancel
    End Sub

    Private Sub Liberacion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoSize = False
        Me.MaximizeBox = False
        Contraseña.UseSystemPasswordChar = True
        RedondearBoton(Ingresar, 7)
        RedondearBoton(Cancelar, 7)
    End Sub
End Class

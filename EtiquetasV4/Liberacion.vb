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
        ' Los usuarios viven en la tabla usuarios de la base SQLite, con la contraseña cifrada (PBKDF2).
        Try
            If Not BaseSqlite.HayUsuarios() Then
                MessageBox.Show("No hay usuarios registrados. Crea el primero con el botón Register.",
                                "Sin usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information)
                DialogResult = DialogResult.Cancel
                Return
            End If
            If BaseSqlite.ValidarUsuario(Usuario.Text, Contraseña.Text) Then
                DialogResult = DialogResult.OK
                Return
            End If
        Catch ex As Exception
            MessageBox.Show("Error al validar el usuario: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

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

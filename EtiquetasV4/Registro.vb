Imports System.IO
Imports System.Runtime.InteropServices

Public Class Registro

    ' Guarda el usuario en la tabla usuarios con la contraseña cifrada (ver BaseSqlite.CrearUsuario).
    Private Sub GuardarRegistro(usuario As String, contraseña As String)
        Try
            If Not BaseSqlite.CrearUsuario(usuario, contraseña) Then
                MessageBox.Show($"El usuario {usuario} ya existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
        Catch ex As Exception
            MessageBox.Show("Error al guardar el usuario: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        MessageBox.Show("Registro guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ' Cerrar el formulario con resultado Cancelar
        DialogResult = DialogResult.Cancel
    End Sub

    Private Sub Registrar_Click(sender As Object, e As EventArgs) Handles Registrar.Click
        ' Obtener los datos del usuario y la contraseña
        Dim usuario As String = Usuariotxt.Text
        Dim contraseña As String = Contraseñatxt.Text

        ' Verificar que los datos cumplan con los requisitos
        If usuario.Length >= 3 AndAlso usuario.Length <= 8 AndAlso contraseña.Length >= 3 AndAlso contraseña.Length <= 8 Then
            ' Guardar el registro en el archivo
            GuardarRegistro(usuario, contraseña)
        Else
            MessageBox.Show("El usuario y la contraseña deben tener entre 3 y 8 caracteres.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub Registro_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoSize = False
        Me.MaximizeBox = False
        ' Llamar a la función RedondearBoton para redondear el botón Cancelar con un radio de 10 píxeles
        RedondearBoton(Cancelartxt, 5)
        RedondearBoton(Registrar, 5)
        Contraseñatxt.UseSystemPasswordChar = True
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Cancelartxt.Click
        ' Cerrar el formulario con resultado Cancelar
        DialogResult = DialogResult.Cancel
    End Sub

    ' Importar función de la API de Windows para redondear los bordes del botón
    <DllImport("Gdi32.dll", EntryPoint:="CreateRoundRectRgn")>
    Private Shared Function CreateRoundRectRgn(ByVal nLeftRect As Integer, ByVal nTopRect As Integer, ByVal nRightRect As Integer, ByVal nBottomRect As Integer, ByVal nWidthEllipse As Integer, ByVal nHeightEllipse As Integer) As IntPtr
    End Function

    Private Sub RedondearBoton(ByVal boton As Button, ByVal radio As Integer)
        ' Crear una región redondeada con el radio especificado
        Dim regionHandle As IntPtr = CreateRoundRectRgn(0, 0, boton.Width, boton.Height, radio, radio)

        ' Aplicar la región redondeada al botón
        boton.Region = Region.FromHrgn(regionHandle)
    End Sub

End Class

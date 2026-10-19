<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Liberacion
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(Liberacion))
        Label1 = New Label()
        Contraseña = New TextBox()
        Ingresar = New Button()
        Cancelar = New Button()
        Label2 = New Label()
        Usuario = New TextBox()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label1.Location = New Point(28, 57)
        Label1.Margin = New Padding(2, 0, 2, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(94, 20)
        Label1.TabIndex = 100022
        Label1.Text = "Contraseña: "
        ' 
        ' Contraseña
        ' 
        Contraseña.Location = New Point(127, 54)
        Contraseña.Name = "Contraseña"
        Contraseña.Size = New Size(207, 27)
        Contraseña.TabIndex = 2
        ' 
        ' Ingresar
        ' 
        Ingresar.BackColor = SystemColors.ScrollBar
        Ingresar.FlatAppearance.BorderSize = 0
        Ingresar.FlatStyle = FlatStyle.Flat
        Ingresar.Location = New Point(50, 97)
        Ingresar.Name = "Ingresar"
        Ingresar.Size = New Size(89, 34)
        Ingresar.TabIndex = 3
        Ingresar.Text = "Ingresar"
        Ingresar.UseVisualStyleBackColor = False
        ' 
        ' Cancelar
        ' 
        Cancelar.BackColor = SystemColors.ScrollBar
        Cancelar.FlatAppearance.BorderSize = 0
        Cancelar.FlatStyle = FlatStyle.Flat
        Cancelar.Location = New Point(224, 97)
        Cancelar.Name = "Cancelar"
        Cancelar.Size = New Size(89, 34)
        Cancelar.TabIndex = 4
        Cancelar.Text = "Cancelar"
        Cancelar.UseVisualStyleBackColor = False
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label2.Location = New Point(28, 18)
        Label2.Margin = New Padding(2, 0, 2, 0)
        Label2.Name = "Label2"
        Label2.Size = New Size(66, 20)
        Label2.TabIndex = 100026
        Label2.Text = "Usuario:"
        ' 
        ' Usuario
        ' 
        Usuario.Location = New Point(127, 18)
        Usuario.Name = "Usuario"
        Usuario.Size = New Size(207, 27)
        Usuario.TabIndex = 1
        ' 
        ' Liberacion
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.ButtonFace
        ClientSize = New Size(359, 143)
        Controls.Add(Usuario)
        Controls.Add(Label2)
        Controls.Add(Cancelar)
        Controls.Add(Ingresar)
        Controls.Add(Contraseña)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.Fixed3D
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Liberacion"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Inicias sesion"
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents Label1 As Label
    Friend WithEvents Contraseña As TextBox
    Friend WithEvents Ingresar As Button
    Friend WithEvents Cancelar As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents Usuario As TextBox
End Class

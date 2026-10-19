<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Registro
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
        Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(Registro))
        Label2 = New Label()
        Label1 = New Label()
        Label3 = New Label()
        Registrar = New Button()
        Cancelartxt = New Button()
        Usuariotxt = New TextBox()
        Contraseñatxt = New TextBox()
        SuspendLayout()
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point)
        Label2.Location = New Point(89, 10)
        Label2.Margin = New Padding(2, 0, 2, 0)
        Label2.Name = "Label2"
        Label2.Size = New Size(204, 28)
        Label2.TabIndex = 100027
        Label2.Text = "Registrar de personal"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label1.Location = New Point(20, 58)
        Label1.Margin = New Padding(2, 0, 2, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(70, 20)
        Label1.TabIndex = 100028
        Label1.Text = "Usuario: "
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label3.Location = New Point(20, 96)
        Label3.Margin = New Padding(2, 0, 2, 0)
        Label3.Name = "Label3"
        Label3.Size = New Size(94, 20)
        Label3.TabIndex = 100029
        Label3.Text = "Contraseña: "
        ' 
        ' Registrar
        ' 
        Registrar.BackColor = SystemColors.ScrollBar
        Registrar.FlatAppearance.BorderSize = 0
        Registrar.FlatStyle = FlatStyle.Flat
        Registrar.Location = New Point(47, 146)
        Registrar.Name = "Registrar"
        Registrar.Size = New Size(106, 32)
        Registrar.TabIndex = 100030
        Registrar.Text = "Registrar"
        Registrar.UseVisualStyleBackColor = False
        ' 
        ' Cancelartxt
        ' 
        Cancelartxt.BackColor = SystemColors.ScrollBar
        Cancelartxt.FlatAppearance.BorderSize = 0
        Cancelartxt.FlatStyle = FlatStyle.Flat
        Cancelartxt.Location = New Point(219, 146)
        Cancelartxt.Name = "Cancelartxt"
        Cancelartxt.Size = New Size(107, 32)
        Cancelartxt.TabIndex = 100031
        Cancelartxt.Text = "Cancelar"
        Cancelartxt.UseVisualStyleBackColor = False
        ' 
        ' Usuariotxt
        ' 
        Usuariotxt.Location = New Point(124, 58)
        Usuariotxt.Name = "Usuariotxt"
        Usuariotxt.Size = New Size(244, 27)
        Usuariotxt.TabIndex = 100032
        ' 
        ' Contraseñatxt
        ' 
        Contraseñatxt.Location = New Point(124, 96)
        Contraseñatxt.Name = "Contraseñatxt"
        Contraseñatxt.Size = New Size(244, 27)
        Contraseñatxt.TabIndex = 100033
        ' 
        ' Registro
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.Control
        ClientSize = New Size(387, 193)
        Controls.Add(Contraseñatxt)
        Controls.Add(Usuariotxt)
        Controls.Add(Cancelartxt)
        Controls.Add(Registrar)
        Controls.Add(Label3)
        Controls.Add(Label1)
        Controls.Add(Label2)
        FormBorderStyle = FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Registro"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Registro"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Registrar As Button
    Friend WithEvents Cancelartxt As Button
    Friend WithEvents Usuariotxt As TextBox
    Friend WithEvents Contraseñatxt As TextBox
End Class

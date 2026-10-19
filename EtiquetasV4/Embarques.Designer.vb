<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Embarques
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
        Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(Embarques))
        Imprimir = New Button()
        SO = New TextBox()
        Label6 = New Label()
        Label1 = New Label()
        DN = New TextBox()
        Label2 = New Label()
        SA = New TextBox()
        Label3 = New Label()
        Tracking = New TextBox()
        Label4 = New Label()
        Carrier = New TextBox()
        Label5 = New Label()
        Cantidad = New TextBox()
        PrintDocument3 = New Printing.PrintDocument()
        PrintDialog3 = New PrintDialog()
        Label7 = New Label()
        TextBox1 = New TextBox()
        Reimprimir = New Button()
        Fuente = New TextBox()
        PrintPreviewDialog3 = New PrintPreviewDialog()
        Barras = New TextBox()
        fuenteParrafo = New TextBox()
        fuenteAddress = New TextBox()
        SuspendLayout()
        ' 
        ' Imprimir
        ' 
        Imprimir.BackColor = SystemColors.ScrollBar
        Imprimir.FlatAppearance.BorderSize = 0
        Imprimir.FlatStyle = FlatStyle.Flat
        Imprimir.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Imprimir.Location = New Point(87, 265)
        Imprimir.Margin = New Padding(3, 4, 3, 4)
        Imprimir.Name = "Imprimir"
        Imprimir.Size = New Size(86, 31)
        Imprimir.TabIndex = 7
        Imprimir.Text = "Print"
        Imprimir.UseVisualStyleBackColor = False
        ' 
        ' SO
        ' 
        SO.Location = New Point(34, 39)
        SO.Name = "SO"
        SO.Size = New Size(222, 27)
        SO.TabIndex = 1
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label6.Location = New Point(34, 16)
        Label6.Margin = New Padding(2, 0, 2, 0)
        Label6.Name = "Label6"
        Label6.Size = New Size(87, 20)
        Label6.TabIndex = 100022
        Label6.Text = "Sales Order"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label1.Location = New Point(34, 77)
        Label1.Margin = New Padding(2, 0, 2, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(128, 20)
        Label1.TabIndex = 100024
        Label1.Text = "Delivery Number"
        ' 
        ' DN
        ' 
        DN.Location = New Point(34, 100)
        DN.Name = "DN"
        DN.Size = New Size(222, 27)
        DN.TabIndex = 2
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label2.Location = New Point(34, 138)
        Label2.Margin = New Padding(2, 0, 2, 0)
        Label2.Name = "Label2"
        Label2.Size = New Size(115, 20)
        Label2.TabIndex = 100026
        Label2.Text = "Ship to Address"
        ' 
        ' SA
        ' 
        SA.Font = New Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
        SA.Location = New Point(34, 161)
        SA.Multiline = True
        SA.Name = "SA"
        SA.RightToLeft = RightToLeft.No
        SA.ScrollBars = ScrollBars.Horizontal
        SA.Size = New Size(222, 86)
        SA.TabIndex = 3
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label3.Location = New Point(299, 16)
        Label3.Margin = New Padding(2, 0, 2, 0)
        Label3.Name = "Label3"
        Label3.Size = New Size(67, 20)
        Label3.TabIndex = 100028
        Label3.Text = "Tracking"
        ' 
        ' Tracking
        ' 
        Tracking.Location = New Point(299, 39)
        Tracking.Name = "Tracking"
        Tracking.Size = New Size(222, 27)
        Tracking.TabIndex = 4
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label4.Location = New Point(299, 77)
        Label4.Margin = New Padding(2, 0, 2, 0)
        Label4.Name = "Label4"
        Label4.Size = New Size(56, 20)
        Label4.TabIndex = 100030
        Label4.Text = "Carrier"
        ' 
        ' Carrier
        ' 
        Carrier.Location = New Point(299, 100)
        Carrier.Name = "Carrier"
        Carrier.Size = New Size(222, 27)
        Carrier.TabIndex = 5
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label5.Location = New Point(299, 138)
        Label5.Margin = New Padding(2, 0, 2, 0)
        Label5.Name = "Label5"
        Label5.Size = New Size(102, 20)
        Label5.TabIndex = 100032
        Label5.Text = "No. de copias"
        ' 
        ' Cantidad
        ' 
        Cantidad.Location = New Point(299, 162)
        Cantidad.Margin = New Padding(3, 4, 3, 4)
        Cantidad.Name = "Cantidad"
        Cantidad.Size = New Size(102, 27)
        Cantidad.TabIndex = 6
        ' 
        ' PrintDocument3
        ' 
        ' 
        ' PrintDialog3
        ' 
        PrintDialog3.UseEXDialog = True
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Label7.Location = New Point(334, 205)
        Label7.Margin = New Padding(2, 0, 2, 0)
        Label7.Name = "Label7"
        Label7.Size = New Size(158, 20)
        Label7.TabIndex = 100034
        Label7.Text = "No. de Reimpresiones"
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(334, 233)
        TextBox1.Margin = New Padding(3, 4, 3, 4)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(158, 27)
        TextBox1.TabIndex = 0
        ' 
        ' Reimprimir
        ' 
        Reimprimir.BackColor = SystemColors.ScrollBar
        Reimprimir.FlatAppearance.BorderSize = 0
        Reimprimir.FlatStyle = FlatStyle.Flat
        Reimprimir.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        Reimprimir.Location = New Point(360, 267)
        Reimprimir.Margin = New Padding(2, 3, 2, 3)
        Reimprimir.Name = "Reimprimir"
        Reimprimir.Size = New Size(108, 32)
        Reimprimir.TabIndex = 0
        Reimprimir.Text = "Reprint"
        Reimprimir.UseVisualStyleBackColor = False
        ' 
        ' Fuente
        ' 
        Fuente.Font = New Font("Segoe UI Semibold", 28F, FontStyle.Bold, GraphicsUnit.Point)
        Fuente.Location = New Point(560, 304)
        Fuente.Multiline = True
        Fuente.Name = "Fuente"
        Fuente.Size = New Size(10, 10)
        Fuente.TabIndex = 100036
        ' 
        ' PrintPreviewDialog3
        ' 
        PrintPreviewDialog3.AutoScrollMargin = New Size(0, 0)
        PrintPreviewDialog3.AutoScrollMinSize = New Size(0, 0)
        PrintPreviewDialog3.ClientSize = New Size(400, 300)
        PrintPreviewDialog3.Enabled = True
        PrintPreviewDialog3.Icon = CType(resources.GetObject("PrintPreviewDialog3.Icon"), Icon)
        PrintPreviewDialog3.Name = "PrintPreviewDialog1"
        PrintPreviewDialog3.Visible = False
        ' 
        ' Barras
        ' 
        Barras.Font = New Font("Bar-Code 39", 22.2F, FontStyle.Regular, GraphicsUnit.Point)
        Barras.Location = New Point(542, 304)
        Barras.Multiline = True
        Barras.Name = "Barras"
        Barras.Size = New Size(10, 10)
        Barras.TabIndex = 100037
        ' 
        ' fuenteParrafo
        ' 
        fuenteParrafo.Font = New Font("Segoe UI", 24F, FontStyle.Regular, GraphicsUnit.Point)
        fuenteParrafo.Location = New Point(527, 304)
        fuenteParrafo.Multiline = True
        fuenteParrafo.Name = "fuenteParrafo"
        fuenteParrafo.Size = New Size(10, 10)
        fuenteParrafo.TabIndex = 100038
        ' 
        ' fuenteAddress
        ' 
        fuenteAddress.Font = New Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point)
        fuenteAddress.Location = New Point(511, 304)
        fuenteAddress.Multiline = True
        fuenteAddress.Name = "fuenteAddress"
        fuenteAddress.Size = New Size(10, 10)
        fuenteAddress.TabIndex = 100039
        ' 
        ' Embarques
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(574, 321)
        Controls.Add(fuenteAddress)
        Controls.Add(fuenteParrafo)
        Controls.Add(Barras)
        Controls.Add(Fuente)
        Controls.Add(Reimprimir)
        Controls.Add(Label7)
        Controls.Add(TextBox1)
        Controls.Add(Label5)
        Controls.Add(Cantidad)
        Controls.Add(Label4)
        Controls.Add(Carrier)
        Controls.Add(Label3)
        Controls.Add(Tracking)
        Controls.Add(Label2)
        Controls.Add(SA)
        Controls.Add(Label1)
        Controls.Add(DN)
        Controls.Add(Label6)
        Controls.Add(SO)
        Controls.Add(Imprimir)
        FormBorderStyle = FormBorderStyle.None
        Name = "Embarques"
        Text = "Embarques"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Imprimir As Button
    Friend WithEvents SO As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents DN As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents SA As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Tracking As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Carrier As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Cantidad As TextBox
    Friend WithEvents PrintDocument3 As Printing.PrintDocument
    Friend WithEvents PrintDialog3 As PrintDialog
    Friend WithEvents Label7 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Reimprimir As Button
    Friend WithEvents Fuente As TextBox
    Friend WithEvents PrintPreviewDialog3 As PrintPreviewDialog
    Friend WithEvents Barras As TextBox
    Friend WithEvents fuenteParrafo As TextBox
    Friend WithEvents fuenteAddress As TextBox
End Class

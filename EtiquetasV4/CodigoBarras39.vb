' Dibuja códigos de barras Code 39 con rectángulos (GDI+), sin depender de una fuente instalada.
' Antes se escribía "*VALOR*" con la fuente Libre Barcode 39, pero al imprimir con Microsoft Print to PDF
' (y en máquinas sin la fuente) salían letras en lugar de barras.
'
' Las medidas están en las unidades del Graphics de impresión (1/100 de pulgada) y copian lo que dibujaba
' Libre Barcode 39 a 24 pt, para que el código conserve su tamaño y su posición en la etiqueta.
Public Module CodigoBarras39

    Private Const Angosta As Single = 1.0F       ' barra o espacio angosto
    Private Const Ancha As Single = 3.0F         ' barra o espacio ancho (relación 3:1)
    Private Const Separacion As Single = 1.46F   ' espacio entre caracteres
    Private Const Alto As Single = 19.6F
    Private Const MargenX As Single = 5.56F      ' margen que DrawString dejaba antes de la primera barra
    Private Const MargenY As Single = 0.4F

    ' Nueve elementos por carácter (barra, espacio, barra, ... barra): n = angosto, w = ancho.
    Private ReadOnly Patrones As New Dictionary(Of Char, String) From {
        {"0"c, "nnnwwnwnn"}, {"1"c, "wnnwnnnnw"}, {"2"c, "nnwwnnnnw"}, {"3"c, "wnwwnnnnn"},
        {"4"c, "nnnwwnnnw"}, {"5"c, "wnnwwnnnn"}, {"6"c, "nnwwwnnnn"}, {"7"c, "nnnwnnwnw"},
        {"8"c, "wnnwnnwnn"}, {"9"c, "nnwwnnwnn"},
        {"A"c, "wnnnnwnnw"}, {"B"c, "nnwnnwnnw"}, {"C"c, "wnwnnwnnn"}, {"D"c, "nnnnwwnnw"},
        {"E"c, "wnnnwwnnn"}, {"F"c, "nnwnwwnnn"}, {"G"c, "nnnnnwwnw"}, {"H"c, "wnnnnwwnn"},
        {"I"c, "nnwnnwwnn"}, {"J"c, "nnnnwwwnn"}, {"K"c, "wnnnnnnww"}, {"L"c, "nnwnnnnww"},
        {"M"c, "wnwnnnnwn"}, {"N"c, "nnnnwnnww"}, {"O"c, "wnnnwnnwn"}, {"P"c, "nnwnwnnwn"},
        {"Q"c, "nnnnnnwww"}, {"R"c, "wnnnnnwwn"}, {"S"c, "nnwnnnwwn"}, {"T"c, "nnnnwnwwn"},
        {"U"c, "wwnnnnnnw"}, {"V"c, "nwwnnnnnw"}, {"W"c, "wwwnnnnnn"}, {"X"c, "nwnnwnnnw"},
        {"Y"c, "wwnnwnnnn"}, {"Z"c, "nwwnwnnnn"},
        {"-"c, "nwnnnnwnw"}, {"."c, "wwnnnnwnn"}, {" "c, "nwwnnnwnn"}, {"$"c, "nwnwnwnnn"},
        {"/"c, "nwnwnnnwn"}, {"+"c, "nwnnnwnwn"}, {"%"c, "nnnwnwnwn"}, {"*"c, "nwnnwnwnn"}
    }

    ' Dibuja el código de valor, con los asteriscos de inicio y fin. (x, y) es el mismo punto
    ' que se le pasaba a DrawString con la fuente de código de barras.
    Public Sub Dibujar(g As Graphics, valor As String, x As Single, y As Single)
        Dim posX As Single = x + MargenX
        For Each c As Char In "*" & Normalizar(valor) & "*"
            Dim patron As String = Patrones(c)
            For i As Integer = 0 To 8
                Dim ancho As Single = If(patron(i) = "w"c, Ancha, Angosta)
                If i Mod 2 = 0 Then g.FillRectangle(Brushes.Black, posX, y + MargenY, ancho, Alto)
                posX += ancho
            Next
            posX += Separacion
        Next
    End Sub

    ' Code 39 solo tiene mayúsculas, dígitos y - . espacio $ / + %: se pasa a mayúsculas y se omite lo demás
    ' (incluido el *, que solo marca inicio y fin).
    Public Function Normalizar(valor As String) As String
        Return New String(If(valor, "").ToUpperInvariant().Where(Function(c) c <> "*"c AndAlso Patrones.ContainsKey(c)).ToArray())
    End Function

End Module

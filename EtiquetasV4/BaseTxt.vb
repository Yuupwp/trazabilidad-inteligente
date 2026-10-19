Imports System.IO
Imports System.Text

' Reemplazo de la base de datos SQL Server por un archivo de texto (SOLO PARA DEMOSTRACION).
' Los registros se guardan en Etiquetas\Registros.txt, un registro por linea, campos separados por "|".
Public Module BaseTxt

    Private ReadOnly Ruta As String = Path.Combine(AppContext.BaseDirectory, "Etiquetas", "Registros.txt")
    Private Const Sep As Char = "|"c
    Private Const TotalCampos As Integer = 16
    Private ReadOnly Candado As New Object()

    Private Const Encabezado As String = "# Area|Spot|Asset|WO|Item|Supervisor|No_id|Turno|Copias|Serial|Consecutivo_Global|Consecutivo_Individual|Fecha|Liberado_por|Fecha_liberacion|Modelo_final"

    Private Function Limpiar(valor As String) As String
        If valor Is Nothing Then Return ""
        Return valor.Replace(Sep, "/"c).Replace(vbCr, " ").Replace(vbLf, " ").Trim()
    End Function

    Private Sub AsegurarArchivo()
        Dim carpeta As String = Path.GetDirectoryName(Ruta)
        If Not Directory.Exists(carpeta) Then Directory.CreateDirectory(carpeta)
        If Not File.Exists(Ruta) Then File.WriteAllText(Ruta, Encabezado & vbCrLf, Encoding.UTF8)
    End Sub

    ' Equivale a: SELECT MAX(Consecutivo_Global) FROM Quality_print_records
    Public Function MaxConsecutivoGlobal() As Integer
        SyncLock Candado
            AsegurarArchivo()
            Dim maximo As Integer = 0
            For Each linea As String In File.ReadAllLines(Ruta)
                If linea.StartsWith("#") OrElse linea.Trim() = "" Then Continue For
                Dim p() As String = linea.Split(Sep)
                Dim n As Integer
                If p.Length > 10 AndAlso Integer.TryParse(p(10), n) AndAlso n > maximo Then maximo = n
            Next
            Return maximo
        End SyncLock
    End Function

    ' Equivale a: INSERT INTO Quality_print_records (...)
    Public Sub Insertar(area As String, spot As String, asset As String, wo As String, item As String,
                        supervisor As String, noId As String, turno As String, copias As String,
                        serial As String, consecutivoGlobal As Integer, consecutivoIndividual As Integer,
                        fecha As String)
        SyncLock Candado
            AsegurarArchivo()
            Dim campos() As String = {Limpiar(area), Limpiar(spot), Limpiar(asset), Limpiar(wo), Limpiar(item),
                                      Limpiar(supervisor), Limpiar(noId), Limpiar(turno), Limpiar(copias),
                                      Limpiar(serial), consecutivoGlobal.ToString(), consecutivoIndividual.ToString(),
                                      Limpiar(fecha), "", "", ""}
            File.AppendAllText(Ruta, String.Join(Sep, campos) & vbCrLf, Encoding.UTF8)
        End SyncLock
    End Sub

    ' Equivale a: SELECT WO, Modelo_Item FROM Quality_print_records WHERE Serial = @Serial
    Public Function BuscarPorSerial(serial As String, ByRef wo As String, ByRef item As String) As Boolean
        SyncLock Candado
            AsegurarArchivo()
            For Each linea As String In File.ReadAllLines(Ruta)
                If linea.StartsWith("#") OrElse linea.Trim() = "" Then Continue For
                Dim p() As String = linea.Split(Sep)
                If p.Length > 9 AndAlso String.Equals(p(9), serial.Trim(), StringComparison.OrdinalIgnoreCase) Then
                    wo = p(3)
                    item = p(4)
                    Return True
                End If
            Next
            Return False
        End SyncLock
    End Function

    ' Equivale a: UPDATE Quality_print_records SET Liberado_por, Fecha_liberacion, Modelo_final WHERE Serial = @Serial
    ' Regresa cuantos registros se actualizaron.
    Public Function Liberar(serial As String, liberadoPor As String, fechaLiberacion As String, modeloFinal As String) As Integer
        SyncLock Candado
            AsegurarArchivo()
            Dim lineas As String() = File.ReadAllLines(Ruta)
            Dim actualizados As Integer = 0
            For i As Integer = 0 To lineas.Length - 1
                If lineas(i).StartsWith("#") OrElse lineas(i).Trim() = "" Then Continue For
                Dim lista As New List(Of String)(lineas(i).Split(Sep))
                While lista.Count < TotalCampos
                    lista.Add("")
                End While
                If String.Equals(lista(9), serial.Trim(), StringComparison.OrdinalIgnoreCase) Then
                    lista(13) = Limpiar(liberadoPor)
                    lista(14) = Limpiar(fechaLiberacion)
                    lista(15) = Limpiar(modeloFinal)
                    lineas(i) = String.Join(Sep, lista)
                    actualizados += 1
                End If
            Next
            If actualizados > 0 Then File.WriteAllLines(Ruta, lineas, Encoding.UTF8)
            Return actualizados
        End SyncLock
    End Function

End Module

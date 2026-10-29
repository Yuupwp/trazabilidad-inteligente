Imports System.IO
Imports System.Reflection
Imports System.Security.Cryptography
Imports Microsoft.Data.Sqlite

' Acceso a la base SQLite compartida con el módulo Python (reemplaza a BaseTxt / SQL Server).
' El esquema vive en db\schema.sql (embebido en el ensamblado) y se aplica al abrir la base por primera vez.
'
' Ubicación de la base, en este orden:
'   1. Variable de entorno TRAZA_DB (ruta completa al archivo .db).
'   2. <raíz del repositorio>\datos\trazabilidad.db, buscando EtiquetasV4.sln hacia arriba desde el ejecutable.
'   3. <carpeta del ejecutable>\datos\trazabilidad.db (app publicada fuera del repositorio).
Public Module BaseSqlite

    Private Const FormatoFecha As String = "yyyy-MM-dd HH:mm:ss"
    Private ReadOnly Candado As New Object()
    Private _cadenaConexion As String = Nothing

    Public ReadOnly Property RutaBase As String = ResolverRuta()

    Private Function ResolverRuta() As String
        Dim variable As String = Environment.GetEnvironmentVariable("TRAZA_DB")
        If Not String.IsNullOrWhiteSpace(variable) Then Return Path.GetFullPath(variable)

        Dim carpeta As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
        While carpeta IsNot Nothing
            If File.Exists(Path.Combine(carpeta.FullName, "EtiquetasV4.sln")) Then
                Return Path.Combine(carpeta.FullName, "datos", "trazabilidad.db")
            End If
            carpeta = carpeta.Parent
        End While

        Return Path.Combine(AppContext.BaseDirectory, "datos", "trazabilidad.db")
    End Function

    ' Abre una conexión; la primera vez crea la carpeta y aplica el esquema.
    Private Function Abrir() As SqliteConnection
        SyncLock Candado
            If _cadenaConexion Is Nothing Then
                Directory.CreateDirectory(Path.GetDirectoryName(RutaBase))
                Dim constructor As New SqliteConnectionStringBuilder With {
                    .DataSource = RutaBase,
                    .Mode = SqliteOpenMode.ReadWriteCreate
                }
                Dim cadena As String = constructor.ToString()
                Using conexion As New SqliteConnection(cadena)
                    conexion.Open()
                    Using cmd As SqliteCommand = conexion.CreateCommand()
                        cmd.CommandText = LeerEsquema()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                _cadenaConexion = cadena
            End If
        End SyncLock

        Dim nueva As New SqliteConnection(_cadenaConexion)
        nueva.Open()
        Return nueva
    End Function

    Private Function LeerEsquema() As String
        Using recurso As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("schema.sql")
            If recurso Is Nothing Then Throw New InvalidOperationException("No se encontró el recurso schema.sql en el ensamblado.")
            Using lector As New StreamReader(recurso)
                Return lector.ReadToEnd()
            End Using
        End Using
    End Function

    ' Cadena vacía -> NULL, para no guardar campos vacíos como texto.
    Private Function Valor(texto As String) As Object
        If String.IsNullOrWhiteSpace(texto) Then Return DBNull.Value
        Return texto.Trim()
    End Function

    Public Function MaxConsecutivoGlobal() As Integer
        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText = "SELECT COALESCE(MAX(consecutivo_global), 0) FROM etiquetas"
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
        End Using
    End Function

    ' Lanza SqliteException si el serial ya existe (columna UNIQUE).
    Public Sub Insertar(area As String, spot As String, asset As String, wo As String, item As String,
                        supervisor As String, noId As String, turno As String, copias As String,
                        serial As String, consecutivoGlobal As Integer, consecutivoIndividual As Integer,
                        fecha As DateTime, revision As String)
        Dim numeroCopias As Integer
        Dim copiasValor As Object = If(Integer.TryParse(copias, numeroCopias), CObj(numeroCopias), DBNull.Value)

        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText =
                    "INSERT INTO etiquetas (serial, consecutivo_global, consecutivo_individual, area, spot, asset, wo, item, " &
                    "revision, supervisor, no_id, turno, copias, fecha_impresion) " &
                    "VALUES (@serial, @global, @individual, @area, @spot, @asset, @wo, @item, " &
                    "@revision, @supervisor, @noId, @turno, @copias, @fecha)"
                cmd.Parameters.AddWithValue("@serial", serial.Trim().ToUpper())
                cmd.Parameters.AddWithValue("@global", consecutivoGlobal)
                cmd.Parameters.AddWithValue("@individual", consecutivoIndividual)
                cmd.Parameters.AddWithValue("@area", If(area, "").Trim())
                cmd.Parameters.AddWithValue("@spot", Valor(spot))
                cmd.Parameters.AddWithValue("@asset", Valor(asset))
                cmd.Parameters.AddWithValue("@wo", If(wo, "").Trim())
                cmd.Parameters.AddWithValue("@item", If(item, "").Trim())
                cmd.Parameters.AddWithValue("@revision", Valor(revision))
                cmd.Parameters.AddWithValue("@supervisor", Valor(supervisor))
                cmd.Parameters.AddWithValue("@noId", Valor(noId))
                cmd.Parameters.AddWithValue("@turno", Valor(turno))
                cmd.Parameters.AddWithValue("@copias", copiasValor)
                cmd.Parameters.AddWithValue("@fecha", fecha.ToString(FormatoFecha))
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    ' Busca una etiqueta por serial. estado: IMPRESA, LIBERADA o RECHAZADA.
    Public Function BuscarPorSerial(serial As String, ByRef wo As String, ByRef item As String, ByRef estado As String) As Boolean
        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText = "SELECT wo, item, estado FROM etiquetas WHERE serial = @serial"
                cmd.Parameters.AddWithValue("@serial", serial.Trim())
                Using lector As SqliteDataReader = cmd.ExecuteReader()
                    If Not lector.Read() Then Return False
                    wo = lector.GetString(0)
                    item = lector.GetString(1)
                    estado = lector.GetString(2)
                    Return True
                End Using
            End Using
        End Using
    End Function

    ' Resultado de liberar o rechazar una etiqueta.
    Public Enum ResultadoCierre
        Hecho
        NoExiste
        YaCerrada      ' ya estaba LIBERADA o RECHAZADA; el estado actual se regresa en estadoActual
    End Enum

    ' Solo se libera una etiqueta que sigue en IMPRESA (no se puede liberar dos veces ni liberar una rechazada).
    Public Function Liberar(serial As String, liberadoPor As String, fechaLiberacion As DateTime, modeloFinal As String,
                            ByRef estadoActual As String) As ResultadoCierre
        Return Cerrar(serial, "LIBERADA", liberadoPor, fechaLiberacion, modeloFinal, estadoActual)
    End Function

    ' Marca como RECHAZADA una etiqueta que sigue en IMPRESA.
    Public Function Rechazar(serial As String, rechazadoPor As String, fecha As DateTime, ByRef estadoActual As String) As ResultadoCierre
        Return Cerrar(serial, "RECHAZADA", rechazadoPor, fecha, Nothing, estadoActual)
    End Function

    ' liberado_por y fecha_liberacion guardan quién y cuándo cerró la etiqueta, tanto al liberar como al rechazar.
    ' La condición estado = 'IMPRESA' va en el mismo UPDATE para que dos estaciones no cierren la misma etiqueta.
    Private Function Cerrar(serial As String, nuevoEstado As String, usuario As String, fecha As DateTime,
                            modeloFinal As String, ByRef estadoActual As String) As ResultadoCierre
        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText =
                    "UPDATE etiquetas SET estado = @estado, liberado_por = @usuario, " &
                    "fecha_liberacion = @fecha, modelo_final = @modelo " &
                    "WHERE serial = @serial AND estado = 'IMPRESA'"
                cmd.Parameters.AddWithValue("@estado", nuevoEstado)
                cmd.Parameters.AddWithValue("@usuario", Valor(usuario))
                cmd.Parameters.AddWithValue("@fecha", fecha.ToString(FormatoFecha))
                cmd.Parameters.AddWithValue("@modelo", Valor(modeloFinal))
                cmd.Parameters.AddWithValue("@serial", serial.Trim())
                If cmd.ExecuteNonQuery() > 0 Then
                    estadoActual = nuevoEstado
                    Return ResultadoCierre.Hecho
                End If
            End Using

            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText = "SELECT estado FROM etiquetas WHERE serial = @serial"
                cmd.Parameters.AddWithValue("@serial", serial.Trim())
                Dim estado As Object = cmd.ExecuteScalar()
                If estado Is Nothing OrElse estado Is DBNull.Value Then Return ResultadoCierre.NoExiste
                estadoActual = CStr(estado)
                Return ResultadoCierre.YaCerrada
            End Using
        End Using
    End Function

    ' ------------------------------------------------------------ usuarios de QA
    ' Contraseñas con PBKDF2-HMAC-SHA256, 100 000 iteraciones, sal aleatoria de 16 bytes y hash de 32 bytes,
    ' ambos guardados en Base64. En Python: hashlib.pbkdf2_hmac("sha256", pwd.encode("utf-8"), sal, 100000, 32).
    ' Si se cambian estos parámetros, las contraseñas guardadas dejan de validar.
    Private Const Iteraciones As Integer = 100000
    Private Const BytesSal As Integer = 16
    Private Const BytesHash As Integer = 32

    Private Function CalcularHash(contraseña As String, sal As Byte()) As Byte()
        Return Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash)
    End Function

    Public Function HayUsuarios() As Boolean
        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM usuarios)"
                Return Convert.ToInt32(cmd.ExecuteScalar()) = 1
            End Using
        End Using
    End Function

    ' Regresa False si el usuario ya existe.
    Public Function CrearUsuario(usuario As String, contraseña As String, Optional rol As String = "QA") As Boolean
        Dim sal As Byte() = RandomNumberGenerator.GetBytes(BytesSal)
        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText =
                    "INSERT INTO usuarios (usuario, password_hash, salt, rol, creado) " &
                    "VALUES (@usuario, @hash, @sal, @rol, @creado) ON CONFLICT (usuario) DO NOTHING"
                cmd.Parameters.AddWithValue("@usuario", usuario.Trim())
                cmd.Parameters.AddWithValue("@hash", Convert.ToBase64String(CalcularHash(contraseña, sal)))
                cmd.Parameters.AddWithValue("@sal", Convert.ToBase64String(sal))
                cmd.Parameters.AddWithValue("@rol", rol)
                cmd.Parameters.AddWithValue("@creado", DateTime.Now.ToString(FormatoFecha))
                Return cmd.ExecuteNonQuery() > 0
            End Using
        End Using
    End Function

    Public Function ValidarUsuario(usuario As String, contraseña As String) As Boolean
        Using conexion As SqliteConnection = Abrir()
            Using cmd As SqliteCommand = conexion.CreateCommand()
                cmd.CommandText = "SELECT password_hash, salt FROM usuarios WHERE usuario = @usuario"
                cmd.Parameters.AddWithValue("@usuario", If(usuario, "").Trim())
                Using lector As SqliteDataReader = cmd.ExecuteReader()
                    If Not lector.Read() Then Return False
                    Dim guardado As Byte() = Convert.FromBase64String(lector.GetString(0))
                    Dim sal As Byte() = Convert.FromBase64String(lector.GetString(1))
                    Return CryptographicOperations.FixedTimeEquals(CalcularHash(If(contraseña, ""), sal), guardado)
                End Using
            End Using
        End Using
    End Function

End Module

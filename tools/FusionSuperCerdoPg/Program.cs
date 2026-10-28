// Copia la base SuperCerdo unificada de SQL Server a un esquema de trabajo de carnisys en Postgres (por defecto "stg").
// Por cada tabla que existe en los dos motores crea stg.<tabla> (LIKE public.<tabla>, mismas columnas que carnisys hoy),
// la carga por COPY binario y despues compara contra el origen (filas y estadisticas por columna; --deep agrega un
// hash de cada valor). No toca public ni ninguna otra base: el volcado a la empresa 1 lo hace 05-volcar-a-carnisys.sql.
// Configuracion por variables de entorno (nada hardcodeado ni impreso):
//   FUSION_SS_CONN          cadena de conexion de SQL Server (origen, solo lectura)
//   FUSION_PG_CONN          cadena de conexion de Postgres SIN password (superusuario: la carga bypasea RLS)
//   FUSION_PG_PASSWORD      password de ese usuario (aparte: puede tener ';' que rompe la cadena)
//   FUSION_PG_DB            nombre de la base destino esperada (guarda contra cargar en otra base por error)
// Argumentos: [--schema stg] [--verify-only] [--deep]. Ver docs/DECISIONS.md "Unificacion SuperCerdo SM + SL".
using System.Globalization;
using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;

string schema = ArgValue("--schema") ?? "stg";
bool verifyOnly = args.Contains("--verify-only");
bool deep = args.Contains("--deep");
if (schema is "public" or "" || schema.Contains('"')) throw new InvalidOperationException("El esquema de trabajo no puede ser public");

string ssConn = Env("FUSION_SS_CONN");
string pgConn = new NpgsqlConnectionStringBuilder(Env("FUSION_PG_CONN")) { Password = Env("FUSION_PG_PASSWORD") }.ConnectionString;
string expectedDb = Env("FUSION_PG_DB");

// Columnas de Postgres que en SQL Server se llaman distinto: (tabla, columna PG) -> columna SQL Server.
var renamed = new Dictionary<(string, string), string>
{
    [("empresas", "empresa_propia")] = "empresapropia",
    [("empresas", "es_carniceria")] = "escarniceria",
    [("usuarios", "fechabloqueonosegurautc")] = "fechaBloqueoNoSeguroUtc",   // "NoSegura" en Postgres, "NoSeguro" en SQL Server
};

var avisos = new List<string>();
var diferencias = new List<string>();

await using var pg = new NpgsqlConnection(pgConn);
await pg.OpenAsync();
await using var ss = new SqlConnection(ssConn);
await ss.OpenAsync();
string db = (string)(await Scalar(pg, "select current_database()"))!;
if (db != expectedDb) throw new InvalidOperationException($"Base destino inesperada: {db} (se esperaba {expectedDb})");

// ---- catalogos: tablas de public (carnisys) y de SQL Server ----
var ssTablas = new Dictionary<string, (string nombre, Dictionary<string, string> cols)>(StringComparer.OrdinalIgnoreCase);
using (var cmd = new SqlCommand(@"select t.name, c.name, ty.name from sys.columns c join sys.tables t on t.object_id = c.object_id
    join sys.types ty on ty.user_type_id = c.user_type_id where t.is_ms_shipped = 0 and t.schema_id = schema_id('dbo') order by t.name, c.column_id", ss) { CommandTimeout = 0 })
using (var r = cmd.ExecuteReader())
    while (r.Read())
    {
        if (!ssTablas.TryGetValue(r.GetString(0), out var t)) ssTablas[r.GetString(0)] = t = (r.GetString(0), new(StringComparer.OrdinalIgnoreCase));
        t.cols[r.GetString(1)] = r.GetString(2);
    }

var pgTablas = new List<string>();
await using (var cmd = new NpgsqlCommand("select table_name from information_schema.tables where table_schema = 'public' and table_type = 'BASE TABLE' order by 1", pg))
await using (var r = await cmd.ExecuteReaderAsync())
    while (await r.ReadAsync()) pgTablas.Add(r.GetString(0));

var comunes = pgTablas.Where(t => ssTablas.ContainsKey(t)).ToList();
foreach (var t in pgTablas.Where(t => !ssTablas.ContainsKey(t))) avisos.Add($"SOLO EN POSTGRES (no se copia): {t}");
foreach (var t in ssTablas.Keys.Where(t => !pgTablas.Contains(t.ToLowerInvariant()))) avisos.Add($"SOLO EN SQL SERVER (no se copia): {t}");

// ---- esquema de trabajo: stg.<tabla> con las columnas de public.<tabla> ----
if (!verifyOnly)
{
    await Exec(pg, $"create schema if not exists {Q(schema)}");
    foreach (var t in comunes)
        await Exec(pg, $"drop table if exists {Q(schema)}.{Q(t)}; create table {Q(schema)}.{Q(t)} (like public.{Q(t)} including defaults)");
}

// columnas de cada tabla de trabajo
var planes = new List<Plan>();
foreach (var t in comunes)
{
    var cols = new List<Col>();
    await using (var cmd = new NpgsqlCommand(@"select column_name, udt_name, is_nullable = 'YES', column_default is not null
        from information_schema.columns where table_schema = @s and table_name = @t order by ordinal_position", pg))
    {
        cmd.Parameters.AddWithValue("s", schema); cmd.Parameters.AddWithValue("t", t);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) cols.Add(new Col(r.GetString(0), r.GetString(1), r.GetBoolean(2), r.GetBoolean(3), null));
    }
    var src = ssTablas[t];
    var mapeadas = new List<Col>();
    foreach (var c in cols)
    {
        string origen = renamed.TryGetValue((t, c.Nombre), out var rn) ? rn : c.Nombre;
        string? real = src.cols.Keys.FirstOrDefault(k => k.Equals(origen, StringComparison.OrdinalIgnoreCase));
        if (real != null) mapeadas.Add(c with { Origen = real });
        // tabla de SQL Server sin idEmpresa (esquema viejo o tabla nueva sin multiempresa): todo es de SuperCerdo, la empresa 1
        else if (c.Nombre == "idempresa") { mapeadas.Add(c with { Origen = null }); avisos.Add($"IDEMPRESA FIJA = 1: {t} (no tiene idEmpresa en SQL Server)"); }
        else if (!c.Nullable && !c.TieneDefault) throw new InvalidOperationException($"{t}.{c.Nombre}: NOT NULL sin default y sin columna en SQL Server");
        else avisos.Add($"COLUMNA SIN ORIGEN (queda con su default): {t}.{c.Nombre}");
    }
    var usadas = mapeadas.Where(m => m.Origen != null).Select(m => m.Origen!).ToHashSet(StringComparer.OrdinalIgnoreCase);
    foreach (var extra in src.cols.Keys.Where(k => !usadas.Contains(k))) avisos.Add($"COLUMNA DESCARTADA (no existe en carnisys): {src.nombre}.{extra}");
    planes.Add(new Plan(t, src.nombre, mapeadas));
}

// ---- carga: una sola transaccion ----
if (!verifyOnly)
{
    await using var tx = await pg.BeginTransactionAsync();
    foreach (var p in planes)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long n = await Copiar(pg, ss, schema, p, avisos);
        Console.WriteLine($"  {p.Tabla,-38} {n,9} filas ({sw.Elapsed.TotalSeconds:0.0}s)");
    }
    await tx.CommitAsync();
    Console.WriteLine("COMMIT ok");
}

// ---- verificacion ----
foreach (var p in planes)
{
    long a = Convert.ToInt64(await ScalarSs(ss, $"select count_big(*) from [{p.Origen}]"));
    long b = Convert.ToInt64(await Scalar(pg, $"select count(*) from {Q(schema)}.{Q(p.Tabla)}"));
    if (a != b) diferencias.Add($"FILAS {p.Tabla}: SQL Server={a} Postgres={b}");
    foreach (var c in p.Cols) await CompararColumna(pg, ss, schema, p, c, diferencias);
    if (deep) await CompararHash(pg, ss, schema, p, diferencias);
}
Console.WriteLine($"\n== Verificacion: {planes.Count} tablas{(deep ? " (con hash de cada valor)" : "")} ==");
Console.WriteLine("== Avisos =="); foreach (var w in avisos.Distinct()) Console.WriteLine("  " + w);
Console.WriteLine("== Diferencias =="); if (diferencias.Count == 0) Console.WriteLine("  (ninguna)");
foreach (var d in diferencias) Console.WriteLine("  " + d);
return diferencias.Count == 0 ? 0 : 2;

// =====================================================================================
string? ArgValue(string name) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
static string Env(string n) => Environment.GetEnvironmentVariable(n) is { Length: > 0 } v ? v : throw new InvalidOperationException($"Falta la variable de entorno {n}");
static string Q(string id) => "\"" + id.Replace("\"", "\"\"") + "\"";
static async Task<object?> Scalar(NpgsqlConnection c, string sql)
{ await using var cmd = new NpgsqlCommand(sql, c) { CommandTimeout = 0 }; var v = await cmd.ExecuteScalarAsync(); return v is DBNull ? null : v; }
static async Task<object?> ScalarSs(SqlConnection c, string sql)
{ using var cmd = new SqlCommand(sql, c) { CommandTimeout = 0 }; var v = await cmd.ExecuteScalarAsync(); return v is DBNull ? null : v; }
static async Task Exec(NpgsqlConnection c, string sql) { await using var cmd = new NpgsqlCommand(sql, c) { CommandTimeout = 0 }; await cmd.ExecuteNonQueryAsync(); }

// Valor neutro para una columna NOT NULL de Postgres cuyo origen trae NULL.
static object Neutro(string udt, string clave) => udt switch
{
    "bool" => false, "int2" => (short)0, "int4" => 0, "int8" => 0L, "float4" => 0f, "float8" => 0d, "numeric" => 0m,
    "text" or "varchar" or "bpchar" => "",
    _ => throw new InvalidDataException($"NULL en columna NOT NULL sin valor neutro: {clave} ({udt})"),
};

// Convierte a entero sin perder decimales.
static long Entero(object raw, string clave) => raw switch
{
    decimal d when d != decimal.Truncate(d) => throw new InvalidDataException($"{clave}: {d} no es entero"),
    double f when f != Math.Truncate(f) => throw new InvalidDataException($"{clave}: {f} no es entero"),
    bool b => b ? 1 : 0,
    _ => Convert.ToInt64(raw, CultureInfo.InvariantCulture),
};

// Copia una tabla de SQL Server a stg por COPY binario. Devuelve las filas escritas.
static async Task<long> Copiar(NpgsqlConnection pg, SqlConnection ss, string schema, Plan p, List<string> avisos)
{
    string sel = "select " + string.Join(", ", p.Cols.Select(c => SsExpr(c))) + $" from [{p.Origen}]";
    string copy = $"copy {Q(schema)}.{Q(p.Tabla)} ({string.Join(", ", p.Cols.Select(c => Q(c.Nombre)))}) from stdin (format binary)";
    var rellenos = new Dictionary<string, long>(); long nul = 0, filas = 0;
    using var cmd = new SqlCommand(sel, ss) { CommandTimeout = 0 };
    using var r = cmd.ExecuteReader();
    await using var w = await pg.BeginBinaryImportAsync(copy);
    while (r.Read())
    {
        await w.StartRowAsync();
        for (int i = 0; i < p.Cols.Count; i++)
        {
            var c = p.Cols[i]; string clave = $"{p.Tabla}.{c.Nombre}";
            object? v = r.IsDBNull(i) ? null : r.GetValue(i);
            if (v == null && !c.Nullable) { rellenos[clave] = rellenos.GetValueOrDefault(clave) + 1; v = Neutro(c.Udt, clave); }
            if (v == null) { await w.WriteNullAsync(); continue; }
            switch (c.Udt)
            {
                case "bool": await w.WriteAsync(v is bool b ? b : Convert.ToInt64(v, CultureInfo.InvariantCulture) != 0, NpgsqlDbType.Boolean); break;
                case "int2": await w.WriteAsync(checked((short)Entero(v, clave)), NpgsqlDbType.Smallint); break;
                case "int4": await w.WriteAsync(checked((int)Entero(v, clave)), NpgsqlDbType.Integer); break;
                case "int8": await w.WriteAsync(Entero(v, clave), NpgsqlDbType.Bigint); break;
                case "float4": await w.WriteAsync(Convert.ToSingle(v, CultureInfo.InvariantCulture), NpgsqlDbType.Real); break;
                case "float8": await w.WriteAsync(Convert.ToDouble(v, CultureInfo.InvariantCulture), NpgsqlDbType.Double); break;
                case "numeric": await w.WriteAsync(Convert.ToDecimal(v, CultureInfo.InvariantCulture), NpgsqlDbType.Numeric); break;
                case "text": case "varchar": case "bpchar":
                {
                    string s = v is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : Convert.ToString(v) ?? "";
                    // Postgres no admite el caracter NUL en texto: se quita y se cuenta
                    if (s.Contains('\0')) { nul += s.Count(ch => ch == '\0'); s = s.Replace("\0", ""); }
                    await w.WriteAsync(s, c.Udt == "bpchar" ? NpgsqlDbType.Char : c.Udt == "varchar" ? NpgsqlDbType.Varchar : NpgsqlDbType.Text); break;
                }
                case "uuid": await w.WriteAsync((Guid)v, NpgsqlDbType.Uuid); break;
                case "timestamp": await w.WriteAsync(DateTime.SpecifyKind((DateTime)v, DateTimeKind.Unspecified), NpgsqlDbType.Timestamp); break;
                // las columnas timestamptz de carnisys guardan UTC: el origen se interpreta como UTC
                case "timestamptz": await w.WriteAsync(DateTime.SpecifyKind((DateTime)v, DateTimeKind.Utc), NpgsqlDbType.TimestampTz); break;
                case "date": await w.WriteAsync(DateOnly.FromDateTime((DateTime)v), NpgsqlDbType.Date); break;
                case "time": await w.WriteAsync(v is TimeSpan ts ? TimeOnly.FromTimeSpan(ts) : TimeOnly.FromDateTime((DateTime)v), NpgsqlDbType.Time); break;
                case "bytea": await w.WriteAsync((byte[])v, NpgsqlDbType.Bytea); break;
                default: throw new NotSupportedException($"Tipo destino no soportado: {clave} ({c.Udt})");
            }
        }
        filas++;
    }
    await w.CompleteAsync();
    foreach (var (k, n) in rellenos) avisos.Add($"NULL -> NEUTRO: {k} = {n} filas (NOT NULL en carnisys, NULL en SQL Server)");
    if (nul > 0) avisos.Add($"NUL QUITADOS: {p.Tabla} = {nul}");
    return filas;
}

// Estadisticas por columna: no-nulos y (segun tipo) suma, largo total de texto o fecha minima.
static async Task CompararColumna(NpgsqlConnection pg, SqlConnection ss, string schema, Plan p, Col c, List<string> dif)
{
    string sc = SsExpr(c), pc = Q(c.Nombre), etiqueta = $"{p.Tabla}.{c.Nombre}";
    long a = Convert.ToInt64(await ScalarSs(ss, $"select count_big({sc}) from [{p.Origen}]"));
    long b = Convert.ToInt64(await Scalar(pg, $"select count({pc}) from {Q(schema)}.{Q(p.Tabla)}"));
    if (a != b && c.Nullable) { dif.Add($"NO-NULOS {etiqueta}: SQL Server={a} Postgres={b}"); return; }
    if (a != b) return; // NOT NULL rellenado a proposito: ya esta en los avisos
    (string, string)? st = c.Udt switch
    {
        "bool" => ($"sum(case when {sc} <> 0 then 1 else 0 end)", $"count(*) filter (where {pc})"),
        "int2" or "int4" or "int8" or "float4" or "float8" or "numeric" => ($"sum(cast({sc} as float))", $"sum({pc}::float8)"),
        // largo en caracteres UTF-16 (el de SQL Server): los emojis cuentan doble en los dos lados
        "text" or "varchar" or "bpchar" => ($"sum(cast(len({sc} + 'x') - 1 as bigint))", $"sum(char_length({pc}) + char_length(regexp_replace({pc}, '[^\U00010000-\U0010FFFF]', '', 'g')))"),
        "timestamp" or "timestamptz" or "date" => ($"min({sc})", $"min({pc})"),
        _ => null,
    };
    if (st == null) return;
    object? x = await ScalarSs(ss, $"select {st.Value.Item1} from [{p.Origen}]");
    object? y = await Scalar(pg, $"select {st.Value.Item2} from {Q(schema)}.{Q(p.Tabla)}");
    if (!Iguales(x, y)) dif.Add($"ESTADISTICA {etiqueta} ({c.Udt}): SQL Server={x} Postgres={y}");
}

static bool Iguales(object? a, object? b)
{
    // tabla vacia: SUM da NULL en SQL Server y 0 en Postgres (count(*) filter)
    if (a == null && b != null && b is not DateTime && Convert.ToDouble(b, CultureInfo.InvariantCulture) == 0) return true;
    if (a == null || b == null) return a == null && b == null;
    // Postgres guarda microsegundos; datetime2 de SQL Server, 100 ns: se compara al microsegundo
    if (a is DateTime da && b is DateTime db) return da.Ticks / 10 == db.Ticks / 10;
    if (a is DateTime d2 && b is DateOnly dob) return DateOnly.FromDateTime(d2) == dob;
    double x = Convert.ToDouble(a, CultureInfo.InvariantCulture), y = Convert.ToDouble(b, CultureInfo.InvariantCulture);
    return Math.Abs(x - y) <= 1e-9 * Math.Max(1, Math.Max(Math.Abs(x), Math.Abs(y)));
}

// Hash de contenido: por columna, suma (sin orden) de un FNV-1a de 64 bits del valor canonico.
static async Task CompararHash(NpgsqlConnection pg, SqlConnection ss, string schema, Plan p, List<string> dif)
{
    var hs = new ulong[p.Cols.Count]; var hp = new ulong[p.Cols.Count];
    using (var cmd = new SqlCommand("select " + string.Join(", ", p.Cols.Select(c => SsExpr(c))) + $" from [{p.Origen}]", ss) { CommandTimeout = 0 })
    using (var r = cmd.ExecuteReader())
        while (r.Read()) for (int i = 0; i < p.Cols.Count; i++) hs[i] += Fnv(Canon(r.IsDBNull(i) ? null : r.GetValue(i), p.Cols[i]));
    await using (var cmd = new NpgsqlCommand("select " + string.Join(", ", p.Cols.Select(c => Q(c.Nombre))) + $" from {Q(schema)}.{Q(p.Tabla)}", pg) { CommandTimeout = 0 })
    await using (var r = await cmd.ExecuteReaderAsync())
        while (await r.ReadAsync()) for (int i = 0; i < p.Cols.Count; i++) hp[i] += Fnv(Canon(r.IsDBNull(i) ? null : r.GetValue(i), p.Cols[i]));
    for (int i = 0; i < p.Cols.Count; i++)
        if (hs[i] != hp[i]) dif.Add($"CONTENIDO {p.Tabla}.{p.Cols[i].Nombre} ({p.Cols[i].Udt}): el hash de valores difiere");
}

// Representacion canonica de un valor (de cualquiera de los dos motores) segun el tipo destino.
static string Canon(object? v, Col c)
{
    if (v == null && !c.Nullable) v = Neutro(c.Udt, c.Nombre);
    if (v == null) return "<NULL>";
    var inv = CultureInfo.InvariantCulture;
    return c.Udt switch
    {
        "bool" => (v is bool b ? b : Convert.ToInt64(v, inv) != 0) ? "1" : "0",
        "int2" or "int4" or "int8" => Entero(v, c.Nombre).ToString(inv),
        "float4" => Convert.ToSingle(v, inv).ToString("R", inv),
        "float8" => Convert.ToDouble(v, inv).ToString("R", inv),
        "numeric" => (Convert.ToDecimal(v, inv) / 1.0000000000000000000000000000m).ToString(inv),
        "text" or "varchar" or "bpchar" => (v is IFormattable f ? f.ToString(null, inv) : Convert.ToString(v) ?? "").Replace("\0", "").TrimEnd(c.Udt == "bpchar" ? ' ' : '\0'),
        "uuid" => ((Guid)v).ToString(),
        // resolucion de microsegundo (la de Postgres)
        "timestamp" or "timestamptz" => (((DateTime)v).Ticks / 10).ToString(inv),
        "date" => (v is DateOnly d ? d : DateOnly.FromDateTime((DateTime)v)).DayNumber.ToString(inv),
        "time" => (v is TimeOnly t ? t.Ticks / 10 : v is TimeSpan ts ? ts.Ticks / 10 : ((DateTime)v).TimeOfDay.Ticks / 10).ToString(inv),
        "bytea" => Convert.ToHexString((byte[])v),
        _ => throw new NotSupportedException(c.Udt),
    };
}

static ulong Fnv(string s) { ulong h = 14695981039346656037UL; foreach (char ch in s) { h ^= ch; h *= 1099511628211UL; } return h; }

// Expresion de SQL Server que da el valor de la columna: la columna de origen, o 1 para un idempresa que el origen no tiene.
static string SsExpr(Col c) => c.Origen is null ? "1" : $"[{c.Origen}]";

// Origen = columna de SQL Server; null solo para idempresa fijo en 1 (ver SsExpr).
record Col(string Nombre, string Udt, bool Nullable, bool TieneDefault, string? Origen);
record Plan(string Tabla, string Origen, List<Col> Cols);

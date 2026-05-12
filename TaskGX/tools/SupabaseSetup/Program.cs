using Npgsql;

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("SUPABASE_DB_CONNECTION nao foi configurada.");
    return 1;
}

connectionString = NormalizePostgresConnectionString(connectionString);

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var schemaPath = Path.Combine(root, "database", "supabase", "schema.sql");
var seedPath = Path.Combine(root, "database", "supabase", "seed_prioridades.sql");

await using var dataSource = NpgsqlDataSource.Create(connectionString);
await using var connection = await dataSource.OpenConnectionAsync();

Console.WriteLine("Conexao com Supabase aberta.");

await ExecuteSqlFileAsync(connection, schemaPath);
await ExecuteSqlFileAsync(connection, seedPath);

Console.WriteLine("Schema e prioridades aplicados com sucesso.");
return 0;

static async Task ExecuteSqlFileAsync(NpgsqlConnection connection, string path)
{
    var sql = await File.ReadAllTextAsync(path);
    await using var command = new NpgsqlCommand(sql, connection)
    {
        CommandTimeout = 60
    };

    await command.ExecuteNonQueryAsync();
    Console.WriteLine($"Executado: {Path.GetFileName(path)}");
}

static string NormalizePostgresConnectionString(string connectionString)
{
    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgresql" && uri.Scheme != "postgres"))
    {
        return connectionString;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2)
        throw new InvalidOperationException("A connection string PostgreSQL precisa conter usuario e senha.");

    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = Uri.UnescapeDataString(userInfo[1]);
    var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
    var port = uri.Port > 0 ? uri.Port : 5432;

    return string.Join(';', new[]
    {
        $"Host={uri.Host}",
        $"Port={port}",
        $"Database={database}",
        $"Username={username}",
        $"Password={password}",
        "SSL Mode=Require",
        "Trust Server Certificate=true"
    });
}

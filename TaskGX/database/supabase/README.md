# Supabase

Este projeto usa PostgreSQL no Supabase via `Npgsql.EntityFrameworkCore.PostgreSQL`.

## Configurar a connection string

Use uma variavel de ambiente para nao gravar a senha no repositorio.
O projeto aceita tanto o formato URI do Supabase quanto o formato ADO.NET do Npgsql.

Formato URI:

```powershell
$env:SUPABASE_DB_CONNECTION="postgresql://postgres.YOUR_PROJECT_REF:YOUR_SUPABASE_DATABASE_PASSWORD@YOUR_SUPABASE_HOST:5432/postgres"
```

Formato ADO.NET:

```powershell
$env:SUPABASE_DB_CONNECTION="Host=YOUR_SUPABASE_HOST;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT_REF;Password=YOUR_SUPABASE_DATABASE_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

Tambem funciona com a chave padrao do .NET:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=YOUR_SUPABASE_HOST;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT_REF;Password=YOUR_SUPABASE_DATABASE_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

Para desenvolvimento local, prefira user-secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://postgres.YOUR_PROJECT_REF:YOUR_SUPABASE_DATABASE_PASSWORD@YOUR_SUPABASE_HOST:5432/postgres"
```

No Supabase, copie o host e a senha em Project Settings > Database. Para desenvolvimento local, a conexao direta ou o session pooler funcionam melhor para Entity Framework.

## Criar o schema

Abra o SQL Editor do Supabase e execute `database/supabase/schema.sql`.

Para uma base nova, execute tambem `database/supabase/seed_prioridades.sql`.
Se voce for importar a tabela `Prioridades` do MySQL, pule esse seed.

Tambem existe um runner local para aplicar os dois scripts:

```powershell
$env:SUPABASE_DB_CONNECTION="Host=YOUR_SUPABASE_HOST;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT_REF;Password=YOUR_SUPABASE_DATABASE_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
dotnet run --project tools/SupabaseSetup/SupabaseSetup.csproj
```

## Migrar dados do MySQL

Exporte as tabelas do MySQL nesta ordem para manter as chaves estrangeiras:

1. `Usuarios`
2. `Prioridades`
3. `Listas`
4. `Tarefas`

Ao importar no PostgreSQL, preserve os IDs originais. Depois da importacao, execute `database/supabase/sync_sequences.sql`.

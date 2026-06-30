using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using TaskGX.API.Repositories;
using TaskGX.API.Services;
using TaskGX.Data;

var builder = WebApplication.CreateBuilder(args);

var stringConexao = ObterStringConexaoPostgres(builder.Configuration);

builder.Services.AddDbContext<TaskGXContext>(options =>
    options.UseNpgsql(stringConexao, opcoesNpgsql =>
        opcoesNpgsql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null)));

builder.Services
    .AddOptions<ConfiguracoesJwt>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var opcoesGoogleAuth = builder.Services
    .AddOptions<ConfiguracoesGoogleAuth>()
    .Bind(builder.Configuration.GetSection("GoogleAuth"));

if (!builder.Environment.IsDevelopment())
{
    opcoesGoogleAuth
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

builder.Services
    .AddOptions<ConfiguracoesEmail>()
    .Bind(builder.Configuration.GetSection("ConfiguracoesEmail"))
    .ValidateDataAnnotations();

builder.Services.AddProblemDetails();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = contexto =>
    {
        var problemaValidacao = new ValidationProblemDetails(contexto.ModelState)
        {
            Title = "A requisicao contem dados invalidos.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        return new BadRequestObjectResult(problemaValidacao);
    };
});

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<CadastroService>();
builder.Services.AddScoped<VerificacaoService>();
builder.Services.AddScoped<AlteracaoEmailService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<EnvioEmailService>();
builder.Services.AddScoped<TokenService>();

var origensCorsPermitidas = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()?
    .Where(origem => !string.IsNullOrWhiteSpace(origem))
    .Select(origem => origem.Trim())
    .ToArray();

if (origensCorsPermitidas is null || origensCorsPermitidas.Length == 0)
{
    origensCorsPermitidas =
    [
        "http://localhost:5173",
        "https://localhost:5173",
        "http://127.0.0.1:5173",
        "https://127.0.0.1:5173"
    ];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", politica =>
        politica.WithOrigins(origensCorsPermitidas)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var configuracoesJwt = builder.Configuration.GetSection("Jwt").Get<ConfiguracoesJwt>()
    ?? throw new InvalidOperationException("Configuracoes JWT nao encontradas.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracoesJwt.Emissor,
            ValidateAudience = true,
            ValidAudience = configuracoesJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracoesJwt.Chave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var esquemaBearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Cole apenas o token JWT.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", esquemaBearer);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler(aplicacaoExcecao =>
{
    aplicacaoExcecao.Run(async contexto =>
    {
        var recursoExcecao = contexto.Features.Get<IExceptionHandlerPathFeature>();
        var excecao = recursoExcecao?.Error;
        var registrador = contexto.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("TratadorGlobalExcecao");

        if (excecao != null)
        {
            registrador.LogError(
                excecao,
                "Erro nao tratado em {Metodo} {Caminho}",
                contexto.Request.Method,
                contexto.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocorreu um erro interno ao processar a requisicao."
        };

        if (app.Environment.IsDevelopment() && excecao != null)
        {
            problema.Detail = excecao.Message;
            problema.Extensions["tipoExcecao"] = excecao.GetType().FullName;
        }

        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await contexto.Response.WriteAsJsonAsync(problema);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapGet("/swagger/v1/swagger-corrigido.json", (ISwaggerProvider provedorSwagger) =>
    {
        var documentoSwagger = provedorSwagger.GetSwagger("v1");
        using var escritorTexto = new StringWriter();
        var escritorOpenApi = new OpenApiJsonWriter(escritorTexto);

        documentoSwagger.SerializeAsV3(escritorOpenApi);

        var jsonSwagger = CorrigirRequisitosSegurancaSwagger(escritorTexto.ToString());
        return Results.Text(jsonSwagger, "application/json");
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger-corrigido.json", "TaskGX v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    nome = "TaskGX API",
    status = "ok",
    ambiente = app.Environment.EnvironmentName
}));

app.MapControllers();
app.Run();

static string CorrigirRequisitosSegurancaSwagger(string jsonSwagger)
{
    return Regex.Replace(
        jsonSwagger,
        "\"security\"\\s*:\\s*\\[\\s*\\{\\s*\\}\\s*\\]",
        "\"security\": [{\"Bearer\": []}]");
}

static string ObterStringConexaoPostgres(IConfiguration configuration)
{
    var stringConexaoSupabase = configuration["SUPABASE_DB_CONNECTION"];
    var stringConexao = !string.IsNullOrWhiteSpace(stringConexaoSupabase)
        ? stringConexaoSupabase
        : configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(stringConexao))
        throw new InvalidOperationException("String de conexao 'DefaultConnection' nao encontrada.");

    return NormalizarStringConexaoPostgres(stringConexao);
}

static string NormalizarStringConexaoPostgres(string stringConexao)
{
    if (!Uri.TryCreate(stringConexao, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgresql" && uri.Scheme != "postgres"))
    {
        return stringConexao;
    }

    var dadosUsuario = uri.UserInfo.Split(':', 2);
    if (dadosUsuario.Length != 2)
        throw new InvalidOperationException("A connection string PostgreSQL precisa conter usuario e senha.");

    var usuario = Uri.UnescapeDataString(dadosUsuario[0]);
    var senha = Uri.UnescapeDataString(dadosUsuario[1]);
    var banco = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
    var porta = uri.Port > 0 ? uri.Port : 5432;

    return string.Join(';', new[]
    {
        $"Host={uri.Host}",
        $"Port={porta}",
        $"Database={banco}",
        $"Username={usuario}",
        $"Password={senha}",
        "SSL Mode=Require",
        "Trust Server Certificate=true"
    });
}

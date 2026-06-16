# TaskGX API

TaskGX API e a API REST central do projeto TaskGX. Ela fornece os recursos
necessarios para autenticar utilizadores, criar contas, verificar email e gerir
listas, tarefas e prioridades. A API foi preparada para ser consumida por uma
aplicacao web em React e tambem por uma aplicacao desktop que reutilize os
mesmos dados e regras de autenticacao.

## Tecnologias

- ASP.NET Core Web API com .NET 10
- JWT Authentication para proteger endpoints privados
- Entity Framework Core
- PostgreSQL/Supabase com provider Npgsql
- Swagger/OpenAPI em ambiente de desenvolvimento

## Funcionalidades principais

- Autenticacao com email e senha por JWT
- Registo/cadastro de utilizadores
- Verificacao de email por codigo
- Reenvio de codigo de verificacao
- Consulta e atualizacao do utilizador autenticado
- Alteracao de senha e solicitacao/confirmacao de alteracao de email
- Gestao de listas do utilizador
- Gestao de tarefas por lista
- Consulta de prioridades
- Documentacao interativa com Swagger

## Configuracao segura

Credenciais sensiveis nao devem ficar no `appsettings.json` versionado. Use
User Secrets em desenvolvimento local ou variaveis de ambiente em ambientes de
execucao.

O projeto contem um ficheiro de referencia em
`TaskGX/appsettings.example.json`, apenas com placeholders. Copie a estrutura
necessaria para o seu ambiente local e configure valores reais fora do
repositorio.

Valores principais:

- `ConnectionStrings:DefaultConnection` ou variavel `SUPABASE_DB_CONNECTION`
- `Jwt:Chave`
- `Jwt:Emissor`
- `Jwt:Audiencia`
- `Jwt:MinutosExpiracao`
- `GoogleAuth:ClientId`
- `ConfiguracoesEmail:Host`
- `ConfiguracoesEmail:Porta`
- `ConfiguracoesEmail:NomeUsuario`
- `ConfiguracoesEmail:Senha`
- `ConfiguracoesEmail:EmailRemetente`
- `ConfiguracoesEmail:NomeRemetente`
- `ConfiguracoesEmail:HabilitarSsl`
- `Cors:AllowedOrigins`

Exemplo com User Secrets:

```powershell
cd TaskGX
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_DATABASE_CONNECTION_STRING"
dotnet user-secrets set "Jwt:Chave" "YOUR_JWT_SECRET_KEY"
dotnet user-secrets set "ConfiguracoesEmail:NomeUsuario" "YOUR_EMAIL"
dotnet user-secrets set "ConfiguracoesEmail:Senha" "YOUR_EMAIL_APP_PASSWORD"
```

Para variaveis de ambiente, pode usar o formato hierarquico do ASP.NET Core:

```powershell
$env:SUPABASE_DB_CONNECTION="YOUR_DATABASE_CONNECTION_STRING"
$env:Jwt__Chave="YOUR_JWT_SECRET_KEY"
$env:ConfiguracoesEmail__NomeUsuario="YOUR_EMAIL"
$env:ConfiguracoesEmail__Senha="YOUR_EMAIL_APP_PASSWORD"
```

## Correr localmente

Requisitos:

- .NET SDK compativel com `net10.0`
- Base de dados PostgreSQL/Supabase configurada
- User Secrets ou variaveis de ambiente com as credenciais necessarias

Comandos:

```powershell
cd TaskGX
dotnet restore
dotnet build
dotnet run
```

Perfis locais configurados:

- HTTP: `http://localhost:5192`
- HTTPS: `https://localhost:7284`

Em desenvolvimento, o Swagger fica disponivel em:

- `https://localhost:7284/swagger`
- `https://localhost:7284/swagger/v1/swagger-corrigido.json`

O frontend React em desenvolvimento pode consumir a API a partir de:

- `http://localhost:5173`

## Base de dados

Os scripts para PostgreSQL/Supabase ficam em `TaskGX/database/supabase`:

- `schema.sql`
- `seed_prioridades.sql`
- `sync_sequences.sql`

Para uma base nova, execute primeiro o `schema.sql` e depois o
`seed_prioridades.sql`.

## Autenticacao

As rotas privadas exigem o header:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

Fluxo recomendado:

1. Criar conta em `POST /api/cadastro`.
2. Confirmar o email em `POST /api/verificacao/verificar-email`.
3. Fazer login em `POST /api/autenticacao/login`.
4. Usar o `token` retornado nas rotas protegidas.

## Endpoints principais

| Metodo | URL | JWT | Descricao |
| --- | --- | --- | --- |
| GET | `/` | Nao | Estado basico da API. |
| POST | `/api/cadastro` | Nao | Regista um novo utilizador. |
| POST | `/api/autenticacao/login` | Nao | Autentica com email e senha. |
| POST | `/api/autenticacao/google-login` | Nao | Autentica com token do Google. |
| POST | `/api/verificacao/verificar-email` | Nao | Verifica o email com codigo de 6 digitos. |
| POST | `/api/verificacao/reenviar-codigo` | Nao | Reenvia o codigo de verificacao. |
| GET | `/api/Usuarios/eu` | Sim | Obtem o utilizador autenticado. |
| PUT | `/api/Usuarios/eu` | Sim | Atualiza o perfil do utilizador autenticado. |
| PATCH | `/api/Usuarios/eu/senha` | Sim | Altera a senha do utilizador autenticado. |
| POST | `/api/Usuarios/eu/email/solicitar-alteracao` | Sim | Solicita alteracao de email. |
| POST | `/api/Usuarios/eu/email/confirmar-alteracao` | Sim | Confirma alteracao de email. |
| GET | `/api/Listas` | Sim | Lista as listas do utilizador. |
| POST | `/api/Listas` | Sim | Cria uma lista. |
| PUT | `/api/Listas/{id}` | Sim | Atualiza uma lista. |
| DELETE | `/api/Listas/{id}` | Sim | Remove uma lista. |
| GET | `/api/Tarefas?listaId={listaId}` | Sim | Lista tarefas de uma lista. |
| POST | `/api/Tarefas` | Sim | Cria uma tarefa. |
| PUT | `/api/Tarefas/{id}` | Sim | Atualiza uma tarefa. |
| DELETE | `/api/Tarefas/{id}` | Sim | Remove uma tarefa. |
| POST | `/api/Tarefas/{id}/concluir` | Sim | Marca uma tarefa como concluida. |
| GET | `/api/Prioridades` | Sim | Lista as prioridades disponiveis. |

Documentacao mais detalhada dos endpoints: `docs/API.md`.

Arquitetura e integracao com frontend/desktop: `docs/ARCHITECTURE.md`.

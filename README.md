@ -0,0 +1,247 @@
# TaskGX API

API REST do TaskGX para cadastro, autenticacao, verificacao de email e gestao de listas, tarefas e prioridades.

## Tecnologias

- ASP.NET Core com .NET 10
- Entity Framework Core
- PostgreSQL/Supabase via Npgsql
- JWT Bearer para rotas autenticadas
- Swagger em ambiente de desenvolvimento

## Configuracao

Configure a conexao com o PostgreSQL/Supabase por variavel de ambiente ou user-secrets. A API aceita `SUPABASE_DB_CONNECTION` ou `ConnectionStrings:DefaultConnection`.

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=YOUR_SUPABASE_HOST;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

Tambem configure as secoes:

- `Jwt:Chave`, `Jwt:Emissor`, `Jwt:Audiencia`, `Jwt:MinutosExpiracao`
- `GoogleAuth:ClientId`
- `ConfiguracoesEmail:Host`, `ConfiguracoesEmail:Porta`, `ConfiguracoesEmail:NomeUsuario`, `ConfiguracoesEmail:Senha`, `ConfiguracoesEmail:EmailRemetente`, `ConfiguracoesEmail:NomeRemetente`, `ConfiguracoesEmail:HabilitarSsl`

Os scripts de banco ficam em `database/supabase`. Para uma base nova, execute `schema.sql` e depois `seed_prioridades.sql`.

## Executar localmente

```powershell
dotnet restore
dotnet run
```

Perfis locais configurados:

- HTTP: `http://localhost:5192`
- HTTPS: `https://localhost:7284`

Em desenvolvimento, a documentacao interativa fica em `/swagger` e o JSON corrigido em `/swagger/v1/swagger-corrigido.json`.

## Autenticacao

As rotas marcadas como protegidas exigem o header:

```http
Authorization: Bearer SEU_TOKEN_JWT
```

Fluxo sugerido:

1. Criar conta em `POST /api/cadastro`.
2. Confirmar o email em `POST /api/verificacao/verificar-email`.
3. Fazer login em `POST /api/autenticacao/login`.
4. Usar o `token` retornado nas demais rotas protegidas.

## Endpoints

### Status

| Metodo | Rota | Auth | Descricao |
| --- | --- | --- | --- |
| GET | `/` | Nao | Retorna nome, status e ambiente da API. |

### Cadastro, login e verificacao

| Metodo | Rota | Auth | Descricao |
| --- | --- | --- | --- |
| POST | `/api/cadastro` | Nao | Cria uma conta e envia codigo de verificacao. |
| POST | `/api/autenticacao/login` | Nao | Autentica por email e senha. |
| POST | `/api/autenticacao/google-login` | Nao | Autentica usando token do Google. |
| POST | `/api/verificacao/verificar-email` | Nao | Confirma o email com codigo de 6 digitos. |
| POST | `/api/verificacao/reenviar-codigo` | Nao | Reenvia o codigo de verificacao. |

Payloads principais:

Cadastro:

```json
{
  "nome": "Usuario TaskGX",
  "email": "usuario@taskgx.com",
  "senha": "Senha123!",
  "confirmarSenha": "Senha123!"
}
```

Login:

```json
{
  "email": "usuario@taskgx.com",
  "senha": "Senha123!"
}
```

Login com Google:

```json
{
  "idToken": "GOOGLE_ID_TOKEN"
}
```

Verificacao de email:

```json
{
  "email": "usuario@taskgx.com",
  "codigo": "123456"
}
```

### Usuario

Todas as rotas abaixo sao protegidas.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/Usuarios/eu` | Retorna o perfil do usuario autenticado. |
| PUT | `/api/Usuarios/eu` | Atualiza nome e avatar do usuario autenticado. |
| PATCH | `/api/Usuarios/eu/senha` | Altera a senha do usuario autenticado. |
| POST | `/api/Usuarios/eu/email/solicitar-alteracao` | Solicita alteracao de email e envia codigo. |
| POST | `/api/Usuarios/eu/email/confirmar-alteracao` | Confirma a alteracao de email com codigo de 6 digitos. |

Payloads principais:

Atualizar perfil:

```json
{
  "nome": "Novo Nome",
  "avatar": "https://exemplo.com/avatar.png"
}
```

Alterar senha:

```json
{
  "senhaAtual": "Senha123!",
  "novaSenha": "NovaSenha123!",
  "confirmarNovaSenha": "NovaSenha123!"
}
```

Solicitar alteracao de email:

```json
{
  "novoEmail": "novo@taskgx.com"
}
```

Confirmar alteracao de email:

```json
{
  "codigo": "123456"
}
```

### Listas

Todas as rotas abaixo sao protegidas.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/Listas` | Lista as listas do usuario autenticado. |
| POST | `/api/Listas` | Cria uma lista para o usuario autenticado. |
| PUT | `/api/Listas/{id}` | Atualiza uma lista do usuario autenticado. |
| DELETE | `/api/Listas/{id}` | Remove uma lista do usuario autenticado. |

Payload para criacao/atualizacao:

```json
{
  "nome": "Trabalho",
  "cor": "#2f80ed",
  "favorita": true
}
```

### Tarefas

Todas as rotas abaixo sao protegidas.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/Tarefas?listaId={listaId}` | Lista tarefas de uma lista do usuario autenticado. |
| POST | `/api/Tarefas` | Cria uma tarefa em uma lista do usuario autenticado. |
| PUT | `/api/Tarefas/{id}` | Atualiza uma tarefa do usuario autenticado. |
| DELETE | `/api/Tarefas/{id}` | Remove uma tarefa do usuario autenticado. |
| POST | `/api/Tarefas/{id}/concluir` | Marca uma tarefa como concluida. |

Payload de criacao:

```json
{
  "listaId": 1,
  "titulo": "Revisar README",
  "descricao": "Conferir endpoints da API",
  "tags": "docs,api",
  "prioridadeId": 2,
  "concluida": false,
  "arquivada": false,
  "dataVencimento": "2026-06-30T12:00:00Z",
  "ordem": 1
}
```

Payload de atualizacao:

```json
{
  "id": 10,
  "listaId": 1,
  "titulo": "Revisar README",
  "descricao": "Conferir endpoints da API",
  "tags": "docs,api",
  "prioridadeId": 2,
  "concluida": true,
  "arquivada": false,
  "dataVencimento": "2026-06-30T12:00:00Z",
  "ordem": 1
}
```

### Prioridades

Todas as rotas abaixo sao protegidas.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/Prioridades` | Lista prioridades cadastradas. |
| POST | `/api/Prioridades` | Endpoint reservado; retorna `403 Forbidden`. |
| PUT | `/api/Prioridades/{id}` | Endpoint reservado; retorna `403 Forbidden`. |
| DELETE | `/api/Prioridades/{id}` | Endpoint reservado; retorna `403 Forbidden`. |

## Respostas e erros

- Login retorna `token` e dados basicos do usuario.
- Criacao de lista/tarefa retorna `201 Created` com o DTO criado.
- Atualizacoes e exclusoes retornam `204 No Content` quando bem-sucedidas.
- Erros de validacao usam `ValidationProblemDetails`.
- Erros de regra/autenticacao usam `ProblemDetails`.
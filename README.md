# TaskGX API

A **TaskGX API** é a API REST central do projeto TaskGX. Ela fornece os recursos necessários para autenticar utilizadores, criar contas, verificar endereços de e-mail e gerir listas, tarefas e prioridades.

A API foi preparada para ser consumida pela aplicação web desenvolvida em React e por futuras aplicações desktop ou mobile que reutilizem os mesmos dados e regras de autenticação.

## Tecnologias

* ASP.NET Core Web API com .NET 10
* Entity Framework Core
* PostgreSQL/Supabase
* Provider Npgsql para Entity Framework Core
* Autenticação através de JWT
* Autenticação com Google
* Swagger/OpenAPI em ambiente de desenvolvimento
* Envio de e-mails através de SMTP

## Funcionalidades principais

* Registo de utilizadores
* Autenticação com e-mail e palavra-passe através de JWT
* Autenticação com conta Google
* Verificação de e-mail através de código
* Reenvio do código de verificação
* Consulta e atualização do utilizador autenticado
* Alteração da palavra-passe
* Solicitação e confirmação da alteração de e-mail
* Eliminação da conta e dos dados associados
* Gestão das listas do utilizador
* Gestão de tarefas por lista
* Conclusão de tarefas
* Consulta de prioridades
* Documentação interativa através do Swagger

## Configuração segura

Credenciais e valores sensíveis não devem ser armazenados diretamente no ficheiro `appsettings.json` versionado.

Em desenvolvimento, devem ser utilizados:

* .NET User Secrets;
* variáveis de ambiente;
* ficheiros locais não incluídos no repositório.

O projeto contém um ficheiro de referência em:

```text
TaskGX/appsettings.example.json
```

Esse ficheiro contém apenas placeholders. Os valores reais devem ser configurados fora do repositório.

### Valores principais

* `ConnectionStrings:DefaultConnection`
* `SUPABASE_DB_CONNECTION`
* `Jwt:Chave`
* `Jwt:Emissor`
* `Jwt:Audiencia`
* `Jwt:MinutosExpiracao`
* `GoogleAuth:ClientId`
* `ConfiguracoesEmail:Host`
* `ConfiguracoesEmail:Porta`
* `ConfiguracoesEmail:NomeUsuario`
* `ConfiguracoesEmail:Senha`
* `ConfiguracoesEmail:EmailRemetente`
* `ConfiguracoesEmail:NomeRemetente`
* `ConfiguracoesEmail:HabilitarSsl`
* `Cors:AllowedOrigins`

### Exemplo com User Secrets

```powershell
cd TaskGX

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_DATABASE_CONNECTION_STRING"
dotnet user-secrets set "Jwt:Chave" "YOUR_JWT_SECRET_KEY"
dotnet user-secrets set "GoogleAuth:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "ConfiguracoesEmail:NomeUsuario" "YOUR_EMAIL"
dotnet user-secrets set "ConfiguracoesEmail:Senha" "YOUR_EMAIL_APP_PASSWORD"
```

### Exemplo com variáveis de ambiente

No PowerShell:

```powershell
$env:SUPABASE_DB_CONNECTION="YOUR_DATABASE_CONNECTION_STRING"
$env:Jwt__Chave="YOUR_JWT_SECRET_KEY"
$env:GoogleAuth__ClientId="YOUR_GOOGLE_CLIENT_ID"
$env:ConfiguracoesEmail__NomeUsuario="YOUR_EMAIL"
$env:ConfiguracoesEmail__Senha="YOUR_EMAIL_APP_PASSWORD"
```

O ASP.NET Core utiliza dois caracteres de underscore, `__`, para representar níveis hierárquicos nas variáveis de ambiente.

## Executar localmente

### Requisitos

* .NET SDK compatível com `net10.0`
* Base de dados PostgreSQL ou Supabase configurada
* User Secrets ou variáveis de ambiente com as credenciais necessárias

### Comandos

```powershell
cd TaskGX
dotnet restore
dotnet build
dotnet run
```

## Endereços locais

Os perfis locais estão configurados nos seguintes endereços:

* HTTP: `http://localhost:5192`
* HTTPS: `https://localhost:7284`

As portas podem ser consultadas ou alteradas no ficheiro:

```text
Properties/launchSettings.json
```

Em ambiente de desenvolvimento, o Swagger fica disponível em:

* `https://localhost:7284/swagger`
* `https://localhost:7284/swagger/v1/swagger-corrigido.json`

O frontend React em desenvolvimento pode consumir a API a partir de:

```text
http://localhost:5173
```

Esse endereço deve estar incluído nas origens permitidas pela configuração de CORS.

## Base de dados

Os scripts utilizados para configurar a base de dados PostgreSQL/Supabase encontram-se em:

```text
TaskGX/database/supabase
```

Ficheiros disponíveis:

* `schema.sql`
* `seed_prioridades.sql`
* `sync_sequences.sql`

Para configurar uma base de dados nova:

1. Execute `schema.sql`.
2. Execute `seed_prioridades.sql`.
3. Execute `sync_sequences.sql` caso seja necessário sincronizar as sequências de IDs.

## Autenticação

As rotas privadas exigem um token JWT enviado no header:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### Fluxo com e-mail e palavra-passe

1. Criar uma conta através de `POST /api/cadastro`.
2. Confirmar o e-mail através de `POST /api/verificacao/verificar-email`.
3. Fazer login através de `POST /api/autenticacao/login`.
4. Utilizar o token retornado nas rotas protegidas.

### Fluxo com Google

1. Obter um Google ID Token no frontend.
2. Enviar o token para `POST /api/autenticacao/google-login`.
3. Receber o token JWT do TaskGX.
4. Utilizar o token JWT do TaskGX nas rotas protegidas.

## Endpoints principais

| Método | URL                                          | JWT | Descrição                                                        |
| ------ | -------------------------------------------- | --- | ---------------------------------------------------------------- |
| GET    | `/`                                          | Não | Retorna o estado básico da API.                                  |
| POST   | `/api/cadastro`                              | Não | Regista um novo utilizador.                                      |
| POST   | `/api/autenticacao/login`                    | Não | Autentica com e-mail e palavra-passe.                            |
| POST   | `/api/autenticacao/google-login`             | Não | Autentica através de um Google ID Token.                         |
| POST   | `/api/verificacao/verificar-email`           | Não | Verifica o e-mail através de um código de seis dígitos.          |
| POST   | `/api/verificacao/reenviar-codigo`           | Não | Reenvia o código de verificação.                                 |
| GET    | `/api/Usuarios/eu`                           | Sim | Obtém o utilizador autenticado.                                  |
| PUT    | `/api/Usuarios/eu`                           | Sim | Atualiza o perfil do utilizador autenticado.                     |
| DELETE | `/api/Usuarios/eu`                           | Sim | Elimina a conta e os dados associados ao utilizador autenticado. |
| PATCH  | `/api/Usuarios/eu/senha`                     | Sim | Altera a palavra-passe do utilizador autenticado.                |
| POST   | `/api/Usuarios/eu/email/solicitar-alteracao` | Sim | Solicita a alteração do endereço de e-mail.                      |
| POST   | `/api/Usuarios/eu/email/confirmar-alteracao` | Sim | Confirma a alteração do endereço de e-mail.                      |
| GET    | `/api/Listas`                                | Sim | Obtém as listas do utilizador autenticado.                       |
| POST   | `/api/Listas`                                | Sim | Cria uma nova lista.                                             |
| PUT    | `/api/Listas/{id}`                           | Sim | Atualiza uma lista pertencente ao utilizador.                    |
| DELETE | `/api/Listas/{id}`                           | Sim | Elimina uma lista pertencente ao utilizador.                     |
| GET    | `/api/Tarefas?listaId={listaId}`             | Sim | Obtém as tarefas de uma lista.                                   |
| POST   | `/api/Tarefas`                               | Sim | Cria uma nova tarefa.                                            |
| PUT    | `/api/Tarefas/{id}`                          | Sim | Atualiza uma tarefa pertencente ao utilizador.                   |
| DELETE | `/api/Tarefas/{id}`                          | Sim | Elimina uma tarefa pertencente ao utilizador.                    |
| POST   | `/api/Tarefas/{id}/concluir`                 | Sim | Marca uma tarefa como concluída.                                 |
| GET    | `/api/Prioridades`                           | Sim | Obtém as prioridades disponíveis.                                |

## Documentação adicional

A documentação detalhada dos endpoints encontra-se em:

```text
docs/API.md
```

A documentação sobre a arquitetura e integração com frontend e aplicações desktop encontra-se em:

```text
docs/ARCHITECTURE.md
```

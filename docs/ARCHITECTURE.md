# TaskGX API — Arquitetura

O TaskGX foi organizado em torno de uma API central responsável pela autenticação, utilizadores, listas, tarefas e prioridades.

As aplicações cliente consomem a mesma API e não comunicam diretamente com a base de dados. Dessa forma, as regras de negócio, autenticação e acesso aos dados permanecem centralizadas.

## Visão geral

```text
┌───────────────────┐
│   React Web App   │
└─────────┬─────────┘
          │ HTTP/HTTPS
          ▼
┌──────────────────────────────┐
│ TaskGX ASP.NET Core Web API  │
└─────────────┬────────────────┘
              │ Entity Framework Core / Npgsql
              ▼
┌──────────────────────────────┐
│    PostgreSQL / Supabase     │
└──────────────────────────────┘


┌───────────────────┐
│    Desktop App    │
└─────────┬─────────┘
          │ HTTP/HTTPS
          ▼
┌──────────────────────────────┐
│ TaskGX ASP.NET Core Web API  │
└─────────────┬────────────────┘
              │ Entity Framework Core / Npgsql
              ▼
┌──────────────────────────────┐
│    PostgreSQL / Supabase     │
└──────────────────────────────┘
```

## Princípio da arquitetura

A arquitetura segue um modelo cliente-servidor.

As aplicações cliente são responsáveis pela interface e pela interação com o utilizador. A API concentra:

* autenticação;
* autorização;
* validação;
* regras de negócio;
* acesso à base de dados;
* tratamento de erros;
* exposição dos endpoints.

A base de dados é acedida exclusivamente pela API.

## Componentes

### React Web App

A aplicação web em React consome a TaskGX API através de pedidos HTTP ou HTTPS.

As principais responsabilidades do frontend são:

* apresentar a interface;
* recolher dados introduzidos pelo utilizador;
* enviar pedidos para a API;
* guardar e utilizar o token JWT;
* apresentar mensagens de sucesso ou erro;
* atualizar o estado da aplicação após as respostas da API.

Em desenvolvimento local, o frontend utiliza normalmente:

```text
http://localhost:5173
```

Esse endereço deve estar incluído nas origens permitidas pela configuração de CORS da API.

O frontend não deve:

* comunicar diretamente com a base de dados;
* gerar tokens JWT;
* confiar apenas em validações locais;
* enviar IDs de utilizadores quando a API consegue identificá-los através do token.

### Desktop App

A aplicação desktop poderá consumir a mesma API utilizada pelo frontend React.

Isso permite reutilizar:

* autenticação;
* utilizadores;
* listas;
* tarefas;
* prioridades;
* regras de negócio;
* validações;
* estrutura de dados.

A aplicação desktop deverá enviar os pedidos através de HTTP ou HTTPS e utilizar o mesmo formato de autenticação JWT.

Essa abordagem evita:

* duplicação de regras;
* acesso direto à base de dados;
* inconsistências entre aplicações;
* armazenamento separado dos mesmos dados.

### TaskGX ASP.NET Core Web API

A TaskGX API é o componente central do sistema.

As suas principais responsabilidades incluem:

* registo de utilizadores;
* autenticação com e-mail e palavra-passe;
* autenticação com Google;
* emissão de tokens JWT;
* verificação de e-mail;
* reenvio de códigos de verificação;
* consulta e atualização do utilizador autenticado;
* alteração da palavra-passe;
* alteração do endereço de e-mail;
* eliminação da conta;
* gestão de listas;
* gestão de tarefas;
* consulta de prioridades;
* validação dos dados recebidos;
* controlo de acesso aos recursos;
* comunicação com a base de dados;
* tratamento consistente de erros;
* disponibilização do Swagger em desenvolvimento.

A API garante que cada utilizador apenas consegue consultar ou alterar os seus próprios dados.

### PostgreSQL / Supabase

A base de dados PostgreSQL, disponibilizada através do Supabase, armazena os dados persistentes da aplicação.

Entre os dados armazenados encontram-se:

* utilizadores;
* listas;
* tarefas;
* prioridades;
* códigos de verificação;
* estados de verificação de e-mail;
* dados relacionados com alterações de conta.

O acesso à base de dados é realizado pela API através do Entity Framework Core e do provider Npgsql.

As aplicações cliente não possuem credenciais nem acesso direto à base de dados.

## Organização interna da API

A API está dividida em diferentes responsabilidades.

A estrutura exata pode variar de acordo com a evolução do projeto, mas segue normalmente a seguinte organização:

```text
TaskGX/
├── Controllers/
├── DTOs/
├── Models/
├── Repositories/
├── Services/
├── Data/
├── database/
│   └── supabase/
├── Properties/
├── appsettings.example.json
└── Program.cs
```

### Controllers

Os controllers recebem os pedidos HTTP e devolvem as respostas ao cliente.

São responsáveis por:

* interpretar rotas;
* receber parâmetros;
* validar autenticação;
* chamar services ou repositories;
* devolver códigos HTTP apropriados.

Os controllers devem conter o mínimo possível de lógica de negócio.

### DTOs

Os DTOs definem os formatos de entrada e saída da API.

São utilizados para:

* receber dados do cliente;
* validar campos;
* impedir a exposição direta das entidades;
* controlar quais propriedades são devolvidas;
* evitar o envio de dados sensíveis.

As entidades da base de dados não devem ser devolvidas diretamente quando contêm informações internas ou sensíveis.

### Services

Os services concentram regras de negócio e operações que não pertencem diretamente aos controllers.

Podem incluir:

* autenticação;
* geração de JWT;
* validação de tokens Google;
* envio de e-mails;
* geração de códigos de verificação;
* operações relacionadas com utilizadores.

### Repositories

Os repositories concentram operações de acesso aos dados quando essa abstração é utilizada pelo projeto.

São responsáveis por:

* consultas;
* inserções;
* atualizações;
* eliminações;
* acesso ao contexto do Entity Framework Core.

### Data

A camada de dados contém o contexto do Entity Framework Core e as configurações relacionadas com a base de dados.

O contexto é responsável por:

* mapear entidades;
* configurar relações;
* configurar chaves;
* definir comportamentos de eliminação;
* executar operações no PostgreSQL.

## Fluxo de um pedido

Um pedido típico segue este fluxo:

```text
Utilizador
    │
    ▼
Aplicação cliente
    │
    ▼
Pedido HTTP/HTTPS
    │
    ▼
Controller
    │
    ▼
Service ou Repository
    │
    ▼
Entity Framework Core
    │
    ▼
PostgreSQL / Supabase
    │
    ▼
Resposta da API
    │
    ▼
Aplicação cliente
    │
    ▼
Utilizador
```

Exemplo de criação de uma tarefa:

1. O utilizador preenche o formulário no frontend.
2. O frontend envia um pedido `POST /api/Tarefas`.
3. O token JWT é enviado no header.
4. A API valida o token.
5. A API identifica o utilizador autenticado.
6. A API valida se a lista pertence ao utilizador.
7. A tarefa é guardada na base de dados.
8. A API devolve `201 Created`.
9. O frontend atualiza a interface.

## Autenticação

A API utiliza autenticação através de JWT.

Depois de um login bem-sucedido, o cliente recebe um token JWT.

Esse token deve ser enviado nos endpoints protegidos:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

A API valida:

* assinatura do token;
* emissor;
* audiência;
* data de expiração;
* claims do utilizador.

O ID do utilizador é obtido a partir das claims do token.

O cliente não deve enviar o ID do utilizador para operações relacionadas com a própria conta quando a API disponibiliza rotas como:

```http
GET /api/Usuarios/eu
PUT /api/Usuarios/eu
DELETE /api/Usuarios/eu
```

## Autenticação com Google

No login com Google, o frontend obtém um Google ID Token através do Google Identity Services.

O fluxo é:

```text
Google Identity Services
          │
          ▼
Google ID Token
          │
          ▼
React Web App
          │
          ▼
POST /api/autenticacao/google-login
          │
          ▼
Validação do Google ID Token
          │
          ▼
JWT do TaskGX
```

O Google ID Token é utilizado apenas para confirmar a identidade junto da API.

Depois da autenticação, o cliente deve usar o JWT emitido pelo TaskGX nos restantes endpoints.

## Autorização e isolamento dos dados

Os endpoints privados exigem JWT.

Endpoints públicos incluem normalmente:

* cadastro;
* login;
* login com Google;
* verificação de e-mail;
* reenvio do código de verificação.

Endpoints privados incluem:

* utilizador autenticado;
* listas;
* tarefas;
* prioridades.

A API deve sempre verificar se o recurso solicitado pertence ao utilizador autenticado.

Por exemplo:

* uma lista só pode ser alterada pelo seu proprietário;
* uma tarefa só pode ser consultada ou alterada pelo proprietário da lista;
* a eliminação de conta utiliza o utilizador identificado pelo JWT;
* o cliente não pode utilizar um ID para aceder aos dados de outro utilizador.

## CORS

O CORS controla quais origens podem efetuar pedidos à API através do navegador.

Em desenvolvimento, a origem normalmente autorizada é:

```text
http://localhost:5173
```

Outras origens devem ser adicionadas através da configuração:

```text
Cors:AllowedOrigins
```

A API não deve utilizar uma política aberta em produção sem necessidade.

## Tratamento de erros

A API utiliza respostas HTTP para indicar o resultado dos pedidos.

Os principais códigos utilizados são:

| Código                      | Utilização                                        |
| --------------------------- | ------------------------------------------------- |
| `200 OK`                    | Pedido concluído com sucesso.                     |
| `201 Created`               | Recurso criado com sucesso.                       |
| `204 No Content`            | Operação concluída sem body na resposta.          |
| `400 Bad Request`           | Dados inválidos ou regra de negócio não cumprida. |
| `401 Unauthorized`          | Token ausente, inválido ou expirado.              |
| `403 Forbidden`             | Operação não permitida.                           |
| `404 Not Found`             | Recurso não encontrado.                           |
| `409 Conflict`              | Conflito com dados existentes.                    |
| `500 Internal Server Error` | Erro inesperado.                                  |

Os erros devem utilizar um formato consistente, como `ProblemDetails` ou `ValidationProblemDetails`.

A API não deve devolver:

* stack traces;
* connection strings;
* secrets;
* hashes de palavras-passe;
* detalhes internos da base de dados.

## Configuração

As credenciais sensíveis devem ser configuradas através de:

* .NET User Secrets;
* variáveis de ambiente;
* serviços seguros de gestão de secrets em produção.

O ficheiro:

```text
TaskGX/appsettings.example.json
```

serve apenas como referência da estrutura de configuração e não contém valores reais.

Entre as principais configurações encontram-se:

* conexão com PostgreSQL;
* chave JWT;
* emissor e audiência do JWT;
* tempo de expiração;
* Google Client ID;
* credenciais SMTP;
* origens permitidas pelo CORS.

## Base de dados

Os scripts da base de dados encontram-se em:

```text
TaskGX/database/supabase
```

Ficheiros principais:

* `schema.sql`
* `seed_prioridades.sql`
* `sync_sequences.sql`

Esses ficheiros são utilizados para criar e preparar a estrutura necessária no PostgreSQL/Supabase.

## Segurança

A arquitetura segue os seguintes princípios:

* clientes não comunicam diretamente com a base de dados;
* credenciais não ficam expostas no frontend;
* tokens JWT não devem ser escritos em logs;
* palavras-passe são armazenadas através de hash;
* dados sensíveis não são devolvidos nos DTOs;
* HTTPS deve ser utilizado em produção;
* cada recurso é validado em relação ao utilizador autenticado;
* secrets não devem ser versionados;
* erros internos não devem ser expostos ao cliente.

## Documentação relacionada

* [README principal](../README.md)
* [Documentação dos endpoints](API.md)

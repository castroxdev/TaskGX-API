# TaskGX API - Arquitetura

TaskGX foi organizada para ter uma API central que concentra autenticacao,
utilizadores, listas, tarefas e prioridades. As aplicacoes cliente consomem a
mesma API e nao comunicam diretamente com a base de dados.

## Visao geral

```text
React Web App
      |
      v
TaskGX ASP.NET Core Web API
      |
      v
PostgreSQL / Supabase

Desktop App
      |
      v
TaskGX ASP.NET Core Web API
      |
      v
PostgreSQL / Supabase
```

## Componentes

### React Web App

A aplicacao web em React consome a TaskGX API por HTTP/HTTPS. Em
desenvolvimento local, o CORS da API permite chamadas a partir de
`http://localhost:5173`, que e o porto comum do Vite.

### Desktop App

A aplicacao desktop podera consumir a mesma API, reutilizando autenticacao,
utilizadores, listas, tarefas e prioridades. Isto evita duplicar regras de
acesso e mantem os dados centralizados.

### TaskGX ASP.NET Core Web API

A API e responsavel por:

- registo/cadastro de utilizadores;
- login e emissao de tokens JWT;
- verificacao de email;
- gestao do utilizador autenticado;
- gestao de listas;
- gestao de tarefas;
- consulta de prioridades;
- exposicao do Swagger em desenvolvimento.

### PostgreSQL / Supabase

O PostgreSQL/Supabase armazena os dados persistentes da aplicacao, incluindo
utilizadores, listas, tarefas, prioridades e dados de verificacao.

## Autenticacao e autorizacao

A API usa JWT Authentication. Depois do login, o cliente recebe um token JWT e
deve envia-lo nas rotas privadas:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

Endpoints publicos, como cadastro, login, verificacao de email e reenvio de
codigo, nao exigem JWT. Endpoints de utilizadores, listas, tarefas e
prioridades exigem JWT.

## Configuracao

Credenciais sensiveis devem ser configuradas por User Secrets ou variaveis de
ambiente. O ficheiro `TaskGX/appsettings.example.json` serve apenas como
referencia de estrutura e nao contem valores reais.

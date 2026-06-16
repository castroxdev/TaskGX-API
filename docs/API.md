# TaskGX API - Endpoints

Esta documentacao usa as rotas reais declaradas nos controllers do projeto.
Os exemplos usam JSON em camelCase, que e o formato esperado por clientes web
modernos e e aceite pelo model binding do ASP.NET Core.

Base local comum:

- `https://localhost:7284`
- `http://localhost:5192`

Rotas com JWT exigem:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

## Resumo

| Metodo | URL | JWT | Descricao |
| --- | --- | --- | --- |
| POST | `/api/cadastro` | Nao | Regista um novo utilizador. |
| POST | `/api/autenticacao/login` | Nao | Autentica com email e senha. |
| POST | `/api/autenticacao/google-login` | Nao | Autentica com token do Google. |
| POST | `/api/verificacao/verificar-email` | Nao | Confirma email com codigo de 6 digitos. |
| POST | `/api/verificacao/reenviar-codigo` | Nao | Reenvia codigo de verificacao. |
| GET | `/api/Usuarios/eu` | Sim | Obtem o utilizador autenticado. |
| PUT | `/api/Usuarios/eu` | Sim | Atualiza o perfil autenticado. |
| PATCH | `/api/Usuarios/eu/senha` | Sim | Altera a senha do utilizador autenticado. |
| POST | `/api/Usuarios/eu/email/solicitar-alteracao` | Sim | Solicita alteracao de email. |
| POST | `/api/Usuarios/eu/email/confirmar-alteracao` | Sim | Confirma alteracao de email. |
| GET | `/api/Listas` | Sim | Lista listas do utilizador. |
| POST | `/api/Listas` | Sim | Cria lista. |
| PUT | `/api/Listas/{id}` | Sim | Atualiza lista. |
| DELETE | `/api/Listas/{id}` | Sim | Remove lista. |
| GET | `/api/Tarefas?listaId={listaId}` | Sim | Lista tarefas de uma lista. |
| POST | `/api/Tarefas` | Sim | Cria tarefa. |
| PUT | `/api/Tarefas/{id}` | Sim | Atualiza tarefa. |
| DELETE | `/api/Tarefas/{id}` | Sim | Remove tarefa. |
| POST | `/api/Tarefas/{id}/concluir` | Sim | Marca tarefa como concluida. |
| GET | `/api/Prioridades` | Sim | Lista prioridades. |
| POST | `/api/Prioridades` | Sim | Reservado; retorna `403 Forbidden`. |
| PUT | `/api/Prioridades/{id}` | Sim | Reservado; retorna `403 Forbidden`. |
| DELETE | `/api/Prioridades/{id}` | Sim | Reservado; retorna `403 Forbidden`. |

## Cadastro

### POST `/api/cadastro`

Regista um utilizador e envia o codigo de verificacao de email.

JWT: nao.

Request:

```json
{
  "nome": "Utilizador TaskGX",
  "email": "utilizador@example.com",
  "senha": "Senha123!",
  "confirmarSenha": "Senha123!"
}
```

Response `200 OK`:

```json
{
  "mensagem": "Conta criada com sucesso. Verifique o seu email."
}
```

Response de erro comum `400 Bad Request`:

```json
{
  "title": "Nao foi possivel concluir o cadastro.",
  "detail": "Mensagem de erro do cadastro.",
  "status": 400
}
```

## Autenticacao

### POST `/api/autenticacao/login`

Autentica um utilizador com email e senha.

JWT: nao.

Request:

```json
{
  "email": "utilizador@example.com",
  "senha": "Senha123!"
}
```

Response `200 OK`:

```json
{
  "token": "JWT_TOKEN",
  "usuario": {
    "id": 1,
    "nome": "Utilizador TaskGX",
    "email": "utilizador@example.com"
  }
}
```

Response de erro comum `401 Unauthorized`:

```json
{
  "title": "Falha na autenticacao.",
  "detail": "Credenciais invalidas ou usuario nao autorizado.",
  "status": 401
}
```

### POST `/api/autenticacao/google-login`

Autentica um utilizador com token de identidade do Google.

JWT: nao.

Request:

```json
{
  "idToken": "GOOGLE_ID_TOKEN"
}
```

Response `200 OK`:

```json
{
  "token": "JWT_TOKEN",
  "usuario": {
    "id": 1,
    "nome": "Utilizador TaskGX",
    "email": "utilizador@example.com",
    "avatar": "https://example.com/avatar.png"
  }
}
```

## Verificacao de email

### POST `/api/verificacao/verificar-email`

Confirma o email com um codigo de 6 digitos.

JWT: nao.

Request:

```json
{
  "email": "utilizador@example.com",
  "codigo": "123456"
}
```

Response `200 OK`:

```json
{
  "mensagem": "Email verificado com sucesso."
}
```

### POST `/api/verificacao/reenviar-codigo`

Reenvia o codigo de verificacao para o email indicado.

JWT: nao.

Request:

```json
{
  "email": "utilizador@example.com"
}
```

Response `200 OK`:

```json
{
  "mensagem": "Codigo de verificacao reenviado com sucesso."
}
```

## Utilizador autenticado

### GET `/api/Usuarios/eu`

Obtem o perfil do utilizador autenticado.

JWT: sim.

Request: sem body.

Response `200 OK`:

```json
{
  "id": 1,
  "nome": "Utilizador TaskGX",
  "email": "utilizador@example.com",
  "avatar": null,
  "ativo": true,
  "emailVerificado": true,
  "criadoEm": "2026-06-16T10:00:00Z",
  "dataAtualizacao": "2026-06-16T10:00:00Z"
}
```

### PUT `/api/Usuarios/eu`

Atualiza nome e avatar do utilizador autenticado.

JWT: sim.

Request:

```json
{
  "nome": "Novo Nome",
  "avatar": "https://example.com/avatar.png"
}
```

Response `204 No Content`: sem body.

### PATCH `/api/Usuarios/eu/senha`

Altera a senha do utilizador autenticado.

JWT: sim.

Request:

```json
{
  "senhaAtual": "Senha123!",
  "novaSenha": "NovaSenha123!",
  "confirmarNovaSenha": "NovaSenha123!"
}
```

Response `204 No Content`: sem body.

### POST `/api/Usuarios/eu/email/solicitar-alteracao`

Solicita alteracao de email e envia codigo de confirmacao.

JWT: sim.

Request:

```json
{
  "novoEmail": "novo-email@example.com"
}
```

Response `200 OK`:

```json
{
  "mensagem": "Codigo de confirmacao enviado para o novo email."
}
```

### POST `/api/Usuarios/eu/email/confirmar-alteracao`

Confirma a alteracao de email com codigo de 6 digitos.

JWT: sim.

Request:

```json
{
  "codigo": "123456"
}
```

Response `200 OK`:

```json
{
  "mensagem": "Email alterado com sucesso."
}
```

## Listas

### GET `/api/Listas`

Lista as listas do utilizador autenticado.

JWT: sim.

Request: sem body.

Response `200 OK`:

```json
[
  {
    "id": 1,
    "nome": "Trabalho",
    "cor": "#2f80ed",
    "favorita": true
  }
]
```

### POST `/api/Listas`

Cria uma lista para o utilizador autenticado.

JWT: sim.

Request:

```json
{
  "nome": "Trabalho",
  "cor": "#2f80ed",
  "favorita": true
}
```

Response `201 Created`:

```json
{
  "id": 1,
  "nome": "Trabalho",
  "cor": "#2f80ed",
  "favorita": true
}
```

### PUT `/api/Listas/{id}`

Atualiza uma lista do utilizador autenticado.

JWT: sim.

Request:

```json
{
  "nome": "Trabalho atualizado",
  "cor": "#27ae60",
  "favorita": false
}
```

Response `204 No Content`: sem body.

### DELETE `/api/Listas/{id}`

Remove uma lista do utilizador autenticado.

JWT: sim.

Request: sem body.

Response `204 No Content`: sem body.

## Tarefas

### GET `/api/Tarefas?listaId={listaId}`

Lista tarefas de uma lista pertencente ao utilizador autenticado.

JWT: sim.

Request: sem body.

Response `200 OK`:

```json
[
  {
    "id": 10,
    "titulo": "Preparar README",
    "descricao": "Rever documentacao da API",
    "tags": "docs,api",
    "concluida": false,
    "arquivada": false,
    "dataVencimento": "2026-06-30T12:00:00Z",
    "dataCriacao": "2026-06-16T10:00:00Z",
    "listaId": 1,
    "listaNome": "Trabalho",
    "prioridadeId": 2,
    "prioridadeNome": "Media"
  }
]
```

### POST `/api/Tarefas`

Cria uma tarefa numa lista do utilizador autenticado.

JWT: sim.

Request:

```json
{
  "listaId": 1,
  "titulo": "Preparar README",
  "descricao": "Rever documentacao da API",
  "tags": "docs,api",
  "prioridadeId": 2,
  "concluida": false,
  "arquivada": false,
  "dataVencimento": "2026-06-30T12:00:00Z",
  "ordem": 1
}
```

Response `201 Created`:

```json
{
  "id": 10,
  "titulo": "Preparar README",
  "descricao": "Rever documentacao da API",
  "tags": "docs,api",
  "concluida": false,
  "arquivada": false,
  "dataVencimento": "2026-06-30T12:00:00Z",
  "dataCriacao": "2026-06-16T10:00:00Z",
  "listaId": 1,
  "listaNome": "Trabalho",
  "prioridadeId": 2,
  "prioridadeNome": "Media"
}
```

### PUT `/api/Tarefas/{id}`

Atualiza uma tarefa do utilizador autenticado. O `id` da rota deve ser igual ao
`id` enviado no body.

JWT: sim.

Request:

```json
{
  "id": 10,
  "listaId": 1,
  "titulo": "Preparar documentacao",
  "descricao": "Rever README e docs/API.md",
  "tags": "docs,api",
  "prioridadeId": 1,
  "concluida": true,
  "arquivada": false,
  "dataVencimento": "2026-06-30T12:00:00Z",
  "ordem": 1
}
```

Response `204 No Content`: sem body.

### DELETE `/api/Tarefas/{id}`

Remove uma tarefa do utilizador autenticado.

JWT: sim.

Request: sem body.

Response `204 No Content`: sem body.

### POST `/api/Tarefas/{id}/concluir`

Marca uma tarefa como concluida.

JWT: sim.

Request: sem body.

Response `204 No Content`: sem body.

## Prioridades

### GET `/api/Prioridades`

Lista as prioridades disponiveis.

JWT: sim.

Request: sem body.

Response `200 OK`:

```json
[
  {
    "id": 1,
    "nome": "Alta"
  },
  {
    "id": 2,
    "nome": "Media"
  },
  {
    "id": 3,
    "nome": "Baixa"
  }
]
```

### POST `/api/Prioridades`

Endpoint reservado. A criacao de prioridades nao esta disponivel pela API.

JWT: sim.

Request: sem body.

Response: `403 Forbidden`.

### PUT `/api/Prioridades/{id}`

Endpoint reservado. A atualizacao de prioridades nao esta disponivel pela API.

JWT: sim.

Request: sem body.

Response: `403 Forbidden`.

### DELETE `/api/Prioridades/{id}`

Endpoint reservado. A remocao de prioridades nao esta disponivel pela API.

JWT: sim.

Request: sem body.

Response: `403 Forbidden`.

## Erros e validacao

Erros de validacao usam `ValidationProblemDetails`.

Exemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "A requisicao contem dados invalidos.",
  "status": 400,
  "errors": {
    "email": [
      "The Email field is not a valid e-mail address."
    ]
  }
}
```

Erros de autenticacao ou regras de negocio usam `ProblemDetails`.

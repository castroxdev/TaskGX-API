# TaskGX API — Documentação dos Endpoints

Esta documentação apresenta as rotas declaradas nos controllers da TaskGX API, incluindo os pedidos, respostas e requisitos de autenticação.

Os exemplos utilizam JSON em `camelCase`, formato aceite pelo model binding do ASP.NET Core e utilizado pelos clientes do projeto.

## Índice

* [Endereços locais](#endereços-locais)
* [Autenticação JWT](#autenticação-jwt)
* [Resumo dos endpoints](#resumo-dos-endpoints)
* [Cadastro](#cadastro)
* [Autenticação](#autenticação)
* [Verificação de e-mail](#verificação-de-e-mail)
* [Utilizador autenticado](#utilizador-autenticado)
* [Listas](#listas)
* [Tarefas](#tarefas)
* [Prioridades](#prioridades)
* [Erros e validação](#erros-e-validação)

## Endereços locais

A API utiliza normalmente os seguintes endereços em ambiente de desenvolvimento:

* HTTPS: `https://localhost:7284`
* HTTP: `http://localhost:5192`

A documentação interativa do Swagger pode ser acedida através de:

```text
https://localhost:7284/swagger
```

As portas podem variar de acordo com a configuração presente em `Properties/launchSettings.json`.

## Autenticação JWT

Os endpoints privados exigem um token JWT no header `Authorization`:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

O token pode ser obtido através do login normal ou do login com Google.

Os endpoints que retornam `204 No Content` não possuem body na resposta.

## Resumo dos endpoints

| Método | URL                                          | JWT | Descrição                                               |
| ------ | -------------------------------------------- | --- | ------------------------------------------------------- |
| POST   | `/api/cadastro`                              | Não | Regista um novo utilizador.                             |
| POST   | `/api/autenticacao/login`                    | Não | Autentica com e-mail e palavra-passe.                   |
| POST   | `/api/autenticacao/google-login`             | Não | Autentica através de um Google ID Token.                |
| POST   | `/api/verificacao/verificar-email`           | Não | Confirma o e-mail através de um código de seis dígitos. |
| POST   | `/api/verificacao/reenviar-codigo`           | Não | Reenvia o código de verificação.                        |
| GET    | `/api/Usuarios/eu`                           | Sim | Obtém o utilizador autenticado.                         |
| PUT    | `/api/Usuarios/eu`                           | Sim | Atualiza o perfil do utilizador autenticado.            |
| DELETE | `/api/Usuarios/eu`                           | Sim | Elimina a conta e os dados associados.                  |
| PATCH  | `/api/Usuarios/eu/senha`                     | Sim | Altera a palavra-passe do utilizador autenticado.       |
| POST   | `/api/Usuarios/eu/email/solicitar-alteracao` | Sim | Solicita a alteração do endereço de e-mail.             |
| POST   | `/api/Usuarios/eu/email/confirmar-alteracao` | Sim | Confirma a alteração do endereço de e-mail.             |
| GET    | `/api/Listas`                                | Sim | Obtém as listas do utilizador.                          |
| POST   | `/api/Listas`                                | Sim | Cria uma lista.                                         |
| PUT    | `/api/Listas/{id}`                           | Sim | Atualiza uma lista.                                     |
| DELETE | `/api/Listas/{id}`                           | Sim | Elimina uma lista.                                      |
| GET    | `/api/Tarefas?listaId={listaId}`             | Sim | Obtém as tarefas de uma lista.                          |
| POST   | `/api/Tarefas`                               | Sim | Cria uma tarefa.                                        |
| PUT    | `/api/Tarefas/{id}`                          | Sim | Atualiza uma tarefa.                                    |
| DELETE | `/api/Tarefas/{id}`                          | Sim | Elimina uma tarefa.                                     |
| POST   | `/api/Tarefas/{id}/concluir`                 | Sim | Marca uma tarefa como concluída.                        |
| GET    | `/api/Prioridades`                           | Sim | Obtém as prioridades disponíveis.                       |
| POST   | `/api/Prioridades`                           | Sim | Endpoint reservado; retorna `403 Forbidden`.            |
| PUT    | `/api/Prioridades/{id}`                      | Sim | Endpoint reservado; retorna `403 Forbidden`.            |
| DELETE | `/api/Prioridades/{id}`                      | Sim | Endpoint reservado; retorna `403 Forbidden`.            |

---

## Cadastro

### POST `/api/cadastro`

Regista um novo utilizador e envia um código de verificação para o endereço de e-mail indicado.

**Autenticação:** não requer JWT.

#### Pedido

```json
{
  "nome": "Utilizador TaskGX",
  "email": "utilizador@example.com",
  "senha": "Senha123!",
  "confirmarSenha": "Senha123!"
}
```

#### Resposta de sucesso

`200 OK`

```json
{
  "mensagem": "Conta criada com sucesso. Verifique o seu email."
}
```

#### Possível resposta de erro

`400 Bad Request`

```json
{
  "title": "Nao foi possivel concluir o cadastro.",
  "detail": "Mensagem de erro do cadastro.",
  "status": 400
}
```

---

## Autenticação

### POST `/api/autenticacao/login`

Autentica um utilizador através de e-mail e palavra-passe.

**Autenticação:** não requer JWT.

#### Pedido

```json
{
  "email": "utilizador@example.com",
  "senha": "Senha123!"
}
```

#### Resposta de sucesso

`200 OK`

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

O valor de `token` deve ser utilizado nos endpoints protegidos.

#### Possível resposta de erro

`401 Unauthorized`

```json
{
  "title": "Falha na autenticacao.",
  "detail": "Credenciais invalidas ou usuario nao autorizado.",
  "status": 401
}
```

### POST `/api/autenticacao/google-login`

Autentica um utilizador através de um Google ID Token obtido pelo frontend.

**Autenticação:** não requer JWT.

#### Pedido

```json
{
  "idToken": "GOOGLE_ID_TOKEN"
}
```

#### Resposta de sucesso

`200 OK`

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

O token retornado pela TaskGX API deve ser utilizado nos restantes endpoints. O Google ID Token não substitui o JWT da aplicação.

---

## Verificação de e-mail

### POST `/api/verificacao/verificar-email`

Confirma o endereço de e-mail através de um código de seis dígitos.

**Autenticação:** não requer JWT.

#### Pedido

```json
{
  "email": "utilizador@example.com",
  "codigo": "123456"
}
```

#### Resposta de sucesso

`200 OK`

```json
{
  "mensagem": "Email verificado com sucesso."
}
```

### POST `/api/verificacao/reenviar-codigo`

Reenvia o código de verificação para o endereço de e-mail indicado.

**Autenticação:** não requer JWT.

#### Pedido

```json
{
  "email": "utilizador@example.com"
}
```

#### Resposta de sucesso

`200 OK`

```json
{
  "mensagem": "Codigo de verificacao reenviado com sucesso."
}
```

---

## Utilizador autenticado

### GET `/api/Usuarios/eu`

Obtém os dados do utilizador autenticado.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

#### Resposta de sucesso

`200 OK`

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

Atualiza o nome e o avatar do utilizador autenticado.

**Autenticação:** requer JWT.

#### Pedido

```json
{
  "nome": "Novo Nome",
  "avatar": "https://example.com/avatar.png"
}
```

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

### DELETE `/api/Usuarios/eu`

Elimina permanentemente a conta do utilizador autenticado e os dados associados.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

O endpoint não recebe um ID de utilizador. A conta é identificada exclusivamente através do JWT.

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

#### Respostas esperadas

| Código             | Descrição                                      |
| ------------------ | ---------------------------------------------- |
| `204 No Content`   | Conta eliminada com sucesso.                   |
| `401 Unauthorized` | JWT ausente, inválido ou expirado.             |
| `404 Not Found`    | O utilizador associado ao token já não existe. |

Depois de uma eliminação bem-sucedida, o cliente deve remover o JWT e encerrar a sessão local.

### PATCH `/api/Usuarios/eu/senha`

Altera a palavra-passe do utilizador autenticado.

**Autenticação:** requer JWT.

#### Pedido

```json
{
  "senhaAtual": "Senha123!",
  "novaSenha": "NovaSenha123!",
  "confirmarNovaSenha": "NovaSenha123!"
}
```

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

### POST `/api/Usuarios/eu/email/solicitar-alteracao`

Solicita a alteração do endereço de e-mail e envia um código de confirmação para o novo endereço.

**Autenticação:** requer JWT.

#### Pedido

```json
{
  "novoEmail": "novo-email@example.com"
}
```

#### Resposta de sucesso

`200 OK`

```json
{
  "mensagem": "Codigo de confirmacao enviado para o novo email."
}
```

### POST `/api/Usuarios/eu/email/confirmar-alteracao`

Confirma a alteração do endereço de e-mail através de um código de seis dígitos.

**Autenticação:** requer JWT.

#### Pedido

```json
{
  "codigo": "123456"
}
```

#### Resposta de sucesso

`200 OK`

```json
{
  "mensagem": "Email alterado com sucesso."
}
```

---

## Listas

### GET `/api/Listas`

Obtém as listas pertencentes ao utilizador autenticado.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

#### Resposta de sucesso

`200 OK`

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

Cria uma nova lista para o utilizador autenticado.

**Autenticação:** requer JWT.

#### Pedido

```json
{
  "nome": "Trabalho",
  "cor": "#2f80ed",
  "favorita": true
}
```

#### Resposta de sucesso

`201 Created`

```json
{
  "id": 1,
  "nome": "Trabalho",
  "cor": "#2f80ed",
  "favorita": true
}
```

### PUT `/api/Listas/{id}`

Atualiza uma lista pertencente ao utilizador autenticado.

**Autenticação:** requer JWT.

#### Parâmetro de rota

| Parâmetro | Tipo    | Descrição               |
| --------- | ------- | ----------------------- |
| `id`      | inteiro | Identificador da lista. |

#### Pedido

```json
{
  "nome": "Trabalho atualizado",
  "cor": "#27ae60",
  "favorita": false
}
```

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

### DELETE `/api/Listas/{id}`

Elimina uma lista pertencente ao utilizador autenticado.

**Autenticação:** requer JWT.

#### Parâmetro de rota

| Parâmetro | Tipo    | Descrição               |
| --------- | ------- | ----------------------- |
| `id`      | inteiro | Identificador da lista. |

#### Pedido

Sem body.

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

---

## Tarefas

### GET `/api/Tarefas?listaId={listaId}`

Obtém as tarefas de uma lista pertencente ao utilizador autenticado.

**Autenticação:** requer JWT.

#### Parâmetro de query

| Parâmetro | Tipo    | Descrição                                           |
| --------- | ------- | --------------------------------------------------- |
| `listaId` | inteiro | Identificador da lista cujas tarefas serão obtidas. |

#### Pedido

Sem body.

#### Resposta de sucesso

`200 OK`

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

Cria uma tarefa numa lista pertencente ao utilizador autenticado.

**Autenticação:** requer JWT.

#### Pedido

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

#### Resposta de sucesso

`201 Created`

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

Atualiza uma tarefa pertencente ao utilizador autenticado.

O `id` indicado na rota deve ser igual ao `id` enviado no body.

**Autenticação:** requer JWT.

#### Parâmetro de rota

| Parâmetro | Tipo    | Descrição                |
| --------- | ------- | ------------------------ |
| `id`      | inteiro | Identificador da tarefa. |

#### Pedido

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

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

### DELETE `/api/Tarefas/{id}`

Elimina uma tarefa pertencente ao utilizador autenticado.

**Autenticação:** requer JWT.

#### Parâmetro de rota

| Parâmetro | Tipo    | Descrição                |
| --------- | ------- | ------------------------ |
| `id`      | inteiro | Identificador da tarefa. |

#### Pedido

Sem body.

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

### POST `/api/Tarefas/{id}/concluir`

Marca uma tarefa como concluída.

**Autenticação:** requer JWT.

#### Parâmetro de rota

| Parâmetro | Tipo    | Descrição                |
| --------- | ------- | ------------------------ |
| `id`      | inteiro | Identificador da tarefa. |

#### Pedido

Sem body.

#### Resposta de sucesso

`204 No Content`

A resposta não possui body.

---

## Prioridades

### GET `/api/Prioridades`

Obtém as prioridades disponíveis para utilização nas tarefas.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

#### Resposta de sucesso

`200 OK`

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

Endpoint reservado.

A criação de prioridades não está disponível através da API.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

#### Resposta

`403 Forbidden`

### PUT `/api/Prioridades/{id}`

Endpoint reservado.

A atualização de prioridades não está disponível através da API.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

#### Resposta

`403 Forbidden`

### DELETE `/api/Prioridades/{id}`

Endpoint reservado.

A eliminação de prioridades não está disponível através da API.

**Autenticação:** requer JWT.

#### Pedido

Sem body.

#### Resposta

`403 Forbidden`

---

## Erros e validação

### ValidationProblemDetails

Erros de validação utilizam o formato `ValidationProblemDetails`.

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

A propriedade `errors` apresenta os campos inválidos e as respetivas mensagens de validação.

### ProblemDetails

Erros de autenticação, recursos inexistentes e regras de negócio utilizam o formato `ProblemDetails`.

Exemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Nao foi possivel concluir o pedido.",
  "status": 400,
  "detail": "Descricao do erro."
}
```

### Códigos HTTP utilizados

| Código                      | Significado                                |
| --------------------------- | ------------------------------------------ |
| `200 OK`                    | Pedido concluído com sucesso.              |
| `201 Created`               | Recurso criado com sucesso.                |
| `204 No Content`            | Pedido concluído sem conteúdo na resposta. |
| `400 Bad Request`           | Pedido ou dados inválidos.                 |
| `401 Unauthorized`          | JWT ausente, inválido ou expirado.         |
| `403 Forbidden`             | Operação não permitida.                    |
| `404 Not Found`             | Recurso não encontrado.                    |
| `409 Conflict`              | Conflito com um recurso existente.         |
| `500 Internal Server Error` | Erro interno inesperado.                   |

Nem todos os endpoints utilizam todos os códigos apresentados. Consulte o Swagger para verificar as respostas documentadas de cada operação.

---

[Voltar ao README principal](../README.md) | [Consultar a arquitetura](ARCHITECTURE.md)

# Documentção de Endpoints - Benefícios API

## Base URL
```
http://localhost:8080
ou
https://localhost:8081
```

## Autenticção

Todos os endpoints, exceto o de login, requerem autenticção via JWT Bearer Token.

**Header obrigatório:**
```
Authorization: Bearer {seu-token-jwt}
```

---

## Login

### POST /api/usuarios/login
Realiza o login do usuário e retorna o token JWT.

**Corpo da requisção:**
```json
{
  "email": "admin@exemplo.com",
  "senha": "admin123"
}
```

**Resposta de sucesso (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuarioId": "99999999-9999-9999-9999-999999999999",
  "nome": "Administrador",
  "email": "admin@exemplo.com"
}
```

**Resposta de erro (401):**
```json
{
  "message": "Email ou senha inválidos"
}
```

---

## Usuários

### POST /api/usuarios
Cria um novo usuário.

**Requer autenticção:** ?

**Corpo da requisção:**
```json
{
  "nome": "João Silva",
  "senha": "senha123",
  "email": "joao@exemplo.com",
  "empresaId": "11111111-1111-1111-1111-111111111111"
}
```

**Resposta de sucesso (201):**
```json
{
  "id": "33333333-3333-3333-3333-333333333333"
}
```

---

### PUT /api/usuarios/{id}
Atualiza um usuário existente.

**Requer autenticção:** ?

**Parâmetros de URL:**
- `id` (guid): ID do usuário

**Corpo da requisção:**
```json
{
  "nome": "João Silva Atualizado",
  "email": "joao.novo@exemplo.com",
  "empresaId": "11111111-1111-1111-1111-111111111111"
}
```

**Resposta de sucesso (204):** Sem conteúdo

**Resposta de erro (404):**
```json
{
  "message": "Usuário não encontrado"
}
```

---

### GET /api/usuarios/{id}
Busca um usuário por ID.

**Requer autenticção:** ?

**Parâmetros de URL:**
- `id` (guid): ID do usuário

**Resposta de sucesso (200):**
```json
{
  "id": "99999999-9999-9999-9999-999999999999",
  "nome": "Administrador",
  "email": "admin@exemplo.com",
  "empresaId": "11111111-1111-1111-1111-111111111111",
  "empresaNome": "Empresa Exemplo LTDA",
  "dataInclusao": "2024-01-15T10:30:00",
  "dataAlteracao": null
}
```

**Resposta de erro (404):**
```json
{
  "message": "Usuário não encontrado"
}
```

---

### GET /api/usuarios
Lista todos os usuários.

**Requer autenticção:** ?

**Resposta de sucesso (200):**
```json
[
  {
    "id": "99999999-9999-9999-9999-999999999999",
    "nome": "Administrador",
    "email": "admin@exemplo.com",
    "empresaId": "11111111-1111-1111-1111-111111111111",
    "empresaNome": "Empresa Exemplo LTDA",
    "dataInclusao": "2024-01-15T10:30:00",
    "dataAlteracao": null
  },
  {
    "id": "88888888-8888-8888-8888-888888888888",
    "nome": "Usuário Teste",
    "email": "teste@exemplo.com",
    "empresaId": "11111111-1111-1111-1111-111111111111",
    "empresaNome": "Empresa Exemplo LTDA",
    "dataInclusao": "2024-01-15T11:00:00",
    "dataAlteracao": null
  }
]
```

---

### DELETE /api/usuarios/{id}
Deleta um usuário.

**Requer autenticção:** ?

**Parâmetros de URL:**
- `id` (guid): ID do usuário

**Resposta de sucesso (204):** Sem conteúdo

**Resposta de erro (404):**
```json
{
  "message": "Usuário não encontrado"
}
```

---

## Empresas

### POST /api/empresas
Cria uma nova empresa.

**Requer autenticção:** ?

**Corpo da requisção:**
```json
{
  "razaoSocial": "Minha Empresa LTDA",
  "dominio": "minhaempresa",
  "nomeBanco": "db_minhaempresa",
  "usuarioBanco": "user_db",
  "senhaBanco": "senha_db"
}
```

**Resposta de sucesso (201):**
```json
{
  "id": "44444444-4444-4444-4444-444444444444"
}
```

---

### PUT /api/empresas/{id}
Atualiza uma empresa existente.

**Requer autenticção:** ?

**Parâmetros de URL:**
- `id` (guid): ID da empresa

**Corpo da requisção:**
```json
{
  "razaoSocial": "Minha Empresa LTDA - Atualizada",
  "dominio": "minhaempresa",
  "nomeBanco": "db_minhaempresa_novo",
  "usuarioBanco": "user_db_novo",
  "senhaBanco": "senha_db_nova"
}
```

**Resposta de sucesso (204):** Sem conteúdo

**Resposta de erro (404):**
```json
{
  "message": "Empresa não encontrada"
}
```

---

### GET /api/empresas/{id}
Busca uma empresa por ID.

**Requer autenticção:** ?

**Parâmetros de URL:**
- `id` (guid): ID da empresa

**Resposta de sucesso (200):**
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "razaoSocial": "Empresa Exemplo LTDA",
  "dominio": "exemplo",
  "dataInclusao": "2024-01-15T09:00:00",
  "dataAlteracao": null
}
```

**Resposta de erro (404):**
```json
{
  "message": "Empresa não encontrada"
}
```

---

### GET /api/empresas
Lista todas as empresas.

**Requer autenticção:** ?

**Resposta de sucesso (200):**
```json
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "razaoSocial": "Empresa Exemplo LTDA",
    "dominio": "exemplo",
    "dataInclusao": "2024-01-15T09:00:00",
    "dataAlteracao": null
  },
  {
    "id": "22222222-2222-2222-2222-222222222222",
    "razaoSocial": "Outra Empresa SA",
    "dominio": "outra",
    "dataInclusao": "2024-01-15T09:30:00",
    "dataAlteracao": null
  }
]
```

---

### DELETE /api/empresas/{id}
Deleta uma empresa.

**Requer autenticção:** ?

**Parâmetros de URL:**
- `id` (guid): ID da empresa

**Resposta de sucesso (204):** Sem conteúdo

**Resposta de erro (404):**
```json
{
  "message": "Empresa não encontrada"
}
```

---

## Health Check

### GET /health
Verifica o status da API.

**Requer autenticção:** ?

**Resposta de sucesso (200):**
```
Healthy
```

---

## Códigos de Status HTTP

| Código | Descrição |
|--------|-----------|
| 200 | OK - Requisção bem-sucedida |
| 201 | Created - Recurso criado com sucesso |
| 204 | No Content - Operção bem-sucedida sem retorno |
| 400 | Bad Request - Dados inválidos |
| 401 | Unauthorized - Não autenticado ou token inválido |
| 404 | Not Found - Recurso não encontrado |
| 500 | Internal Server Error - Erro interno do servidor |

---

## Observções de Segurança

1. **Senhas**: São criptografadas em Base64 (considere usar bcrypt em produção)
2. **Dados de Conexão de Banco**: Campos `nomeBanco`, `usuarioBanco` e `senhaBanco` são criptografados
3. **Token JWT**: Expira em 8 horas
4. **HTTPS**: Sempre use HTTPS em produção

---

## Exemplo de Uso com cURL

### Login
```bash
curl -X POST http://localhost:8080/api/usuarios/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@exemplo.com","senha":"admin123"}'
```

### Listar Usuários (com token)
```bash
curl -X GET http://localhost:8080/api/usuarios \
  -H "Authorization: Bearer {seu-token}"
```

### Criar Empresa
```bash
curl -X POST http://localhost:8080/api/empresas \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "razaoSocial": "Nova Empresa",
    "dominio": "novaempresa",
    "nomeBanco": "db_nova",
    "usuarioBanco": "user_nova",
    "senhaBanco": "senha_nova"
  }'
```

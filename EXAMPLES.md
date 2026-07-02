# Exemplos de Uso - Benef�cios API

Este documento cont�m exemplos pr�ticos de como usar a API.

## ?? Base URL

```
http://localhost:8080
```

---

## ?? Fluxo Completo de Uso

### 1?? Criar uma Empresa

**Requisi��o:**
```http
POST /api/empresas
Content-Type: application/json
Authorization: Bearer {seu-token}

{
  "razaoSocial": "Tech Solutions LTDA",
  "dominio": "techsolutions",
  "nomeBanco": "db_techsolutions",
  "usuarioBanco": "tech_user",
  "senhaBanco": "tech_pass123"
}
```

**Resposta:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

---

### 2?? Criar um Usu�rio

**Requisi��o:**
```http
POST /api/usuarios
Content-Type: application/json
Authorization: Bearer {seu-token}

{
  "nome": "Maria Silva",
  "senha": "senha123",
  "email": "maria@techsolutions.com",
  "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

**Resposta:**
```json
{
  "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901"
}
```

---

### 3?? Fazer Login

**Requisi��o:**
```http
POST /api/usuarios/login
Content-Type: application/json

{
  "email": "maria@techsolutions.com",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJiMmMzZDRlNS1mNmE3LTg5MDEtYmNkZS1mMTIzNDU2Nzg5MDEiLCJlbWFpbCI6Im1hcmlhQHRlY2hzb2x1dGlvbnMuY29tIiwibmJmIjoxNzA1MzI0ODAwLCJleHAiOjE3MDUzNTM2MDAsImlhdCI6MTcwNTMyNDgwMCwiaXNzIjoiQmVuZWZpY2lvc0FwaSIsImF1ZCI6IkJlbmVmaWNpb3NDbGllbnQifQ.signature",
  "usuarioId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "nome": "Maria Silva",
  "email": "maria@techsolutions.com"
}
```

---

### 4?? Buscar Usu�rio por ID

**Requisi��o:**
```http
GET /api/usuarios/b2c3d4e5-f6a7-8901-bcde-f12345678901
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**
```json
{
  "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "nome": "Maria Silva",
  "email": "maria@techsolutions.com",
  "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "empresaNome": "Tech Solutions LTDA",
  "dataInclusao": "2024-01-15T10:30:00",
  "dataAlteracao": null
}
```

---

### 5?? Listar Todos os Usu�rios

**Requisi��o:**
```http
GET /api/usuarios
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**
```json
[
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "nome": "Maria Silva",
    "email": "maria@techsolutions.com",
    "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "empresaNome": "Tech Solutions LTDA",
    "dataInclusao": "2024-01-15T10:30:00",
    "dataAlteracao": null
  },
  {
    "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
    "nome": "Jo�o Santos",
    "email": "joao@techsolutions.com",
    "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "empresaNome": "Tech Solutions LTDA",
    "dataInclusao": "2024-01-15T11:00:00",
    "dataAlteracao": null
  }
]
```

---

### 6?? Atualizar Usu�rio

**Requisi��o:**
```http
PUT /api/usuarios/b2c3d4e5-f6a7-8901-bcde-f12345678901
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "nome": "Maria Silva Santos",
  "email": "maria.santos@techsolutions.com",
  "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

**Resposta:**
```http
204 No Content
```

---

### 7?? Deletar Usu�rio

**Requisi��o:**
```http
DELETE /api/usuarios/b2c3d4e5-f6a7-8901-bcde-f12345678901
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**
```http
204 No Content
```

---

### 8?? Buscar Empresa por ID

**Requisi��o:**
```http
GET /api/empresas/a1b2c3d4-e5f6-7890-abcd-ef1234567890
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "razaoSocial": "Tech Solutions LTDA",
  "dominio": "techsolutions",
  "dataInclusao": "2024-01-15T09:00:00",
  "dataAlteracao": null
}
```

---

### 9?? Listar Todas as Empresas

**Requisi��o:**
```http
GET /api/empresas
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**
```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "razaoSocial": "Tech Solutions LTDA",
    "dominio": "techsolutions",
    "dataInclusao": "2024-01-15T09:00:00",
    "dataAlteracao": null
  },
  {
    "id": "d4e5f6a7-b8c9-0123-def1-234567890123",
    "razaoSocial": "Innovation Corp",
    "dominio": "innovation",
    "dataInclusao": "2024-01-15T09:30:00",
    "dataAlteracao": null
  }
]
```

---

### ?? Atualizar Empresa

**Requisi��o:**
```http
PUT /api/empresas/a1b2c3d4-e5f6-7890-abcd-ef1234567890
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "razaoSocial": "Tech Solutions LTDA - Matriz",
  "dominio": "techsolutions",
  "nomeBanco": "db_techsolutions_v2",
  "usuarioBanco": "tech_user_v2",
  "senhaBanco": "tech_pass_v2_123"
}
```

**Resposta:**
```http
204 No Content
```

---

### 1??1?? Deletar Empresa

**Requisi��o:**
```http
DELETE /api/empresas/a1b2c3d4-e5f6-7890-abcd-ef1234567890
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**
```http
204 No Content
```

---

### 1??2?? Health Check

**Requisi��o:**
```http
GET /health
```

**Resposta:**
```
Healthy
```

---

## ?? Testando com cURL

### Login
```bash
curl -X POST http://localhost:8080/api/usuarios/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@exemplo.com",
    "senha": "admin123"
  }'
```

### Criar Empresa
```bash
TOKEN="seu-token-aqui"

curl -X POST http://localhost:8080/api/empresas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "razaoSocial": "Minha Empresa",
    "dominio": "minhaempresa",
    "nomeBanco": "db_minha",
    "usuarioBanco": "user_minha",
    "senhaBanco": "pass_minha"
  }'
```

### Listar Usu�rios
```bash
TOKEN="seu-token-aqui"

curl -X GET http://localhost:8080/api/usuarios \
  -H "Authorization: Bearer $TOKEN"
```

---

## ?? Testando com PowerShell

### Login
```powershell
$body = @{
    email = "admin@exemplo.com"
    senha = "admin123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:8080/api/usuarios/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

$token = $response.token
Write-Host "Token: $token"
```

### Criar Empresa
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    razaoSocial = "Minha Empresa"
    dominio = "minhaempresa"
    nomeBanco = "db_minha"
    usuarioBanco = "user_minha"
    senhaBanco = "pass_minha"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:8080/api/empresas" `
    -Method Post `
    -Headers $headers `
    -Body $body

Write-Host "ID da Empresa: $($response.id)"
```

### Listar Usu�rios
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

$usuarios = Invoke-RestMethod -Uri "http://localhost:8080/api/usuarios" `
    -Method Get `
    -Headers $headers

$usuarios | Format-Table -Property id, nome, email, empresaNome
```

---

## ?? Testando com Postman

### 1. Importar Collection

Crie uma collection no Postman com as seguintes vari�veis:

- `baseUrl`: `http://localhost:8080`
- `token`: (ser� preenchido ap�s login)

### 2. Configurar Autentica��o

Para todos os endpoints (exceto login):

1. V� em **Authorization**
2. Selecione **Bearer Token**
3. Use a vari�vel `{{token}}`

### 3. Script de Login

No endpoint de login, adicione este script em **Tests**:

```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.collectionVariables.set("token", jsonData.token);
    console.log("Token salvo:", jsonData.token);
}
```

---

## ?? Exemplos de Erros Comuns

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

**Solu��o:** Verifique se o token JWT est� correto e n�o expirou.

---

### 404 Not Found
```json
{
  "message": "Usu�rio n�o encontrado"
}
```

**Solu��o:** Verifique se o ID existe no banco de dados.

---

### 500 Internal Server Error
```json
{
  "message": "Erro ao criar usu�rio"
}
```

**Solu��o:** Verifique os logs da aplica��o para mais detalhes.

---

## ?? Cen�rios de Teste

### Cen�rio 1: Onboarding de Nova Empresa

1. Criar empresa
2. Criar usu�rio admin da empresa
3. Login com o novo usu�rio
4. Criar mais usu�rios

### Cen�rio 2: Gest�o de Usu�rios

1. Login
2. Listar todos os usu�rios
3. Buscar usu�rio espec�fico
4. Atualizar dados do usu�rio
5. Deletar usu�rio

### Cen�rio 3: Autentica��o e Autoriza��o

1. Tentar acessar endpoint sem token (deve falhar)
2. Fazer login
3. Usar token para acessar endpoints
4. Aguardar expira��o do token (8 horas)
5. Tentar usar token expirado (deve falhar)
6. Fazer novo login

---

## ?? Integra��o com Frontend

### Exemplo com JavaScript (Fetch API)

```javascript
// Login
async function login(email, senha) {
    const response = await fetch('http://localhost:8080/api/usuarios/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, senha })
    });

    const data = await response.json();
    localStorage.setItem('token', data.token);
    return data;
}

// Listar Usu�rios
async function getUsuarios() {
    const token = localStorage.getItem('token');

    const response = await fetch('http://localhost:8080/api/usuarios', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return await response.json();
}

// Criar Empresa
async function createEmpresa(empresa) {
    const token = localStorage.getItem('token');

    const response = await fetch('http://localhost:8080/api/empresas', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(empresa)
    });

    return await response.json();
}
```

---

## ?? Boas Pr�ticas

1. **Sempre use HTTPS em produ��o**
2. **Armazene o token de forma segura** (HttpOnly cookies no frontend)
3. **Implemente refresh token** para melhor UX
4. **Valide os dados no frontend** antes de enviar
5. **Trate erros adequadamente**
6. **Use interceptors** para adicionar token automaticamente
7. **Implemente logout** limpando o token
8. **N�o armazene senhas** em texto plano no c�digo

---

Para mais informa��es, consulte a [Documenta��o de Endpoints](ENDPOINTS.md).

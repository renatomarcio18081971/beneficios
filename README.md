# Benefícios API

API desenvolvida em .NET 9 seguindo princípios de Domain-Driven Design (DDD) de forma enxuta.

## Arquitetura

A solução está organizada em camadas seguindo DDD:

```
beneficios-api
├── src
│   ├── Beneficios.Api             # Camada de Apresentação (Controllers)
│   ├── Beneficios.Application     # Camada de Aplicação (Services, DTOs, Mappings)
│   ├── Beneficios.Domain          # Camada de Domínio (Entities, Interfaces, ValueObjects)
│   └── Beneficios.Infrastructure  # Camada de Infraestrutura (Repositories, Configurations)
└── tests
    └── Beneficios.Tests           # Testes Unitários

```

## Tecnologias Utilizadas

- **.NET 9**
- **PostgreSQL** - Banco de dados
- **Dapper** - Micro ORM para alta performance
- **AutoMapper** - Mapeamento entre objetos
- **JWT** - Autenticção e autorizção
- **Serilog** - Logs estruturados
- **xUnit + Moq** - Testes unitários
- **Docker** - Containerizção

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)
- [PostgreSQL](https://www.postgresql.org/download/) (caso não use Docker)

## Configurção

### appsettings.json

Configure a connection string do PostgreSQL e as configurções do JWT:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=beneficios;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres",
    "Issuer": "BeneficiosApi",
    "Audience": "BeneficiosClient"
  }
}
```

## Docker

### Subir a aplicção com Docker Compose

```bash
docker-compose up -d
```

Isso irá subir:
- PostgreSQL na porta 5432
- API na porta 8080

### Criar apenas o banco de dados

```bash
docker-compose up -d postgres
```

## Database

Os scripts SQL estão em `src/Beneficios.Infrastructure/Scripts/`:

1. `01_Create_Table_Empresas.sql` - Cria tabela de empresas
2. `02_Create_Table_Usuarios.sql` - Cria tabela de usuários

Execute os scripts na ordem para criar o banco de dados.

## Como Executar Localmente

### Via Visual Studio / Visual Studio Code

1. Abra a solution `beneficios-api.sln`
2. Configure o projeto `Beneficios.Api` como projeto de inicializção
3. Execute (F5)

### Via CLI

```bash
cd src/Beneficios.Api
dotnet run
```

A API estár disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `http://localhost:5000/swagger`
- Health Check: `http://localhost:5000/health`

## Executar Testes

```bash
dotnet test
```

## Endpoints

### Autenticção

- **POST** `/api/usuarios/login` - Fazer login (público)

### Usuários (Autenticado)

- **POST** `/api/usuarios` - Criar usuário
- **PUT** `/api/usuarios/{id}` - Atualizar usuário
- **GET** `/api/usuarios/{id}` - Buscar usuário por ID
- **GET** `/api/usuarios` - Listar todos os usuários
- **DELETE** `/api/usuarios/{id}` - Deletar usuário

### Empresas (Autenticado)

- **POST** `/api/empresas` - Criar empresa
- **PUT** `/api/empresas/{id}` - Atualizar empresa
- **GET** `/api/empresas/{id}` - Buscar empresa por ID
- **GET** `/api/empresas` - Listar todas as empresas
- **DELETE** `/api/empresas/{id}` - Deletar empresa

## Autenticção

A API utiliza JWT Bearer Token. Para acessar endpoints protegidos:

1. Faça login em `/api/usuarios/login`
2. Copie o token retornado
3. Adicione o header: `Authorization: Bearer {seu-token}`

## Observções

- Senhas são criptografadas usando Base64 (considere usar bcrypt em produção)
- Dados sensíveis de empresas (conexão com banco) são criptografados
- Logs são salvos na pasta `logs/`
- Health check disponível em `/health`

## Licença


Este projeto é de código aberto.
=======

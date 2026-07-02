# Benef�cios API

API desenvolvida em .NET 9 seguindo princ�pios de Domain-Driven Design (DDD) de forma enxuta.

## ??? Arquitetura

A solu��o est� organizada em camadas seguindo DDD:

```
?? beneficios-api
 ??? ?? src
 ?    ??? ?? Beneficios.Api           # Camada de Apresenta��o (Controllers)
 ?    ??? ?? Beneficios.Application   # Camada de Aplica��o (Services, DTOs, Mappings)
 ?    ??? ?? Beneficios.Domain        # Camada de Dom�nio (Entities, Interfaces, ValueObjects)
 ?    ??? ?? Beneficios.Infrastructure # Camada de Infraestrutura (Repositories, Configurations)
 ??? ?? tests
      ??? ?? Beneficios.Tests         # Testes Unit�rios
```

## ?? Tecnologias Utilizadas

- **.NET 9**
- **PostgreSQL** - Banco de dados
- **Dapper** - Micro ORM para alta performance
- **AutoMapper** - Mapeamento entre objetos
- **JWT** - Autentica��o e autoriza��o
- **Serilog** - Logs estruturados
- **xUnit + Moq** - Testes unit�rios
- **Docker** - Containeriza��o

## ?? Pr�-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)
- [PostgreSQL](https://www.postgresql.org/download/) (caso n�o use Docker)

## ?? Configura��o

### appsettings.json

Configure a connection string do PostgreSQL e as configura��es do JWT:

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

## ?? Docker

### Subir a aplica��o com Docker Compose

```bash
docker-compose up -d
```

Isso ir� subir:
- PostgreSQL na porta 5432
- API na porta 8080

### Criar apenas o banco de dados

```bash
docker-compose up -d postgres
```

## ??? Database

Os scripts SQL est�o em `src/Beneficios.Infrastructure/Scripts/`:

1. `01_Create_Table_Empresas.sql` - Cria tabela de empresas
2. `02_Create_Table_Usuarios.sql` - Cria tabela de usu�rios

Execute os scripts na ordem para criar o banco de dados.

## ?? Como Executar Localmente

### Via Visual Studio / Visual Studio Code

1. Abra a solution `beneficios-api.sln`
2. Configure o projeto `Beneficios.Api` como projeto de inicializa��o
3. Execute (F5)

### Via CLI

```bash
cd src/Beneficios.Api
dotnet run
```

A API estar� dispon�vel em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `http://localhost:5000/swagger`
- Health Check: `http://localhost:5000/health`

## ?? Executar Testes

```bash
dotnet test
```

## ?? Endpoints

### Autentica��o

- **POST** `/api/usuarios/login` - Fazer login (p�blico)

### Usu�rios (Autenticado)

- **POST** `/api/usuarios` - Criar usu�rio
- **PUT** `/api/usuarios/{id}` - Atualizar usu�rio
- **GET** `/api/usuarios/{id}` - Buscar usu�rio por ID
- **GET** `/api/usuarios` - Listar todos os usu�rios
- **DELETE** `/api/usuarios/{id}` - Deletar usu�rio

### Empresas (Autenticado)

- **POST** `/api/empresas` - Criar empresa
- **PUT** `/api/empresas/{id}` - Atualizar empresa
- **GET** `/api/empresas/{id}` - Buscar empresa por ID
- **GET** `/api/empresas` - Listar todas as empresas
- **DELETE** `/api/empresas/{id}` - Deletar empresa

## ?? Autentica��o

A API utiliza JWT Bearer Token. Para acessar endpoints protegidos:

1. Fa�a login em `/api/usuarios/login`
2. Copie o token retornado
3. Adicione o header: `Authorization: Bearer {seu-token}`

## ?? Observa��es

- Senhas s�o criptografadas usando Base64 (considere usar bcrypt em produ��o)
- Dados sens�veis de empresas (conex�o com banco) s�o criptografados
- Logs s�o salvos na pasta `logs/`
- Health check dispon�vel em `/health`

## ?? Licen�a

Este projeto � de c�digo aberto.

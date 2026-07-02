# Estrutura do Projeto - Benef�cios API

## ?? Estrutura de Diret�rios Completa

```
C:\Projetos\dotnet\Beneficios\
?
??? ?? beneficios-api.sln                      # Solution principal
??? ?? .gitignore                              # Arquivos ignorados pelo Git
??? ?? README.md                               # Documenta��o principal
??? ?? ENDPOINTS.md                            # Documenta��o de endpoints
??? ?? EXAMPLES.md                             # Exemplos de uso
??? ?? TESTING.md                              # Guia de testes
??? ?? DEPLOY.md                               # Guia de deploy
??? ?? docker-compose.yml                      # Configura��o Docker
?
??? ?? src/
?   ?
?   ??? ?? Beneficios.Api/                     # Camada de Apresenta��o
?   ?   ??? ?? Controllers/
?   ?   ?   ??? UsuariosController.cs          # Controller de usu�rios (POST, PUT, GET, DELETE, LOGIN)
?   ?   ?   ??? EmpresasController.cs          # Controller de empresas (POST, PUT, GET, DELETE)
?   ?   ?
?   ?   ??? ?? Properties/
?   ?   ?   ??? launchSettings.json            # Configura��es de execu��o
?   ?   ?
?   ?   ??? Program.cs                         # Configura��o da aplica��o
?   ?   ??? appsettings.json                   # Configura��es de produ��o
?   ?   ??? appsettings.Development.json       # Configura��es de desenvolvimento
?   ?   ??? Dockerfile                         # Imagem Docker da API
?   ?   ??? docker-compose.override.yml        # Sobrescritas Docker
?   ?   ??? Beneficios.Api.csproj             # Projeto da API
?   ?
?   ??? ?? Beneficios.Application/             # Camada de Aplica��o
?   ?   ??? ?? Services/
?   ?   ?   ??? UsuarioService.cs              # L�gica de neg�cio de usu�rios
?   ?   ?   ??? EmpresaService.cs              # L�gica de neg�cio de empresas
?   ?   ?   ??? TokenService.cs                # Gera��o e valida��o de JWT
?   ?   ?
?   ?   ??? ?? DTOs/
?   ?   ?   ??? UsuarioCreateDto.cs            # DTO para criar usu�rio
?   ?   ?   ??? UsuarioUpdateDto.cs            # DTO para atualizar usu�rio
?   ?   ?   ??? UsuarioDto.cs                  # DTO de retorno de usu�rio
?   ?   ?   ??? LoginDto.cs                    # DTO de login
?   ?   ?   ??? LoginResponseDto.cs            # DTO de resposta de login
?   ?   ?   ??? EmpresaCreateDto.cs            # DTO para criar empresa
?   ?   ?   ??? EmpresaUpdateDto.cs            # DTO para atualizar empresa
?   ?   ?   ??? EmpresaDto.cs                  # DTO de retorno de empresa
?   ?   ?
?   ?   ??? ?? Interfaces/
?   ?   ?   ??? IUsuarioService.cs             # Interface do servi�o de usu�rios
?   ?   ?   ??? IEmpresaService.cs             # Interface do servi�o de empresas
?   ?   ?   ??? ITokenService.cs               # Interface do servi�o de token
?   ?   ?
?   ?   ??? ?? Mappings/
?   ?   ?   ??? MappingProfile.cs              # Perfis do AutoMapper
?   ?   ?
?   ?   ??? Beneficios.Application.csproj      # Projeto da Application
?   ?
?   ??? ?? Beneficios.Domain/                  # Camada de Dom�nio
?   ?   ??? ?? Entities/
?   ?   ?   ??? Usuario.cs                     # Entidade de usu�rio
?   ?   ?   ??? Empresa.cs                     # Entidade de empresa
?   ?   ?
?   ?   ??? ?? Interfaces/
?   ?   ?   ??? IUsuarioRepository.cs          # Interface do reposit�rio de usu�rios
?   ?   ?   ??? IEmpresaRepository.cs          # Interface do reposit�rio de empresas
?   ?   ?
?   ?   ??? ?? ValueObjects/
?   ?   ?   ??? Criptografia.cs                # Objeto de valor para criptografia
?   ?   ?
?   ?   ??? Beneficios.Domain.csproj           # Projeto do Domain
?   ?
?   ??? ?? Beneficios.Infrastructure/          # Camada de Infraestrutura
?       ??? ?? Repositories/
?       ?   ??? UsuarioRepository.cs           # Implementa��o do reposit�rio de usu�rios (Dapper)
?       ?   ??? EmpresaRepository.cs           # Implementa��o do reposit�rio de empresas (Dapper)
?       ?
?       ??? ?? Configurations/
?       ?   ??? DatabaseConfiguration.cs       # Configura��o de conex�o com banco
?       ?
?       ??? ?? Scripts/
?       ?   ??? 00_Init_Database.sql           # Script de inicializa��o completa
?       ?   ??? 01_Create_Table_Empresas.sql   # Script de cria��o da tabela empresas
?       ?   ??? 02_Create_Table_Usuarios.sql   # Script de cria��o da tabela usuarios
?       ?   ??? 03_Insert_Sample_Data.sql      # Script com dados de exemplo
?       ?
?       ??? Beneficios.Infrastructure.csproj   # Projeto da Infrastructure
?
??? ?? tests/
    ??? ?? Beneficios.Tests/                   # Projeto de Testes
        ??? ?? Domain/
        ?   ??? UsuarioTests.cs                # Testes da entidade usu�rio
        ?   ??? EmpresaTests.cs                # Testes da entidade empresa
        ?   ??? CriptografiaTests.cs           # Testes de criptografia
        ?
        ??? ?? Application/
        ?   ??? UsuarioServiceTests.cs         # Testes do servi�o de usu�rios
        ?   ??? EmpresaServiceTests.cs         # Testes do servi�o de empresas
        ?   ??? TokenServiceTests.cs           # Testes do servi�o de token
        ?
        ??? ?? Api/
        ?   ??? UsuariosControllerTests.cs     # Testes da controller de usu�rios
        ?   ??? EmpresasControllerTests.cs     # Testes da controller de empresas
        ?
        ??? Beneficios.Tests.csproj            # Projeto de testes
```

---

## ?? Pacotes NuGet Utilizados

### Beneficios.Api
- `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1
- `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.0
- `Microsoft.AspNetCore.OpenApi` 9.0.17
- `Serilog.AspNetCore` 10.0.0
- `Serilog.Sinks.File` 7.0.0
- `Swashbuckle.AspNetCore` 7.2.0

### Beneficios.Application
- `AutoMapper` 12.0.1
- `System.IdentityModel.Tokens.Jwt` 8.3.1

### Beneficios.Infrastructure
- `Dapper` 2.1.79
- `Npgsql` 10.0.3

### Beneficios.Tests
- `coverlet.collector` 6.0.2
- `FluentAssertions` 8.10.0
- `Microsoft.NET.Test.Sdk` 17.12.0
- `Moq` 4.20.72
- `xunit` 2.9.2
- `xunit.runner.visualstudio` 2.8.2

---

## ??? Estrutura do Banco de Dados

### Tabela: empresas

| Coluna               | Tipo          | Descri��o                          |
|----------------------|---------------|------------------------------------|
| id                   | UUID          | Chave prim�ria                     |
| razao_social         | VARCHAR(50)   | Raz�o social da empresa            |
| dominio              | VARCHAR(20)   | Dom�nio/slug da empresa            |
| nome_banco           | VARCHAR(256)  | Nome do banco (criptografado)      |
| usuario_banco        | VARCHAR(256)  | Usu�rio do banco (criptografado)   |
| senha_banco          | VARCHAR(256)  | Senha do banco (criptografada)     |
| data_inclusao        | TIMESTAMP     | Data de cria��o                    |
| data_alteracao       | TIMESTAMP     | Data da �ltima altera��o (nullable)|
| usuario_alteracao_id | UUID          | ID do usu�rio que alterou (nullable)|

### Tabela: usuarios

| Coluna               | Tipo          | Descri��o                          |
|----------------------|---------------|------------------------------------|
| id                   | UUID          | Chave prim�ria                     |
| nome                 | VARCHAR(50)   | Nome do usu�rio                    |
| senha                | VARCHAR(256)  | Senha (criptografada)              |
| email                | VARCHAR(256)  | Email (�nico)                      |
| empresa_id           | UUID          | FK para empresas                   |
| token                | TEXT          | Token JWT atual                    |
| data_inclusao        | TIMESTAMP     | Data de cria��o                    |
| data_alteracao       | TIMESTAMP     | Data da �ltima altera��o (nullable)|
| usuario_alteracao_id | UUID          | ID do usu�rio que alterou (nullable)|

**Relacionamentos:**
- `usuarios.empresa_id` ? `empresas.id` (FK com CASCADE)

---

## ??? Arquitetura DDD

### Domain (N�cleo)
- **Entities**: Objetos com identidade �nica (Usuario, Empresa)
- **Value Objects**: Objetos sem identidade (Criptografia)
- **Interfaces**: Contratos dos reposit�rios

### Application (Casos de Uso)
- **Services**: Orquestra��o da l�gica de neg�cio
- **DTOs**: Objetos de transfer�ncia de dados
- **Interfaces**: Contratos dos servi�os
- **Mappings**: Mapeamento entre entidades e DTOs

### Infrastructure (Detalhes T�cnicos)
- **Repositories**: Implementa��o de acesso a dados com Dapper
- **Configurations**: Configura��es de banco de dados
- **Scripts**: Scripts SQL de cria��o de tabelas

### API (Interface)
- **Controllers**: Endpoints HTTP
- **Program.cs**: Configura��o de DI, middleware, etc.

---

## ?? Fluxo de Requisi��o

```
1. Cliente HTTP
   ?
2. Controller (Beneficios.Api)
   ?
3. Service (Beneficios.Application)
   ?
4. Repository (Beneficios.Infrastructure)
   ?
5. Database (PostgreSQL)
```

**Fluxo de Resposta:**
```
5. Database (PostgreSQL)
   ?
4. Repository ? dynamic/entity
   ?
3. Service ? DTO (via AutoMapper)
   ?
2. Controller ? JSON
   ?
1. Cliente HTTP
```

---

## ?? Seguran�a Implementada

1. **Autentica��o JWT**
   - Token gerado no login
   - Expira��o de 8 horas
   - Valida��o em todos os endpoints (exceto login)

2. **Criptografia**
   - Senhas de usu�rios (Base64)
   - Dados de conex�o de banco (Base64)
   - ?? **Nota**: Para produ��o, usar bcrypt/Argon2

3. **CORS**
   - Configur�vel no Program.cs

4. **HTTPS**
   - Recomendado para produ��o

---

## ?? Observabilidade

### Logs (Serilog)
- Console
- Arquivo: `logs/beneficios-YYYYMMDD.txt`
- Rotation: Di�ria

### Health Check
- Endpoint: `/health`
- Usado pelo Docker

### M�tricas
- Pronto para integra��o com Prometheus/Grafana

---

## ?? Docker

### Containers
1. **postgres**: PostgreSQL 16 Alpine
2. **api**: .NET 9 API

### Volumes
- `postgres_data`: Persist�ncia do banco de dados

### Networks
- `beneficios-network`: Comunica��o entre containers

### Portas
- PostgreSQL: 5432
- API: 8080 (HTTP), 8081 (HTTPS)

---

## ? Checklist de Funcionalidades

### Implementado ?

- [x] Estrutura DDD enxuta
- [x] CRUD completo de Usu�rios
- [x] CRUD completo de Empresas
- [x] Autentica��o JWT
- [x] Logs com Serilog
- [x] Health Check
- [x] AutoMapper
- [x] Dapper para performance
- [x] PostgreSQL
- [x] Docker e Docker Compose
- [x] Testes unit�rios (Domain, Application, API)
- [x] Swagger/OpenAPI
- [x] Scripts SQL
- [x] Documenta��o completa

### Sugest�es para Evolu��o ??

- [ ] Refresh Token
- [ ] Pagina��o nas listagens
- [ ] Filtros e ordena��o
- [ ] Valida��es com FluentValidation
- [ ] Cache com Redis
- [ ] Rate limiting
- [ ] CQRS com MediatR
- [ ] Event sourcing
- [ ] Notifica��es (SignalR)
- [ ] Multi-tenancy
- [ ] Auditoria completa
- [ ] Soft delete
- [ ] Versionamento de API
- [ ] Internacionaliza��o (i18n)
- [ ] Testes de integra��o
- [ ] Performance tests

---

## ?? Documenta��o Dispon�vel

1. **README.md** - Vis�o geral e quick start
2. **ENDPOINTS.md** - Documenta��o completa de todos os endpoints
3. **EXAMPLES.md** - Exemplos pr�ticos de uso
4. **TESTING.md** - Guia de testes
5. **DEPLOY.md** - Guia de deploy (Docker, Azure, K8s, VM)
6. **Swagger** - Documenta��o interativa (http://localhost:5000/swagger)

---

## ?? Conceitos DDD Aplicados

### Camadas
? Separa��o clara de responsabilidades

### Entities
? Usuario e Empresa com identidade �nica (Guid)

### Value Objects
? Criptografia (comportamento sem identidade)

### Repositories
? Interfaces no Domain, implementa��o na Infrastructure

### Services
? L�gica de neg�cio na Application

### DTOs
? Separa��o entre modelos de dom�nio e API

### Dependency Injection
? Todas as depend�ncias injetadas

### Aggregate Roots
? Empresa como raiz do agregado (Usu�rio pertence a Empresa)

---

## ?? Boas Pr�ticas Seguidas

1. ? C�digo limpo e leg�vel
2. ? SOLID principles
3. ? DRY (Don't Repeat Yourself)
4. ? Separation of Concerns
5. ? Dependency Inversion
6. ? Interface Segregation
7. ? Single Responsibility
8. ? Testabilidade
9. ? Documenta��o
10. ? Versionamento (Git ready)

---

## ?? Para Estudar e Entender

### Iniciante
1. Estrutura de pastas e camadas
2. Controllers e endpoints
3. DTOs e mapeamento
4. Autentica��o JWT b�sica

### Intermedi�rio
1. Padr�o Repository
2. Dependency Injection
3. AutoMapper
4. Dapper vs Entity Framework
5. Docker Compose

### Avan�ado
1. DDD completo
2. CQRS e Event Sourcing
3. Microservices
4. Kubernetes
5. CI/CD pipelines

---

Este projeto serve como base s�lida para evolu��o e aprendizado cont�nuo de arquitetura de software em .NET! ??

# Estrutura do Projeto - Benefícios API

## Estrutura de Diretórios Completa

```
C:\Projetos\dotnet\Beneficios\
?
??? ?? beneficios-api.sln                      # Solution principal
??? ?? .gitignore                              # Arquivos ignorados pelo Git
??? ?? README.md                               # Documentção principal
??? ?? ENDPOINTS.md                            # Documentção de endpoints
??? ?? EXAMPLES.md                             # Exemplos de uso
??? ?? TESTING.md                              # Guia de testes
??? ?? DEPLOY.md                               # Guia de deploy
??? ?? docker-compose.yml                      # Configurção Docker
?
??? ?? src/
?   ?
?   ??? ?? Beneficios.Api/                     # Camada de Apresentação
?   ?   ??? ?? Controllers/
?   ?   ?   ??? UsuariosController.cs          # Controller de usuários (POST, PUT, GET, DELETE, LOGIN)
?   ?   ?   ??? EmpresasController.cs          # Controller de empresas (POST, PUT, GET, DELETE)
?   ?   ?
?   ?   ??? ?? Properties/
?   ?   ?   ??? launchSettings.json            # Configurções de execução
?   ?   ?
?   ?   ??? Program.cs                         # Configurção da aplicção
?   ?   ??? appsettings.json                   # Configurções de produção
?   ?   ??? appsettings.Development.json       # Configurções de desenvolvimento
?   ?   ??? Dockerfile                         # Imagem Docker da API
?   ?   ??? docker-compose.override.yml        # Sobrescritas Docker
?   ?   ??? Beneficios.Api.csproj             # Projeto da API
?   ?
?   ??? ?? Beneficios.Application/             # Camada de Aplicção
?   ?   ??? ?? Services/
?   ?   ?   ??? UsuarioService.cs              # Lógica de negócio de usuários
?   ?   ?   ??? EmpresaService.cs              # Lógica de negócio de empresas
?   ?   ?   ??? TokenService.cs                # Gerção e validção de JWT
?   ?   ?
?   ?   ??? ?? DTOs/
?   ?   ?   ??? UsuarioCreateDto.cs            # DTO para criar usuário
?   ?   ?   ??? UsuarioUpdateDto.cs            # DTO para atualizar usuário
?   ?   ?   ??? UsuarioDto.cs                  # DTO de retorno de usuário
?   ?   ?   ??? LoginDto.cs                    # DTO de login
?   ?   ?   ??? LoginResponseDto.cs            # DTO de resposta de login
?   ?   ?   ??? EmpresaCreateDto.cs            # DTO para criar empresa
?   ?   ?   ??? EmpresaUpdateDto.cs            # DTO para atualizar empresa
?   ?   ?   ??? EmpresaDto.cs                  # DTO de retorno de empresa
?   ?   ?
?   ?   ??? ?? Interfaces/
?   ?   ?   ??? IUsuarioService.cs             # Interface do serviço de usuários
?   ?   ?   ??? IEmpresaService.cs             # Interface do serviço de empresas
?   ?   ?   ??? ITokenService.cs               # Interface do serviço de token
?   ?   ?
?   ?   ??? ?? Mappings/
?   ?   ?   ??? MappingProfile.cs              # Perfis do AutoMapper
?   ?   ?
?   ?   ??? Beneficios.Application.csproj      # Projeto da Application
?   ?
?   ??? ?? Beneficios.Domain/                  # Camada de Domínio
?   ?   ??? ?? Entities/
?   ?   ?   ??? Usuario.cs                     # Entidade de usuário
?   ?   ?   ??? Empresa.cs                     # Entidade de empresa
?   ?   ?
?   ?   ??? ?? Interfaces/
?   ?   ?   ??? IUsuarioRepository.cs          # Interface do repositório de usuários
?   ?   ?   ??? IEmpresaRepository.cs          # Interface do repositório de empresas
?   ?   ?
?   ?   ??? ?? ValueObjects/
?   ?   ?   ??? Criptografia.cs                # Objeto de valor para criptografia
?   ?   ?
?   ?   ??? Beneficios.Domain.csproj           # Projeto do Domain
?   ?
?   ??? ?? Beneficios.Infrastructure/          # Camada de Infraestrutura
?       ??? ?? Repositories/
?       ?   ??? UsuarioRepository.cs           # Implementção do repositório de usuários (Dapper)
?       ?   ??? EmpresaRepository.cs           # Implementção do repositório de empresas (Dapper)
?       ?
?       ??? ?? Configurations/
?       ?   ??? DatabaseConfiguration.cs       # Configurção de conexão com banco
?       ?
?       ??? ?? Scripts/
?       ?   ??? 00_Init_Database.sql           # Script de inicializção completa
?       ?   ??? 01_Create_Table_Empresas.sql   # Script de crição da tabela empresas
?       ?   ??? 02_Create_Table_Usuarios.sql   # Script de crição da tabela usuarios
?       ?   ??? 03_Insert_Sample_Data.sql      # Script com dados de exemplo
?       ?
?       ??? Beneficios.Infrastructure.csproj   # Projeto da Infrastructure
?
??? ?? tests/
    ??? ?? Beneficios.Tests/                   # Projeto de Testes
        ??? ?? Domain/
        ?   ??? UsuarioTests.cs                # Testes da entidade usuário
        ?   ??? EmpresaTests.cs                # Testes da entidade empresa
        ?   ??? CriptografiaTests.cs           # Testes de criptografia
        ?
        ??? ?? Application/
        ?   ??? UsuarioServiceTests.cs         # Testes do serviço de usuários
        ?   ??? EmpresaServiceTests.cs         # Testes do serviço de empresas
        ?   ??? TokenServiceTests.cs           # Testes do serviço de token
        ?
        ??? ?? Api/
        ?   ??? UsuariosControllerTests.cs     # Testes da controller de usuários
        ?   ??? EmpresasControllerTests.cs     # Testes da controller de empresas
        ?
        ??? Beneficios.Tests.csproj            # Projeto de testes
```

---

## Pacotes NuGet Utilizados

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

## Estrutura do Banco de Dados

### Tabela: empresas

| Coluna               | Tipo          | Descrição                          |
|----------------------|---------------|------------------------------------|
| id                   | UUID          | Chave primária                     |
| razao_social         | VARCHAR(50)   | Razão social da empresa            |
| dominio              | VARCHAR(20)   | Domínio/slug da empresa            |
| nome_banco           | VARCHAR(256)  | Nome do banco (criptografado)      |
| usuario_banco        | VARCHAR(256)  | Usuário do banco (criptografado)   |
| senha_banco          | VARCHAR(256)  | Senha do banco (criptografada)     |
| data_inclusao        | TIMESTAMP     | Data de crição                    |
| data_alteracao       | TIMESTAMP     | Data da última alterção (nullable)|
| usuario_alteracao_id | UUID          | ID do usuário que alterou (nullable)|

### Tabela: usuarios

| Coluna               | Tipo          | Descrição                          |
|----------------------|---------------|------------------------------------|
| id                   | UUID          | Chave primária                     |
| nome                 | VARCHAR(50)   | Nome do usuário                    |
| senha                | VARCHAR(256)  | Senha (criptografada)              |
| email                | VARCHAR(256)  | Email (único)                      |
| empresa_id           | UUID          | FK para empresas                   |
| token                | TEXT          | Token JWT atual                    |
| data_inclusao        | TIMESTAMP     | Data de crição                    |
| data_alteracao       | TIMESTAMP     | Data da última alterção (nullable)|
| usuario_alteracao_id | UUID          | ID do usuário que alterou (nullable)|

**Relacionamentos:**
- `usuarios.empresa_id` ? `empresas.id` (FK com CASCADE)

---

## Arquitetura DDD

### Domain (Núcleo)
- **Entities**: Objetos com identidade única (Usuario, Empresa)
- **Value Objects**: Objetos sem identidade (Criptografia)
- **Interfaces**: Contratos dos repositórios

### Application (Casos de Uso)
- **Services**: Orquestrção da lógica de negócio
- **DTOs**: Objetos de transferência de dados
- **Interfaces**: Contratos dos serviços
- **Mappings**: Mapeamento entre entidades e DTOs

### Infrastructure (Detalhes Técnicos)
- **Repositories**: Implementção de acesso a dados com Dapper
- **Configurations**: Configurções de banco de dados
- **Scripts**: Scripts SQL de crição de tabelas

### API (Interface)
- **Controllers**: Endpoints HTTP
- **Program.cs**: Configurção de DI, middleware, etc.

---

## Fluxo de Requisção

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

## Segurança Implementada

1. **Autenticção JWT**
   - Token gerado no login
   - Expirção de 8 horas
   - Validção em todos os endpoints (exceto login)

2. **Criptografia**
   - Senhas de usuários (Base64)
   - Dados de conexão de banco (Base64)
   - ?? **Nota**: Para produção, usar bcrypt/Argon2

3. **CORS**
   - Configurável no Program.cs

4. **HTTPS**
   - Recomendado para produção

---

## Observabilidade

### Logs (Serilog)
- Console
- Arquivo: `logs/beneficios-YYYYMMDD.txt`
- Rotation: Diária

### Health Check
- Endpoint: `/health`
- Usado pelo Docker

### Métricas
- Pronto para integrção com Prometheus/Grafana

---

## Docker

### Containers
1. **postgres**: PostgreSQL 16 Alpine
2. **api**: .NET 9 API

### Volumes
- `postgres_data`: Persistência do banco de dados

### Networks
- `beneficios-network`: Comunicção entre containers

### Portas
- PostgreSQL: 5432
- API: 8080 (HTTP), 8081 (HTTPS)

---

## Checklist de Funcionalidades

### Implementado ?

- [x] Estrutura DDD enxuta
- [x] CRUD completo de Usuários
- [x] CRUD completo de Empresas
- [x] Autenticção JWT
- [x] Logs com Serilog
- [x] Health Check
- [x] AutoMapper
- [x] Dapper para performance
- [x] PostgreSQL
- [x] Docker e Docker Compose
- [x] Testes unitários (Domain, Application, API)
- [x] Swagger/OpenAPI
- [x] Scripts SQL
- [x] Documentção completa

### Sugestões para Evolução ??

- [ ] Refresh Token
- [ ] Paginção nas listagens
- [ ] Filtros e ordenção
- [ ] Validções com FluentValidation
- [ ] Cache com Redis
- [ ] Rate limiting
- [ ] CQRS com MediatR
- [ ] Event sourcing
- [ ] Notificções (SignalR)
- [ ] Multi-tenancy
- [ ] Auditoria completa
- [ ] Soft delete
- [ ] Versionamento de API
- [ ] Internacionalizção (i18n)
- [ ] Testes de integrção
- [ ] Performance tests

---

## Documentção Disponível

1. **README.md** - Visão geral e quick start
2. **ENDPOINTS.md** - Documentção completa de todos os endpoints
3. **EXAMPLES.md** - Exemplos práticos de uso
4. **TESTING.md** - Guia de testes
5. **DEPLOY.md** - Guia de deploy (Docker, Azure, K8s, VM)
6. **Swagger** - Documentção interativa (http://localhost:5000/swagger)

---

## Conceitos DDD Aplicados

### Camadas
? Separção clara de responsabilidades

### Entities
? Usuario e Empresa com identidade única (Guid)

### Value Objects
? Criptografia (comportamento sem identidade)

### Repositories
? Interfaces no Domain, implementção na Infrastructure

### Services
? Lógica de negócio na Application

### DTOs
? Separção entre modelos de domínio e API

### Dependency Injection
? Todas as dependências injetadas

### Aggregate Roots
? Empresa como raiz do agregado (Usuário pertence a Empresa)

---

## Boas Práticas Seguidas

1. ? Código limpo e legível
2. ? SOLID principles
3. ? DRY (Don't Repeat Yourself)
4. ? Separation of Concerns
5. ? Dependency Inversion
6. ? Interface Segregation
7. ? Single Responsibility
8. ? Testabilidade
9. ? Documentção
10. ? Versionamento (Git ready)

---

## Para Estudar e Entender

### Iniciante
1. Estrutura de pastas e camadas
2. Controllers e endpoints
3. DTOs e mapeamento
4. Autenticção JWT básica

### Intermediário
1. Padrão Repository
2. Dependency Injection
3. AutoMapper
4. Dapper vs Entity Framework
5. Docker Compose

### Avançado
1. DDD completo
2. CQRS e Event Sourcing
3. Microservices
4. Kubernetes
5. CI/CD pipelines

---

Este projeto serve como base sólida para evolução e aprendizado contínuo de arquitetura de software em .NET! ??

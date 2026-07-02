# Guia de Testes - Benef�cios API

## ?? Estrutura de Testes

Os testes est�o organizados em:

```
tests/Beneficios.Tests/
??? Domain/           # Testes de entidades e value objects
??? Application/      # Testes de servi�os
??? Api/              # Testes de controllers
```

## ?? Como Executar os Testes

### Via Visual Studio
1. Abra o **Test Explorer** (Ctrl + E, T)
2. Clique em "Run All" para executar todos os testes

### Via Visual Studio Code
1. Instale a extens�o ".NET Core Test Explorer"
2. Os testes aparecer�o na aba de testes
3. Clique em "Run All Tests"

### Via CLI

**Executar todos os testes:**
```bash
dotnet test
```

**Executar com mais detalhes:**
```bash
dotnet test --logger "console;verbosity=detailed"
```

**Executar testes espec�ficos:**
```bash
# Apenas testes do Domain
dotnet test --filter "FullyQualifiedName~Beneficios.Tests.Domain"

# Apenas testes do Application
dotnet test --filter "FullyQualifiedName~Beneficios.Tests.Application"

# Apenas testes da API
dotnet test --filter "FullyQualifiedName~Beneficios.Tests.Api"
```

**Executar com cobertura de c�digo:**
```bash
dotnet test /p:CollectCoverage=true
```

## ?? Testes Implementados

### Domain (Dom�nio)

#### UsuarioTests
- ? Cria��o de usu�rio com propriedades corretas
- ? Valida��o de email
- ? Preenchimento de data de inclus�o

#### EmpresaTests
- ? Cria��o de empresa com propriedades corretas
- ? Valida��o de dom�nio
- ? Preenchimento de data de inclus�o

#### CriptografiaTests
- ? Criptografia de texto
- ? Descriptografia de texto
- ? Reversibilidade (encrypt/decrypt)

### Application (Aplica��o)

#### UsuarioServiceTests
- ? Criar usu�rio com sucesso
- ? Atualizar usu�rio com sucesso
- ? Deletar usu�rio com sucesso

#### EmpresaServiceTests
- ? Criar empresa com sucesso
- ? Atualizar empresa com sucesso
- ? Deletar empresa com sucesso

#### TokenServiceTests
- ? Gerar token v�lido
- ? Validar token correto
- ? Retornar null para token inv�lido
- ? Retornar null para token vazio

### API (Controllers)

#### UsuariosControllerTests
- ? Retornar lista de usu�rios
- ? Buscar usu�rio por ID
- ? Retornar 404 quando usu�rio n�o existe
- ? Deletar usu�rio com sucesso
- ? Retornar 404 ao deletar usu�rio inexistente

#### EmpresasControllerTests
- ? Retornar lista de empresas
- ? Buscar empresa por ID
- ? Retornar 404 quando empresa n�o existe
- ? Deletar empresa com sucesso
- ? Retornar 404 ao deletar empresa inexistente

## ?? Exemplo de Sa�da

```
Test run for C:\Projetos\dotnet\Beneficios\tests\Beneficios.Tests\bin\Debug\net9.0\Beneficios.Tests.dll (.NETCoreApp,Version=v9.0)
Microsoft (R) Test Execution Command Line Tool Version 17.x.x

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    19, Skipped:     0, Total:    19, Duration: 500 ms
```

## ?? Adicionar Novos Testes

### Estrutura b�sica de um teste

```csharp
using Xunit;

namespace Beneficios.Tests.Domain;

public class MinhaClasseTests
{
    [Fact]
    public void MeuMetodo_DeveRetornarValorEsperado()
    {
        // Arrange (Preparar)
        var objeto = new MinhaClasse();

        // Act (Agir)
        var resultado = objeto.MeuMetodo();

        // Assert (Verificar)
        Assert.Equal(valorEsperado, resultado);
    }
}
```

### Com Moq (Mock de depend�ncias)

```csharp
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class MeuServiceTests
{
    private readonly Mock<IRepository> _repositoryMock;
    private readonly MeuService _service;

    public MeuServiceTests()
    {
        _repositoryMock = new Mock<IRepository>();
        _service = new MeuService(_repositoryMock.Object);
    }

    [Fact]
    public async Task Metodo_DeveFazerAlgo()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Entidade());

        // Act
        var resultado = await _service.Metodo();

        // Assert
        Assert.NotNull(resultado);
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
```

## ?? Bibliotecas Utilizadas

- **xUnit**: Framework de testes
- **Moq**: Biblioteca para criar mocks
- **FluentAssertions**: Asser��es mais leg�veis (opcional)

## ?? Boas Pr�ticas

1. **Nomenclatura**: Use `MetodoTestado_CondicaoTestada_ResultadoEsperado`
2. **AAA Pattern**: Arrange, Act, Assert
3. **Um assert por teste**: Cada teste deve verificar apenas uma coisa
4. **Testes independentes**: N�o devem depender da ordem de execu��o
5. **Mock apenas depend�ncias externas**: Banco de dados, APIs externas, etc.

## ?? Cobertura de C�digo

Para gerar relat�rio de cobertura de c�digo:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

Para visualizar em HTML (requer ReportGenerator):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport
```

## ? Meta de Cobertura

- **Domain**: > 90%
- **Application**: > 80%
- **API**: > 70%

## ?? Debug de Testes

### Visual Studio
1. Coloque um breakpoint no teste
2. Clique com bot�o direito no teste
3. Selecione "Debug Test"

### VS Code
1. Configure o launch.json para testes
2. Coloque breakpoints
3. Execute em modo debug

## ?? Refer�ncias

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

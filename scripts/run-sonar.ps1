param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

function Import-SonarEnvFile {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return
    }

    Get-Content $Path | ForEach-Object {
        $line = $_.Trim()
        if ($line.Length -eq 0 -or $line.StartsWith("#")) {
            return
        }

        $parts = $line -split "=", 2
        if ($parts.Count -ne 2) {
            return
        }

        $name = $parts[0].Trim()
        $value = $parts[1].Trim()
        Set-Item -Path "env:$name" -Value $value
    }
}

Import-SonarEnvFile (Join-Path $repoRoot ".env.sonar.local")
Import-SonarEnvFile (Join-Path $repoRoot "Beneficios.Front\.env.sonar.local")

if (-not $env:SONAR_HOST_URL) {
    $env:SONAR_HOST_URL = "http://192.168.18.70:9000"
}

if (-not $env:SONAR_TOKEN) {
    throw @"
SONAR_TOKEN nao definido.

Opcoes:
  1) Crie .env.sonar.local na raiz do repo (copie de Beneficios.Front/.env.sonar.local.example)
  2) Ou defina a variavel de ambiente SONAR_TOKEN antes de executar este script
"@
}

$testProject = Join-Path $repoRoot "tests\Beneficios.Tests\Beneficios.Tests.csproj"
$solution = Join-Path $repoRoot "beneficios-api.sln"
$analysisStarted = $false
$hiddenSonarProps = @()

function Hide-SonarProjectProperties {
    foreach ($path in @(
            (Join-Path $repoRoot "sonar-project.properties"),
            (Join-Path $repoRoot "Beneficios.Front\sonar-project.properties")
        )) {
        if (Test-Path $path) {
            $backup = "$path.sonarscan-bak"
            Move-Item -Path $path -Destination $backup -Force
            $script:hiddenSonarProps += [pscustomobject]@{ Original = $path; Backup = $backup }
        }
    }
}

function Restore-SonarProjectProperties {
    foreach ($item in $hiddenSonarProps) {
        if (Test-Path $item.Backup) {
            Move-Item -Path $item.Backup -Destination $item.Original -Force
        }
    }
    $script:hiddenSonarProps = @()
}

Hide-SonarProjectProperties

Write-Host "Iniciando analise Sonar (beneficios-api) em $env:SONAR_HOST_URL ..."

$beginArgs = @(
    "sonarscanner", "begin",
    "/k:beneficios-api",
    "/n:Beneficios API",
    "/v:1.0",
    "/d:sonar.host.url=$env:SONAR_HOST_URL",
    "/d:sonar.token=$env:SONAR_TOKEN",
    "/d:sonar.sources=src",
    "/d:sonar.tests=tests",
    "/d:sonar.sourceEncoding=UTF-8",
    "/d:sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml",
    "/d:sonar.coverage.exclusions=**/Program.cs,**/*Dto.cs,**/*Params.cs,**/Migrations/**"
)

dotnet @beginArgs
if ($LASTEXITCODE -ne 0) {
    throw "Sonar begin falhou."
}

$analysisStarted = $true
try {
    Write-Host "Compilando solucao..."
    dotnet build $solution --no-incremental
    if ($LASTEXITCODE -ne 0) {
        throw "Build falhou."
    }

    if (-not $SkipTests) {
        Write-Host "Executando testes com cobertura OpenCover..."
        dotnet test $testProject `
            /p:CollectCoverage=true `
            /p:CoverletOutputFormat=opencover `
            /p:CoverletOutput=./coverage/ `
            --no-build
        if ($LASTEXITCODE -ne 0) {
            throw "Testes falharam."
        }
    }

    Write-Host "Finalizando analise Sonar..."
    dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
    if ($LASTEXITCODE -ne 0) {
        throw "Sonar end falhou."
    }

    Restore-SonarProjectProperties
}
finally {
    if ($analysisStarted -and $LASTEXITCODE -ne 0) {
        dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN" 2>$null | Out-Null
    }
    Restore-SonarProjectProperties
}

Write-Host "Analise Sonar concluida. Veja em $env:SONAR_HOST_URL/dashboard?id=beneficios-api"

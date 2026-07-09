param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$frontRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $frontRoot

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

Import-SonarEnvFile (Join-Path $frontRoot ".env.sonar.local")

if (-not $env:SONAR_HOST_URL) {
    $env:SONAR_HOST_URL = "http://192.168.18.70:9000"
}

if (-not $env:SONAR_TOKEN) {
    throw @"
SONAR_TOKEN nao definido.

Opcoes:
  1) Crie Beneficios.Front/.env.sonar.local a partir de .env.sonar.local.example
  2) Ou defina a variavel de ambiente SONAR_TOKEN antes de executar este script
"@
}

if (-not $SkipTests) {
    Write-Host "Executando testes com cobertura..."
    npm run test:coverage
    if ($LASTEXITCODE -ne 0) {
        throw "Testes com cobertura falharam."
    }
}

$lcovFile = Join-Path $frontRoot "coverage\beneficios.front\lcov.info"
if (-not (Test-Path $lcovFile)) {
    throw "Arquivo de cobertura nao encontrado: $lcovFile"
}

Write-Host "Enviando analise para $env:SONAR_HOST_URL (projeto beneficios-front)..."
npx --yes @sonar/scan

if ($LASTEXITCODE -ne 0) {
    throw "Analise Sonar falhou."
}

Write-Host "Analise Sonar concluida com sucesso."

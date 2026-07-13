param(
    [string]$ServerIp = "192.168.18.70",
    [string]$BaseDomain = "tresonze.servidor",
    [string]$BeneficiosDomain = "beneficios.tresonze.servidor"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$certsDir = Join-Path $root "docker\gateway\certs"

New-Item -ItemType Directory -Force -Path $certsDir | Out-Null

Write-Host "Gerando certificado autoassinado em $certsDir ..."
Write-Host "  $BaseDomain, *.$BaseDomain"
Write-Host "  $BeneficiosDomain, *.$BeneficiosDomain"

$subjectAltName = "subjectAltName=IP:${ServerIp},DNS:localhost,DNS:host.docker.internal,DNS:${BaseDomain},DNS:*.${BaseDomain},DNS:${BeneficiosDomain},DNS:*.${BeneficiosDomain}"

if (Get-Command docker -ErrorAction SilentlyContinue) {
    docker run --rm `
        -v "${certsDir}:/certs" `
        alpine/openssl req -x509 -nodes -days 365 -newkey rsa:2048 `
        -keyout /certs/server.key `
        -out /certs/server.crt `
        -subj "/CN=${BaseDomain}" `
        -addext $subjectAltName
}
elseif (Get-Command openssl -ErrorAction SilentlyContinue) {
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 `
        -keyout (Join-Path $certsDir "server.key") `
        -out (Join-Path $certsDir "server.crt") `
        -subj "/CN=${BaseDomain}" `
        -addext $subjectAltName
}
else {
    throw "Instale Docker ou OpenSSL para gerar os certificados em docker/gateway/certs."
}

Write-Host ""
Write-Host "Certificados criados:"
Write-Host "  docker/gateway/certs/server.crt"
Write-Host "  docker/gateway/certs/server.key"
Write-Host ""
Write-Host "Configure no DNS interno:"
Write-Host "  *.${BaseDomain}           A  ${ServerIp}"
Write-Host "  ${BeneficiosDomain}       A  ${ServerIp}"
Write-Host "  *.${BeneficiosDomain}     A  ${ServerIp}"
Write-Host ""
Write-Host "Registrar Ponto API:  https://registrar-ponto-api.${BaseDomain}/swagger"
Write-Host "Beneficios API:         https://beneficios-api.${BaseDomain}/swagger"
Write-Host "Beneficios admin:       https://${BeneficiosDomain}/"
Write-Host "Beneficios tenant:      https://exemplo.${BeneficiosDomain}/"
Write-Host ""
Write-Host "Subida (na ordem):"
Write-Host "  docker compose -f docker-compose.gateway.yml up -d"
Write-Host "  docker compose up -d --build"
Write-Host "  (em registrar-ponto) docker compose up -d --build"

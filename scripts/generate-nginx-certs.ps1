param(
    [string]$ServerIp = "192.168.18.70",
    [string]$BaseDomain = "beneficios.servidor"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$certsDir = Join-Path $root "docker\certs"

New-Item -ItemType Directory -Force -Path $certsDir | Out-Null

Write-Host "Gerando certificado autoassinado para $ServerIp / *.$BaseDomain em $certsDir ..."

$subjectAltName = "subjectAltName=IP:${ServerIp},DNS:localhost,DNS:host.docker.internal,DNS:${BaseDomain},DNS:*.${BaseDomain}"

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
    throw "Instale Docker ou OpenSSL para gerar os certificados em docker/certs."
}

Write-Host ""
Write-Host "Certificados criados:"
Write-Host "  docker/certs/server.crt"
Write-Host "  docker/certs/server.key"
Write-Host ""
Write-Host "Configure no hosts (ou DNS interno):"
Write-Host "  ${ServerIp}  admin.${BaseDomain}"
Write-Host "  ${ServerIp}  empresa1.${BaseDomain}"
Write-Host ""
Write-Host "Front HTTPS:  https://admin.${BaseDomain}:9443/"
Write-Host "Swagger:      https://admin.${BaseDomain}:9443/swagger/index.html"
Write-Host "HTTP (301):   http://${ServerIp}:9081/"

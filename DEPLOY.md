# Guia de Deploy - Benefícios API

## Opções de Deploy

### 1. Docker Compose (Recomendado para desenvolvimento)
### 2. Azure App Service
### 3. Kubernetes
### 4. VM/Servidor Linux

---

## Deploy com Docker Compose

### Pré-requisitos
- Docker
- Docker Compose

### Passos

1. **Clone o repositório**
```bash
git clone <url-do-repositorio>
cd beneficios-api
```

2. **Configure as variáveis de ambiente** (opcional)

   Edite `docker-compose.yml` ou crie um arquivo `.env`:
```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=sua_senha_segura
POSTGRES_DB=beneficios
JWT_SECRET_KEY=sua-chave-jwt-super-segura-com-pelo-menos-32-caracteres
```

3. **Build e execute**
```bash
docker-compose up -d --build
```

4. **Verifique os logs**
```bash
docker-compose logs -f api
```

5. **Acesse a API**
- Swagger: http://localhost:8080/swagger
- Health Check: http://localhost:8080/health

6. **Parar os containers**
```bash
docker-compose down
```

7. **Parar e remover volumes (dados)**
```bash
docker-compose down -v
```

---

## Deploy no Azure App Service

### Pré-requisitos
- Conta Azure
- Azure CLI instalado
- .NET 9 SDK

### Passos

1. **Login no Azure**
```bash
az login
```

2. **Criar Resource Group**
```bash
az group create --name beneficios-rg --location brazilsouth
```

3. **Criar Azure Database for PostgreSQL**
```bash
az postgres flexible-server create \
  --resource-group beneficios-rg \
  --name beneficios-db \
  --location brazilsouth \
  --admin-user adminuser \
  --admin-password SuaSenhaSegura123! \
  --sku-name Standard_B1ms \
  --version 14
```

4. **Configurar firewall do banco**
```bash
az postgres flexible-server firewall-rule create \
  --resource-group beneficios-rg \
  --name beneficios-db \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

5. **Criar App Service Plan**
```bash
az appservice plan create \
  --name beneficios-plan \
  --resource-group beneficios-rg \
  --sku B1 \
  --is-linux
```

6. **Criar Web App**
```bash
az webapp create \
  --resource-group beneficios-rg \
  --plan beneficios-plan \
  --name beneficios-api \
  --runtime "DOTNET:9.0"
```

7. **Configurar variáveis de ambiente**
```bash
az webapp config appsettings set \
  --resource-group beneficios-rg \
  --name beneficios-api \
  --settings \
    ConnectionStrings__DefaultConnection="Host=beneficios-db.postgres.database.azure.com;Database=beneficios;Username=adminuser;Password=SuaSenhaSegura123!" \
    JwtSettings__SecretKey="sua-chave-jwt-super-segura-com-pelo-menos-32-caracteres" \
    JwtSettings__Issuer="BeneficiosApi" \
    JwtSettings__Audience="BeneficiosClient"
```

8. **Deploy da aplicção**
```bash
cd src/Beneficios.Api
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
  --resource-group beneficios-rg \
  --name beneficios-api \
  --src ../app.zip
```

9. **Verificar deploy**
```bash
az webapp browse --resource-group beneficios-rg --name beneficios-api
```

---

## Deploy com Kubernetes

### Pré-requisitos
- Cluster Kubernetes
- kubectl configurado
- Imagem Docker publicada

### 1. Build e push da imagem

```bash
docker build -t seu-registro/beneficios-api:v1 -f src/Beneficios.Api/Dockerfile .
docker push seu-registro/beneficios-api:v1
```

### 2. Criar arquivos de configurção

**namespace.yaml**
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: beneficios
```

**configmap.yaml**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: beneficios-config
  namespace: beneficios
data:
  JwtSettings__Issuer: "BeneficiosApi"
  JwtSettings__Audience: "BeneficiosClient"
```

**secret.yaml**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: beneficios-secret
  namespace: beneficios
type: Opaque
stringData:
  ConnectionStrings__DefaultConnection: "Host=postgres;Database=beneficios;Username=postgres;Password=postgres"
  JwtSettings__SecretKey: "sua-chave-jwt-super-segura-com-pelo-menos-32-caracteres"
```

**postgres-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: beneficios
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16-alpine
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_DB
          value: beneficios
        - name: POSTGRES_USER
          value: postgres
        - name: POSTGRES_PASSWORD
          value: postgres
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: beneficios
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

**api-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: beneficios-api
  namespace: beneficios
spec:
  replicas: 3
  selector:
    matchLabels:
      app: beneficios-api
  template:
    metadata:
      labels:
        app: beneficios-api
    spec:
      containers:
      - name: api
        image: seu-registro/beneficios-api:v1
        ports:
        - containerPort: 8080
        envFrom:
        - configMapRef:
            name: beneficios-config
        - secretRef:
            name: beneficios-secret
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: beneficios-api
  namespace: beneficios
spec:
  type: LoadBalancer
  selector:
    app: beneficios-api
  ports:
  - port: 80
    targetPort: 8080
```

### 3. Deploy no Kubernetes

```bash
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f secret.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f api-deployment.yaml
```

### 4. Verificar deploy

```bash
kubectl get pods -n beneficios
kubectl get services -n beneficios
kubectl logs -f deployment/beneficios-api -n beneficios
```

---

## Deploy em VM/Servidor Linux

### Pré-requisitos
- Ubuntu 20.04+ ou Debian 11+
- Acesso SSH ao servidor

### Passos

1. **Conectar ao servidor**
```bash
ssh usuario@seu-servidor
```

2. **Instalar .NET 9**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version latest
```

3. **Instalar PostgreSQL**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

4. **Configurar banco de dados**
```bash
sudo -u postgres psql
CREATE DATABASE beneficios;
CREATE USER beneficios WITH PASSWORD 'senha_segura';
GRANT ALL PRIVILEGES ON DATABASE beneficios TO beneficios;
\q
```

5. **Clonar e publicar aplicção**
```bash
git clone <url-do-repositorio>
cd beneficios-api/src/Beneficios.Api
dotnet publish -c Release -o /var/www/beneficios-api
```

6. **Configurar appsettings**
```bash
sudo nano /var/www/beneficios-api/appsettings.json
```

7. **Criar serviço systemd**
```bash
sudo nano /etc/systemd/system/beneficios-api.service
```

```ini
[Unit]
Description=Beneficios API
After=network.target

[Service]
WorkingDirectory=/var/www/beneficios-api
ExecStart=/usr/bin/dotnet /var/www/beneficios-api/Beneficios.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=beneficios-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

8. **Iniciar serviço**
```bash
sudo systemctl daemon-reload
sudo systemctl enable beneficios-api
sudo systemctl start beneficios-api
sudo systemctl status beneficios-api
```

9. **Configurar Nginx como reverse proxy**
```bash
sudo apt install nginx
sudo nano /etc/nginx/sites-available/beneficios-api
```

```nginx
server {
    listen 80;
    server_name seu-dominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/beneficios-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

10. **Configurar SSL (Let's Encrypt)**
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d seu-dominio.com
```

---

## Checklist Pré-Deploy

- [ ] Variáveis de ambiente configuradas
- [ ] Banco de dados criado e acessível
- [ ] Scripts SQL executados
- [ ] Testes passando
- [ ] Logs configurados
- [ ] Health check funcionando
- [ ] JWT secret configurado (mínimo 32 caracteres)
- [ ] HTTPS configurado (produção)
- [ ] Backup do banco configurado
- [ ] Monitoramento configurado

---

## Segurança

### Checklist de Segurança

- [ ] Use HTTPS em produção
- [ ] Altere as senhas padrão
- [ ] Use senhas fortes para JWT
- [ ] Configure CORS adequadamente
- [ ] Limite rate limiting
- [ ] Configure firewall
- [ ] Mantenha dependências atualizadas
- [ ] Use bcrypt para senhas (não Base64)
- [ ] Configure logs mas não exponha dados sensíveis
- [ ] Use Azure Key Vault ou similar para secrets

---

## Monitoramento

### Application Insights (Azure)

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

No Program.cs:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Prometheus + Grafana

Adicione:
```bash
dotnet add package prometheus-net.AspNetCore
```

No Program.cs:
```csharp
app.UseHttpMetrics();
app.MapMetrics();
```

---

## CI/CD

### GitHub Actions

Crie `.github/workflows/deploy.yml`:

```yaml
name: Deploy

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2

    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 9.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Test
      run: dotnet test --no-restore --verbosity normal

    - name: Publish
      run: dotnet publish src/Beneficios.Api/Beneficios.Api.csproj -c Release -o ./publish

    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: beneficios-api
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

## Suporte

Em caso de problemas durante o deploy, verifique:

1. Logs da aplicção
2. Logs do servidor web (Nginx/IIS)
3. Conectividade com banco de dados
4. Variáveis de ambiente
5. Portas abertas no firewall

Para mais informções, consulte a documentção oficial do .NET e Azure.

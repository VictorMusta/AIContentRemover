# 🚀 GUIDE DE DÉPLOIEMENT EN PRODUCTION

## Table des matières
1. [Configuration PostgreSQL](#1-configuration-postgresql)
2. [Variables d'environnement](#2-variables-denvironnement)
3. [Migrations en production](#3-migrations-en-production)
4. [Build et publication](#4-build-et-publication)
5. [Déploiement Linux (Ubuntu/Debian)](#5-déploiement-linux)
6. [Déploiement Windows Server](#6-déploiement-windows-server)
7. [Docker (optionnel)](#7-docker)
8. [Nginx Reverse Proxy](#8-nginx-reverse-proxy)
9. [SSL/HTTPS avec Let's Encrypt](#9-ssl-https)
10. [Monitoring et logs](#10-monitoring)
11. [Sécurité en production](#11-sécurité)

---

## 1. Configuration PostgreSQL

### Installation PostgreSQL

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

**Windows Server:**
Télécharger depuis https://www.postgresql.org/download/windows/

### Créer la base de données

```bash
sudo -u postgres psql

CREATE DATABASE aicontentremover;
CREATE USER aicontentremover_user WITH ENCRYPTED PASSWORD 'VOTRE_MOT_DE_PASSE_SECURISE';
GRANT ALL PRIVILEGES ON DATABASE aicontentremover TO aicontentremover_user;
\q
```

### Connection String pour production

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=aicontentremover;Username=aicontentremover_user;Password=VOTRE_MOT_DE_PASSE_SECURISE;SSL Mode=Require"
  }
}
```

---

## 2. Variables d'environnement

### Créer appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "AIContentRemover": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=aicontentremover;Username=aicontentremover_user;Password=${DB_PASSWORD}"
  },
  
  "Security": {
    "RequireApiKey": true,
    "ApiKeys": ["${API_KEY_1}", "${API_KEY_2}"]
  },
  
  "AiContentThreshold": 5,
  
  "AllowedHosts": "api.votredomaine.com",
  
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

### Variables d'environnement système

**Linux:**
```bash
export ASPNETCORE_ENVIRONMENT=Production
export DB_PASSWORD="votre_mdp_postgres"
export API_KEY_1="votre_api_key_secure_1"
export API_KEY_2="votre_api_key_secure_2"
```

**Windows:**
```cmd
setx ASPNETCORE_ENVIRONMENT "Production"
setx DB_PASSWORD "votre_mdp_postgres"
```

---

## 3. Migrations en production

### Méthode 1 : Migrations automatiques (risqué)

Dans `Program.cs`, en production :
```csharp
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Appliquer automatiquement
}
```

### Méthode 2 : Générer un script SQL (RECOMMANDÉ)

```bash
# Sur votre machine de dev
dotnet ef migrations script --output migration.sql --idempotent

# Copier migration.sql sur le serveur
# Puis exécuter manuellement
psql -U aicontentremover_user -d aicontentremover -f migration.sql
```

### Méthode 3 : CLI sur le serveur

```bash
dotnet ef database update --connection "Host=localhost;Database=aicontentremover;Username=aicontentremover_user;Password=XXX"
```

---

## 4. Build et publication

### Build en mode Release

```bash
dotnet publish -c Release -o ./publish --self-contained false
```

Paramètres :
- `-c Release` : Optimisations activées
- `-o ./publish` : Dossier de sortie
- `--self-contained false` : Nécessite .NET Runtime installé sur le serveur

### Build self-contained (inclut le runtime)

```bash
# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux

# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/windows
```

---

## 5. Déploiement Linux (Ubuntu/Debian)

### Installer .NET Runtime 8.0

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --runtime aspnetcore
```

### Copier les fichiers

```bash
sudo mkdir -p /var/www/aicontentremover
sudo chown $USER:$USER /var/www/aicontentremover
scp -r ./publish/* user@serveur:/var/www/aicontentremover/
```

### Créer un service systemd

```bash
sudo nano /etc/systemd/system/aicontentremover.service
```

```ini
[Unit]
Description=AI Content Remover API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/aicontentremover
ExecStart=/usr/bin/dotnet /var/www/aicontentremover/AIContentRemover.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=aicontentremover
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

### Démarrer le service

```bash
sudo systemctl daemon-reload
sudo systemctl start aicontentremover
sudo systemctl enable aicontentremover
sudo systemctl status aicontentremover
```

### Voir les logs

```bash
sudo journalctl -u aicontentremover -f
```

---

## 6. Déploiement Windows Server

### Installer .NET 8.0 Runtime

Télécharger : https://dotnet.microsoft.com/download/dotnet/8.0

### Installer IIS

```powershell
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature -name Web-ASP-Net45
```

### Installer ASP.NET Core Module

Télécharger : https://dotnet.microsoft.com/permalink/dotnetcore-current-windows-runtime-bundle-installer

### Configurer IIS

1. Créer un nouveau site dans IIS Manager
2. Définir le chemin physique : `C:\inetpub\aicontentremover`
3. Binding : HTTP port 80 ou HTTPS port 443
4. Application Pool : "No Managed Code"

### web.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\AIContentRemover.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

---

## 7. Docker (optionnel)

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AIContentRemover/AIContentRemover.csproj", "AIContentRemover/"]
RUN dotnet restore "AIContentRemover/AIContentRemover.csproj"
COPY . .
WORKDIR "/src/AIContentRemover"
RUN dotnet build "AIContentRemover.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AIContentRemover.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIContentRemover.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgreSQL=Host=db;Database=aicontentremover;Username=postgres;Password=postgres123
    depends_on:
      - db
  
  db:
    image: postgres:16
    environment:
      - POSTGRES_DB=aicontentremover
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
    volumes:
      - pgdata:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  pgdata:
```

### Commandes Docker

```bash
# Build
docker-compose build

# Démarrer
docker-compose up -d

# Voir les logs
docker-compose logs -f api

# Arrêter
docker-compose down
```

---

## 8. Nginx Reverse Proxy

### Installation

```bash
sudo apt install nginx
```

### Configuration

```bash
sudo nano /etc/nginx/sites-available/aicontentremover
```

```nginx
server {
    listen 80;
    server_name api.votredomaine.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # CORS pour extensions navigateur
        add_header Access-Control-Allow-Origin "*";
        add_header Access-Control-Allow-Methods "GET, POST, OPTIONS";
        add_header Access-Control-Allow-Headers "Content-Type, X-API-Key";
        
        if ($request_method = 'OPTIONS') {
            return 204;
        }
    }
    
    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;
    limit_req zone=api_limit burst=20 nodelay;
}
```

### Activer le site

```bash
sudo ln -s /etc/nginx/sites-available/aicontentremover /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

---

## 9. SSL/HTTPS avec Let's Encrypt

### Installation Certbot

```bash
sudo apt install certbot python3-certbot-nginx
```

### Obtenir un certificat SSL

```bash
sudo certbot --nginx -d api.votredomaine.com
```

### Renouvellement automatique

```bash
sudo crontab -e

# Ajouter cette ligne
0 3 * * * certbot renew --quiet
```

---

## 10. Monitoring et logs

### Logs avec Serilog (optionnel)

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

Dans `Program.cs` :
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Monitoring avec Health Checks

Déjà disponible sur `/health`

### Uptime Monitoring

Utilisez :
- UptimeRobot (gratuit)
- Pingdom
- StatusCake

Configurez une alerte sur `https://api.votredomaine.com/health`

---

## 11. Sécurité en production

### ✅ Checklist sécurité

- [x] HTTPS obligatoire (SSL/TLS)
- [x] API Keys fortes et rotées régulièrement
- [x] Rate limiting activé
- [x] Pas de secrets dans le code source
- [x] Variables d'environnement pour les credentials
- [x] Firewall configuré (autoriser uniquement 80, 443)
- [x] PostgreSQL n'écoute pas sur l'Internet public
- [x] Logs activés et surveillés
- [x] Mises à jour régulières de .NET
- [x] Backups automatiques de la base de données

### Générer des API Keys sécurisées

```bash
openssl rand -base64 32
# Exemple : 8K7Lp3mNq5Rz2Wv9Xy4Bc6Df1Gh8Jk0M
```

### Firewall (UFW)

```bash
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 22/tcp
sudo ufw enable
```

### Backup PostgreSQL automatique

```bash
sudo nano /usr/local/bin/backup-db.sh
```

```bash
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -U aicontentremover_user aicontentremover > /backups/db_$DATE.sql
find /backups -name "db_*.sql" -mtime +7 -delete
```

```bash
sudo chmod +x /usr/local/bin/backup-db.sh
sudo crontab -e

# Backup quotidien à 2h du matin
0 2 * * * /usr/local/bin/backup-db.sh
```

---

## 🎯 Résumé des étapes

1. ✅ Installer PostgreSQL
2. ✅ Configurer la base de données
3. ✅ Build en mode Release
4. ✅ Copier les fichiers sur le serveur
5. ✅ Créer le service systemd (Linux) ou IIS (Windows)
6. ✅ Appliquer les migrations
7. ✅ Configurer Nginx en reverse proxy
8. ✅ Installer SSL avec Let's Encrypt
9. ✅ Activer le monitoring et les logs
10. ✅ Sécuriser avec firewall et API Keys

**Votre API est maintenant prête pour la production ! 🚀**


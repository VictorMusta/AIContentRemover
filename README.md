# 🤖 AI Content Remover - Backend API

Backend .NET 8 professionnel pour détecter et tagger les tweets contenant du contenu IA généré, destiné à être utilisé par une extension navigateur.

## 🎯 Fonctionnalités

- ✅ API REST complète pour gérer les tags IA sur les tweets
- ✅ Vote communautaire (upvote/downvote)
- ✅ Vérification unitaire et batch (jusqu'à 100 tweets à la fois)
- ✅ Statistiques globales
- ✅ Rate limiting intelligent (protection anti-spam)
- ✅ Support CORS pour extensions navigateur (Chrome, Firefox, Safari)
- ✅ API Key authentication (optionnelle)
- ✅ SQLite en développement, PostgreSQL en production
- ✅ Documentation Swagger complète
- ✅ Architecture propre et scalable

## 🏗️ Architecture

```
AIContentRemover/
├── Controllers/          # Endpoints API REST
│   └── TweetsController.cs
├── Models/              # Entités EF Core
│   ├── TweetTag.cs
│   └── Vote.cs
├── DTOs/                # Data Transfer Objects
│   └── ApiDTOs.cs
├── Services/            # Logique métier
│   └── TweetTagService.cs
├── Data/                # DbContext
│   └── ApplicationDbContext.cs
├── Middleware/          # Authentification API Key
│   └── ApiKeyMiddleware.cs
├── Validators/          # Validation FluentValidation
│   └── RequestValidators.cs
├── Migrations/          # Migrations EF Core
└── Program.cs           # Configuration
```

## 🚀 Démarrage rapide

### Prérequis

- .NET 8.0 SDK
- (Optionnel) PostgreSQL pour la production

### Installation

1. **Cloner et restaurer les packages**
```bash
cd AIContentRemover
dotnet restore
```

2. **Appliquer les migrations (automatique en dev)**
```bash
dotnet ef database update
```

3. **Lancer l'API**
```bash
dotnet run
```

L'API sera disponible sur :
- **HTTP** : `http://localhost:5000`
- **HTTPS** : `https://localhost:5001`
- **Swagger UI** : `https://localhost:5001` (page d'accueil en dev)

## 📚 Endpoints API

### 🔍 Vérifier un tweet

```http
GET /api/tweets/check/{tweetId}
```

**Exemple :**
```bash
curl https://localhost:5001/api/tweets/check/1234567890123456789
```

**Réponse :**
```json
{
  "tweetId": "1234567890123456789",
  "isAiContent": true,
  "aiVotes": 15,
  "notAiVotes": 2,
  "score": 13,
  "lastUpdated": "2025-11-17T10:30:00Z"
}
```

### 🔍 Vérifier plusieurs tweets (batch)

```http
POST /api/tweets/check/batch
Content-Type: application/json

{
  "tweetIds": [
    "1234567890123456789",
    "9876543210987654321"
  ]
}
```

**Exemple :**
```bash
curl -X POST https://localhost:5001/api/tweets/check/batch \
  -H "Content-Type: application/json" \
  -d '{"tweetIds":["1234567890123456789","9876543210987654321"]}'
```

### 🏷️ Tagger un tweet comme IA

```http
POST /api/tweets/tag
Content-Type: application/json

{
  "tweetId": "1234567890123456789",
  "userIdentifier": "uuid-de-votre-extension",
  "isAiVote": true
}
```

**Paramètres :**
- `isAiVote: true` → Vote "Contenu IA"
- `isAiVote: false` → Vote "Pas IA" (downvote)

**Exemple :**
```bash
curl -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d '{
    "tweetId": "1234567890123456789",
    "userIdentifier": "extension-uuid-12345",
    "isAiVote": true
  }'
```

### 📊 Statistiques globales

```http
GET /api/tweets/stats
```

**Réponse :**
```json
{
  "totalTweets": 1523,
  "totalVotes": 8947,
  "tweetsMarkedAsAI": 342,
  "lastUpdate": "2025-11-17T10:30:00Z"
}
```

### 🏆 Top tweets taggés IA

```http
GET /api/tweets/top?limit=50
```

### ❤️ Health check

```http
GET /health
```

## 🔐 Sécurité

### API Key (optionnelle)

Pour activer l'authentification par API Key, modifiez `appsettings.json` :

```json
{
  "Security": {
    "RequireApiKey": true,
    "ApiKeys": [
      "votre-api-key-secure-ici"
    ]
  }
}
```

Ensuite, incluez le header dans vos requêtes :

```bash
curl https://localhost:5001/api/tweets/check/123 \
  -H "X-API-Key: votre-api-key-secure-ici"
```

### Rate Limiting

Configuration par défaut dans `appsettings.json` :

- **POST /api/tweets/tag** : 10 requêtes/minute
- **Autres endpoints** : 100 requêtes/minute

## 🌐 Configuration CORS pour Extension Navigateur

Le backend est configuré pour accepter les requêtes depuis :
- Chrome Extensions (`chrome-extension://`)
- Firefox Extensions (`moz-extension://`)
- Safari Extensions (`safari-extension://`)
- Localhost (développement)

**Aucune configuration supplémentaire nécessaire !**

## 📖 Swagger Documentation

Accédez à la documentation interactive complète :

**Développement :**
- Swagger UI : `https://localhost:5001`
- JSON : `https://localhost:5001/swagger/v1/swagger.json`

## 🗄️ Base de données

### Développement (SQLite)

Par défaut, une base SQLite `aicontentremover.db` est créée automatiquement.

### Production (PostgreSQL)

Modifiez `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=aicontentremover;Username=postgres;Password=votre_mdp"
  }
}
```

Puis exécutez les migrations :

```bash
dotnet ef database update
```

## 📊 Schéma de base de données

### Table : TweetTags

| Colonne | Type | Description |
|---------|------|-------------|
| Id | BIGINT | Clé primaire |
| TweetId | VARCHAR(20) | ID unique du tweet (index unique) |
| AiVotes | INT | Nombre de votes "IA" |
| NotAiVotes | INT | Nombre de votes "Pas IA" |
| CreatedAt | DATETIME | Date de création |
| UpdatedAt | DATETIME | Dernière mise à jour |

### Table : Votes

| Colonne | Type | Description |
|---------|------|-------------|
| Id | BIGINT | Clé primaire |
| TweetTagId | BIGINT | FK vers TweetTags |
| UserIdentifier | VARCHAR(128) | UUID utilisateur (hash) |
| IsAiVote | BOOLEAN | Type de vote (true=IA, false=NotIA) |
| IpAddress | VARCHAR(45) | IP (anti-spam) |
| VotedAt | DATETIME | Date du vote |

**Index unique** sur `(TweetTagId, UserIdentifier)` → Un utilisateur = 1 vote par tweet

## 🔧 Configuration avancée

### Seuil de consensus IA

Modifiez le seuil dans `appsettings.json` :

```json
{
  "AiContentThreshold": 3
}
```

Un tweet est considéré comme "IA" si `Score >= 3` (où `Score = AiVotes - NotAiVotes`)

### Logs en production

Pour des logs détaillés, utilisez Serilog ou ajustez `appsettings.Production.json` :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "AIContentRemover": "Information"
    }
  }
}
```

## 🧪 Tests avec Swagger

1. Lancez l'API : `dotnet run`
2. Ouvrez `https://localhost:5001`
3. Testez les endpoints directement dans l'interface Swagger

## 📱 Exemple d'intégration dans une extension navigateur

### Manifest V3 (Chrome/Firefox)

```json
{
  "manifest_version": 3,
  "name": "AI Content Remover",
  "version": "1.0",
  "permissions": ["storage"],
  "host_permissions": ["https://localhost:5001/*"],
  "content_scripts": [{
    "matches": ["https://twitter.com/*", "https://x.com/*"],
    "js": ["content.js"]
  }]
}
```

### content.js

```javascript
const API_BASE = 'https://localhost:5001/api/tweets';

// Vérifier un tweet
async function checkTweet(tweetId) {
  const response = await fetch(`${API_BASE}/check/${tweetId}`);
  return await response.json();
}

// Batch check de tous les tweets visibles
async function checkVisibleTweets() {
  const tweetIds = extractTweetIds(); // Votre logique d'extraction
  
  const response = await fetch(`${API_BASE}/check/batch`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ tweetIds })
  });
  
  const data = await response.json();
  
  // Masquer les tweets taggés IA
  data.results.forEach(tweet => {
    if (tweet.isAiContent) {
      hideTweet(tweet.tweetId);
    }
  });
}

// Tagger un tweet
async function tagTweet(tweetId, isAi) {
  const userId = await getOrCreateUserId(); // UUID stocké localement
  
  await fetch(`${API_BASE}/tag`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      tweetId,
      userIdentifier: userId,
      isAiVote: isAi
    })
  });
}
```

## 🚨 Points d'attention pour la production

### ⚠️ HTTPS

En production, **utilisez toujours HTTPS** ! Configurez un certificat SSL valide.

### ⚠️ API Key

Activez l'authentification API Key et utilisez des clés fortes :

```bash
# Générer une clé sécurisée
openssl rand -base64 32
```

### ⚠️ Rate Limiting

Ajustez les limites selon votre trafic attendu.

### ⚠️ PostgreSQL

Utilisez PostgreSQL en production pour de meilleures performances.

### ⚠️ Migrations

Exécutez les migrations manuellement :

```bash
dotnet ef database update --connection "votre_connection_string"
```

### ⚠️ Reverse Proxy

Utilisez Nginx ou Caddy devant ASP.NET Core :

```nginx
server {
  listen 443 ssl http2;
  server_name api.aicontentremover.com;
  
  location / {
    proxy_pass http://localhost:5000;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
  }
}
```

## 🛠️ Commandes utiles

```bash
# Lancer en mode développement
dotnet run

# Lancer en production
dotnet run --environment Production

# Créer une migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Supprimer la dernière migration
dotnet ef migrations remove

# Générer un script SQL
dotnet ef migrations script

# Build en release
dotnet build -c Release

# Publier l'application
dotnet publish -c Release -o ./publish
```

## 📊 Évolutions futures

- [ ] Système de modération (admin panel)
- [ ] Webhooks pour notifier les changements
- [ ] Cache Redis pour les requêtes fréquentes
- [ ] Analytics avancés (dashboard)
- [ ] Machine Learning pour pré-taguer automatiquement
- [ ] Support multi-plateformes (YouTube, TikTok, etc.)
- [ ] API GraphQL en complément du REST

## 📄 Licence

MIT License - Libre d'utilisation

## 👤 Support

Pour toute question ou problème :
- Consultez la documentation Swagger
- Vérifiez les logs : `dotnet run --verbosity detailed`
- Ouvrez une issue sur le repository

---

**Fait avec ❤️ pour un web plus transparent**


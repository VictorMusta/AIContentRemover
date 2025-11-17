# ⚡ QUICK START - AI Content Remover

## 🚀 Démarrage rapide (2 minutes)

### 1. Restaurer les packages
```bash
cd AIContentRemover
dotnet restore
```

### 2. Lancer l'API
```bash
dotnet run
```

✅ L'API est disponible sur : **https://localhost:5001**  
✅ Swagger UI : **https://localhost:5001** (page d'accueil)

---

## 📋 Commandes essentielles

### Développement
```bash
# Lancer en mode dev
dotnet run

# Lancer avec hot-reload
dotnet watch run

# Build
dotnet build

# Nettoyer
dotnet clean
```

### Base de données
```bash
# Créer une migration
dotnet ef migrations add NomMigration

# Appliquer les migrations
dotnet ef database update

# Voir les migrations
dotnet ef migrations list

# Supprimer la dernière migration
dotnet ef migrations remove

# Générer un script SQL
dotnet ef migrations script
```

### Tests rapides
```bash
# Health check
curl -k https://localhost:5001/health

# Vérifier un tweet
curl -k https://localhost:5001/api/tweets/check/1234567890

# Tagger un tweet
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d '{"tweetId":"1234567890","userIdentifier":"test-user","isAiVote":true}'

# Stats
curl -k https://localhost:5001/api/tweets/stats
```

---

## 📁 Structure du projet

```
AIContentRemover/
├── Controllers/          # API REST endpoints
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
├── Middleware/          # Auth API Key
│   └── ApiKeyMiddleware.cs
├── Validators/          # FluentValidation
│   └── RequestValidators.cs
├── Migrations/          # EF Core migrations
├── Program.cs           # Configuration
└── appsettings.json     # Configuration
```

---

## 🔌 Endpoints disponibles

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/tweets/check/{tweetId}` | Vérifier un tweet |
| POST | `/api/tweets/check/batch` | Vérifier plusieurs tweets |
| POST | `/api/tweets/tag` | Tagger un tweet |
| GET | `/api/tweets/stats` | Statistiques globales |
| GET | `/api/tweets/top?limit=50` | Top tweets IA |
| GET | `/health` | Health check |

---

## ⚙️ Configuration rapide

### Changer le seuil IA

Dans `appsettings.json` :
```json
{
  "AiContentThreshold": 3
}
```

### Activer l'API Key

Dans `appsettings.json` :
```json
{
  "Security": {
    "RequireApiKey": true,
    "ApiKeys": ["votre-clé-ici"]
  }
}
```

### Changer de base de données

**SQLite (dev):**
```json
{
  "ConnectionStrings": {
    "SQLite": "Data Source=aicontentremover.db"
  }
}
```

**PostgreSQL (prod):**
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=aicontentremover;Username=user;Password=pass"
  }
}
```

---

## 🧪 Tests avec Swagger

1. Lancer : `dotnet run`
2. Ouvrir : `https://localhost:5001`
3. Cliquer sur un endpoint
4. Cliquer "Try it out"
5. Remplir les paramètres
6. Cliquer "Execute"

---

## 📊 Exemple d'utilisation

### Scénario complet

```bash
# 1. Vérifier un tweet (inexistant)
curl -k https://localhost:5001/api/tweets/check/123
# Résultat: 0 votes

# 2. 3 utilisateurs votent "IA"
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d '{"tweetId":"123","userIdentifier":"user1","isAiVote":true}'

curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d '{"tweetId":"123","userIdentifier":"user2","isAiVote":true}'

curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d '{"tweetId":"123","userIdentifier":"user3","isAiVote":true}'

# 3. Vérifier à nouveau
curl -k https://localhost:5001/api/tweets/check/123
# Résultat: isAiContent = true (score >= 3)
```

---

## 🔐 Sécurité

### Rate Limiting par défaut
- **POST /tag** : 10 requêtes/minute
- **Autres** : 100 requêtes/minute

### CORS configuré pour
- Chrome Extensions (`chrome-extension://`)
- Firefox Extensions (`moz-extension://`)
- Localhost (dev)

---

## 🛠️ Troubleshooting

### Port déjà utilisé
Modifier `launchSettings.json` ou utiliser :
```bash
dotnet run --urls "https://localhost:5002"
```

### Erreur de migration
```bash
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Erreur de certificat SSL
Faire confiance au certificat dev :
```bash
dotnet dev-certs https --trust
```

### Voir les logs détaillés
```bash
dotnet run --verbosity detailed
```

---

## 📚 Documentation complète

- **README.md** : Documentation générale
- **TEST-GUIDE.md** : Guide de test complet
- **PRODUCTION-DEPLOY.md** : Déploiement production
- **extension-example.js** : Exemple d'intégration extension

---

## 🚀 Prochaines étapes

1. ✅ Lancer l'API : `dotnet run`
2. ✅ Tester avec Swagger : `https://localhost:5001`
3. ✅ Créer votre extension navigateur
4. ✅ Déployer en production

---

**Fait avec ❤️ pour un web plus transparent**


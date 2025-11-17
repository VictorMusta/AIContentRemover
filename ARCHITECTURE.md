# 🏗️ ARCHITECTURE DÉTAILLÉE

## Vue d'ensemble

Ce backend suit une architecture en couches propre et scalable :

```
┌─────────────────────────────────────────────┐
│          Extension Navigateur               │
│         (Chrome, Firefox, Safari)           │
└─────────────────┬───────────────────────────┘
                  │ HTTPS + CORS
                  ▼
┌─────────────────────────────────────────────┐
│              Controllers                     │
│         (TweetsController.cs)               │
│   - Validation des requêtes                 │
│   - Gestion des erreurs HTTP                │
│   - Documentation Swagger                   │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│             Middleware Layer                │
│   - API Key Authentication                  │
│   - Rate Limiting (AspNetCoreRateLimit)     │
│   - CORS Policy                             │
│   - Exception Handling                      │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│            Services (Business Logic)        │
│         (TweetTagService.cs)                │
│   - Logique de vote                         │
│   - Calcul des scores                       │
│   - Gestion du seuil                        │
│   - Optimisations (batch)                   │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│         Data Access Layer (EF Core)         │
│       (ApplicationDbContext.cs)             │
│   - Mapping ORM                             │
│   - Migrations                              │
│   - Index et performances                   │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│            Base de données                  │
│   SQLite (dev) / PostgreSQL (prod)          │
│   - TweetTags                               │
│   - Votes                                   │
└─────────────────────────────────────────────┘
```

---

## 📦 Composants détaillés

### 1. Controllers Layer

**Responsabilités :**
- Exposer les endpoints REST
- Valider les requêtes entrantes (FluentValidation)
- Retourner les réponses HTTP standardisées
- Documenter l'API (Swagger)

**Fichiers :**
- `Controllers/TweetsController.cs`

**Endpoints :**
- `GET /api/tweets/check/{tweetId}` → Vérifier un tweet
- `POST /api/tweets/check/batch` → Vérifier plusieurs tweets
- `POST /api/tweets/tag` → Voter pour un tweet
- `GET /api/tweets/stats` → Statistiques globales
- `GET /api/tweets/top` → Top tweets IA

### 2. Services Layer

**Responsabilités :**
- Logique métier pure
- Calcul des scores (AiVotes - NotAiVotes)
- Gestion des seuils (isAiContent si score >= threshold)
- Optimisations (batch queries, caching potentiel)

**Fichiers :**
- `Services/TweetTagService.cs`
- Interface : `ITweetTagService`

**Méthodes clés :**
```csharp
Task<TweetCheckResponse?> CheckTweetAsync(string tweetId)
Task<BatchCheckResponse> CheckTweetsBatchAsync(List<string> tweetIds)
Task<TagTweetResponse> TagTweetAsync(string tweetId, string userIdentifier, bool isAiVote, string? ipAddress)
Task<StatsResponse> GetStatsAsync()
Task<List<TweetCheckResponse>> GetTopAiTweetsAsync(int limit)
```

### 3. Data Access Layer (EF Core)

**Responsabilités :**
- Mapping objet-relationnel
- Gestion des migrations
- Configuration des index
- Relations entre entités

**Fichiers :**
- `Data/ApplicationDbContext.cs`
- `Models/TweetTag.cs`
- `Models/Vote.cs`

**Configuration importante :**
- Index unique sur `TweetTags.TweetId` (performance + intégrité)
- Index composite sur `Votes(TweetTagId, UserIdentifier)` (évite les votes multiples)
- Cascade delete : si un TweetTag est supprimé, ses Votes aussi

### 4. DTOs (Data Transfer Objects)

**Responsabilités :**
- Séparer les modèles internes des API publiques
- Contrôler exactement ce qui est exposé
- Faciliter la documentation Swagger

**Fichiers :**
- `DTOs/ApiDTOs.cs`

**DTOs principaux :**
- `TweetCheckResponse` : Réponse de vérification
- `TagTweetRequest` : Requête de vote
- `TagTweetResponse` : Réponse de vote
- `BatchCheckRequest/Response` : Requêtes batch
- `StatsResponse` : Statistiques
- `ErrorResponse` : Erreurs standardisées

### 5. Validators

**Responsabilités :**
- Validation des données entrantes
- Règles métier (ex: TweetId doit être numérique)
- Messages d'erreur explicites

**Fichiers :**
- `Validators/RequestValidators.cs`

**Règles :**
- TweetId : obligatoire, max 20 caractères, uniquement chiffres
- UserIdentifier : obligatoire, max 128 caractères
- Batch : max 100 tweets par requête

### 6. Middleware

**Responsabilités :**
- Authentification (API Key optionnelle)
- Protection contre les abus
- CORS pour extensions navigateur

**Fichiers :**
- `Middleware/ApiKeyMiddleware.cs`

**Configuration CORS :**
```csharp
policy.WithOrigins(
    "chrome-extension://*",
    "moz-extension://*",
    "safari-extension://*"
)
```

---

## 🗄️ Schéma de base de données

### Table: TweetTags

```sql
CREATE TABLE TweetTags (
    Id              BIGINT PRIMARY KEY AUTO_INCREMENT,
    TweetId         VARCHAR(20) NOT NULL UNIQUE,
    AiVotes         INT NOT NULL DEFAULT 0,
    NotAiVotes      INT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX IX_TweetTags_TweetId (TweetId),
    INDEX IX_TweetTags_Votes (AiVotes, NotAiVotes),
    INDEX IX_TweetTags_UpdatedAt (UpdatedAt)
);
```

**Colonnes calculées (non persistées) :**
- `Score` = AiVotes - NotAiVotes
- `IsAiContent` = Score >= Threshold (défaut: 3)

### Table: Votes

```sql
CREATE TABLE Votes (
    Id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    TweetTagId          BIGINT NOT NULL,
    UserIdentifier      VARCHAR(128) NOT NULL,
    IsAiVote            BOOLEAN NOT NULL,
    IpAddress           VARCHAR(45) NULL,
    VotedAt             DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (TweetTagId) REFERENCES TweetTags(Id) ON DELETE CASCADE,
    UNIQUE INDEX IX_Votes_TweetTag_User (TweetTagId, UserIdentifier),
    INDEX IX_Votes_IpAddress (IpAddress)
);
```

**Contraintes :**
- Un utilisateur ne peut voter qu'une seule fois par tweet
- Mais il peut changer son vote (UPDATE au lieu d'INSERT)

---

## 🔄 Flux de données

### Scénario 1 : Vérifier un tweet

```
Extension → GET /api/tweets/check/123
              ↓
         TweetsController.CheckTweet()
              ↓
         TweetTagService.CheckTweetAsync()
              ↓
         DbContext.TweetTags.FirstOrDefaultAsync(t => t.TweetId == "123")
              ↓
         Calcul du score et isAiContent
              ↓
         Retour TweetCheckResponse
```

### Scénario 2 : Tagger un tweet (premier vote)

```
Extension → POST /api/tweets/tag
            { tweetId: "123", userIdentifier: "uuid", isAiVote: true }
              ↓
         Validation (FluentValidation)
              ↓
         TweetTagService.TagTweetAsync()
              ↓
         Vérifier si TweetTag existe
              ↓ (NON)
         Créer nouveau TweetTag
              ↓
         Créer nouveau Vote
              ↓
         Incrémenter AiVotes (0 → 1)
              ↓
         SaveChangesAsync()
              ↓
         Retour TagTweetResponse avec nouveau score
```

### Scénario 3 : Changer de vote

```
Extension → POST /api/tweets/tag
            { tweetId: "123", userIdentifier: "uuid", isAiVote: false }
              ↓
         TweetTagService.TagTweetAsync()
              ↓
         Vérifier si vote existe pour (TweetTagId, UserIdentifier)
              ↓ (OUI)
         Comparer ancien vote vs nouveau
              ↓
         Mettre à jour Vote.IsAiVote
              ↓
         Ajuster compteurs (AiVotes -1, NotAiVotes +1)
              ↓
         SaveChangesAsync()
              ↓
         Retour TagTweetResponse avec score mis à jour
```

### Scénario 4 : Batch check (optimisé)

```
Extension → POST /api/tweets/check/batch
            { tweetIds: ["123", "456", "789"] }
              ↓
         TweetTagService.CheckTweetsBatchAsync()
              ↓
         DbContext.TweetTags.Where(t => tweetIds.Contains(t.TweetId))
         (UNE SEULE REQUÊTE SQL avec WHERE IN)
              ↓
         Créer dictionnaire pour lookup rapide
              ↓
         Pour chaque tweetId, retourner résultat (ou 0 votes si absent)
              ↓
         Retour BatchCheckResponse
```

---

## ⚡ Optimisations

### 1. Batch queries
Au lieu de 100 requêtes SQL séparées, une seule requête avec `WHERE IN`.

### 2. Index stratégiques
- Index unique sur TweetId → Recherche O(log n)
- Index composite sur (TweetTagId, UserIdentifier) → Détecter vote existant en O(log n)
- Index sur IpAddress → Rate limiting par IP

### 3. AsNoTracking()
Pour les lectures sans modification, désactiver le change tracking EF Core.

### 4. Colonnes calculées non persistées
`Score` et `IsAiContent` calculés à la volée, pas stockés en DB.

### 5. Rate Limiting
Protection contre les abus (10 votes/min sur /tag, 100 req/min globalement).

---

## 🔐 Sécurité

### Couche 1 : CORS
- Autorise uniquement les origines légitimes
- Extensions navigateur + localhost

### Couche 2 : Rate Limiting
- Limite les requêtes par IP
- Protection DDoS

### Couche 3 : API Key (optionnelle)
- Authentification par header `X-API-Key`
- Désactivable en développement

### Couche 4 : Validation
- FluentValidation sur toutes les entrées
- Prévention injection SQL (EF Core paramètres)

### Couche 5 : HTTPS
- Certificat SSL en production
- Let's Encrypt gratuit

### Couche 6 : IP Logging
- Stockage de l'IP pour anti-spam
- Conformité RGPD : IP anonymisée possible

---

## 📈 Scalabilité

### Étape 1 : Monolithe (actuel)
- Backend unique
- SQLite (dev) / PostgreSQL (prod)
- Jusqu'à ~10k requêtes/jour

### Étape 2 : Caching
- Redis pour cache des tweets fréquents
- Réduction de 80% des requêtes DB

### Étape 3 : Load Balancing
- Plusieurs instances backend
- Nginx en reverse proxy
- Session sticky non nécessaire (stateless)

### Étape 4 : Database scaling
- PostgreSQL avec réplication read-only
- Séparation lecture/écriture
- Connection pooling

### Étape 5 : Microservices (futur)
- Service de vote séparé
- Service de statistiques séparé
- Message queue (RabbitMQ) pour événements

---

## 🧪 Testabilité

### Unit Tests
Service layer complètement testable :
```csharp
[Fact]
public async Task TagTweet_FirstVote_ShouldIncrementAiVotes()
{
    // Arrange
    var dbContext = CreateInMemoryDbContext();
    var service = new TweetTagService(dbContext, logger, config);
    
    // Act
    var result = await service.TagTweetAsync("123", "user1", true, null);
    
    // Assert
    Assert.True(result.Success);
    Assert.Equal(1, result.TweetData.AiVotes);
}
```

### Integration Tests
Tester les endpoints avec WebApplicationFactory :
```csharp
var client = _factory.CreateClient();
var response = await client.GetAsync("/api/tweets/check/123");
response.EnsureSuccessStatusCode();
```

---

## 📊 Métriques importantes

### Performance
- GET /check/{id} : < 50ms (avec index)
- POST /check/batch (100 tweets) : < 200ms
- POST /tag : < 100ms

### Base de données
- 1 TweetTag ≈ 100 bytes
- 1 Vote ≈ 200 bytes
- 1M tweets ≈ 100 MB
- 10M votes ≈ 2 GB

### Évolutivité
- PostgreSQL : jusqu'à 1 TB+ sans problème
- EF Core : pool de connexions optimisé
- Rate limiting : 100 req/min/IP = ~144k req/jour/IP

---

## 🎯 Prochaines évolutions possibles

1. **Cache distribué (Redis)**
   - Cache des tweets les plus consultés
   - TTL de 5 minutes

2. **WebSockets / SignalR**
   - Notifications temps réel des nouveaux tags
   - Mise à jour live des scores

3. **Machine Learning**
   - Pré-tag automatique des tweets
   - Détection de patterns IA

4. **Modération**
   - Dashboard admin
   - Système de reports
   - Bannissement d'utilisateurs abusifs

5. **Analytics**
   - Dashboard public avec stats
   - Graphiques d'évolution
   - Top contributeurs

6. **Multi-plateforme**
   - Support YouTube, TikTok, Instagram
   - API unifiée

---

**Cette architecture est prête pour scale de 0 à 1M utilisateurs ! 🚀**


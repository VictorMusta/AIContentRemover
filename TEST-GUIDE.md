# 🧪 GUIDE DE TEST - AI Content Remover API

## Préparation

1. **Lancer l'API**
```bash
cd AIContentRemover
dotnet run
```

L'API sera disponible sur `https://localhost:5001`

2. **Ouvrir Swagger UI**

Accédez à : `https://localhost:5001` dans votre navigateur

---

## 📝 Scénarios de test complets

### Scénario 1 : Vérifier un tweet qui n'existe pas encore

**Endpoint:** `GET /api/tweets/check/{tweetId}`

**Test:**
```bash
curl -k https://localhost:5001/api/tweets/check/1234567890123456789
```

**Résultat attendu:**
```json
{
  "tweetId": "1234567890123456789",
  "isAiContent": false,
  "aiVotes": 0,
  "notAiVotes": 0,
  "score": 0,
  "lastUpdated": null
}
```

✅ Le tweet n'existe pas encore dans la base, donc 0 votes.

---

### Scénario 2 : Tagger un tweet comme IA (premier vote)

**Endpoint:** `POST /api/tweets/tag`

**Test:**
```bash
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d "{\"tweetId\":\"1234567890123456789\",\"userIdentifier\":\"user-uuid-001\",\"isAiVote\":true}"
```

**Résultat attendu:**
```json
{
  "success": true,
  "message": "Vote enregistré",
  "tweetData": {
    "tweetId": "1234567890123456789",
    "isAiContent": false,
    "aiVotes": 1,
    "notAiVotes": 0,
    "score": 1,
    "lastUpdated": "2025-11-17T10:30:00Z"
  }
}
```

✅ Le tweet a maintenant 1 vote IA, mais `isAiContent = false` car le score (1) < seuil (3).

---

### Scénario 3 : Ajouter plus de votes pour atteindre le seuil

**Test avec différents utilisateurs:**

```bash
# User 2
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d "{\"tweetId\":\"1234567890123456789\",\"userIdentifier\":\"user-uuid-002\",\"isAiVote\":true}"

# User 3
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d "{\"tweetId\":\"1234567890123456789\",\"userIdentifier\":\"user-uuid-003\",\"isAiVote\":true}"
```

**Vérifier maintenant:**
```bash
curl -k https://localhost:5001/api/tweets/check/1234567890123456789
```

**Résultat attendu:**
```json
{
  "tweetId": "1234567890123456789",
  "isAiContent": true,
  "aiVotes": 3,
  "notAiVotes": 0,
  "score": 3,
  "lastUpdated": "2025-11-17T10:32:00Z"
}
```

✅ **isAiContent = true** car Score (3) >= Seuil (3)

---

### Scénario 4 : Un utilisateur change d'avis

**Test (user-uuid-001 vote maintenant "Not IA"):**

```bash
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d "{\"tweetId\":\"1234567890123456789\",\"userIdentifier\":\"user-uuid-001\",\"isAiVote\":false}"
```

**Résultat attendu:**
```json
{
  "success": true,
  "message": "Vote mis à jour",
  "tweetData": {
    "tweetId": "1234567890123456789",
    "isAiContent": false,
    "aiVotes": 2,
    "notAiVotes": 1,
    "score": 1,
    "lastUpdated": "2025-11-17T10:35:00Z"
  }
}
```

✅ Le vote a été mis à jour (aiVotes: 3→2, notAiVotes: 0→1)
✅ Le tweet n'est plus considéré comme IA car Score (1) < Seuil (3)

---

### Scénario 5 : Batch check de plusieurs tweets

**Test:**
```bash
curl -k -X POST https://localhost:5001/api/tweets/check/batch \
  -H "Content-Type: application/json" \
  -d "{\"tweetIds\":[\"1234567890123456789\",\"9999999999999999999\",\"8888888888888888888\"]}"
```

**Résultat attendu:**
```json
{
  "results": [
    {
      "tweetId": "1234567890123456789",
      "isAiContent": false,
      "aiVotes": 2,
      "notAiVotes": 1,
      "score": 1,
      "lastUpdated": "2025-11-17T10:35:00Z"
    },
    {
      "tweetId": "9999999999999999999",
      "isAiContent": false,
      "aiVotes": 0,
      "notAiVotes": 0,
      "score": 0,
      "lastUpdated": null
    },
    {
      "tweetId": "8888888888888888888",
      "isAiContent": false,
      "aiVotes": 0,
      "notAiVotes": 0,
      "score": 0,
      "lastUpdated": null
    }
  ]
}
```

✅ Optimisation : 1 seule requête pour 3 tweets

---

### Scénario 6 : Statistiques globales

**Test:**
```bash
curl -k https://localhost:5001/api/tweets/stats
```

**Résultat attendu:**
```json
{
  "totalTweets": 1,
  "totalVotes": 3,
  "tweetsMarkedAsAI": 0,
  "lastUpdate": "2025-11-17T10:35:00Z"
}
```

---

### Scénario 7 : Top tweets taggés IA

**Test:**
```bash
curl -k https://localhost:5001/api/tweets/top?limit=10
```

**Résultat attendu:**
```json
[
  {
    "tweetId": "1234567890123456789",
    "isAiContent": false,
    "aiVotes": 2,
    "notAiVotes": 1,
    "score": 1,
    "lastUpdated": "2025-11-17T10:35:00Z"
  }
]
```

---

### Scénario 8 : Health check

**Test:**
```bash
curl -k https://localhost:5001/health
```

**Résultat attendu:**
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-17T10:40:00Z",
  "environment": "Development",
  "version": "1.0.0"
}
```

---

## 🔐 Tests avec API Key (optionnel)

**Activer dans appsettings.json:**
```json
{
  "Security": {
    "RequireApiKey": true,
    "ApiKeys": ["test-key-12345"]
  }
}
```

**Test avec API Key:**
```bash
curl -k https://localhost:5001/api/tweets/check/123 \
  -H "X-API-Key: test-key-12345"
```

**Test sans API Key (doit échouer):**
```bash
curl -k https://localhost:5001/api/tweets/check/123
# Réponse: 401 Unauthorized
```

---

## 🚨 Tests de validation

### Test 1 : TweetId invalide (non numérique)

```bash
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d "{\"tweetId\":\"abc123\",\"userIdentifier\":\"user-001\",\"isAiVote\":true}"
```

**Résultat attendu:**
```json
{
  "error": "Validation échouée",
  "details": "TweetId doit contenir uniquement des chiffres",
  "timestamp": "2025-11-17T10:45:00Z"
}
```

### Test 2 : TweetId trop long

```bash
curl -k -X POST https://localhost:5001/api/tweets/tag \
  -H "Content-Type: application/json" \
  -d "{\"tweetId\":\"123456789012345678901234567890\",\"userIdentifier\":\"user-001\",\"isAiVote\":true}"
```

**Résultat attendu:** Erreur de validation

### Test 3 : Batch avec plus de 100 tweets

```bash
# Créer un array de 150 tweetIds
curl -k -X POST https://localhost:5001/api/tweets/check/batch \
  -H "Content-Type: application/json" \
  -d "{\"tweetIds\":[...150 IDs...]}"
```

**Résultat attendu:** Erreur "Maximum 100 tweets par requête batch"

---

## ⏱️ Tests de Rate Limiting

### Test : Spam de votes

```bash
# Faire plus de 10 requêtes POST /tag en 1 minute
for i in {1..15}; do
  curl -k -X POST https://localhost:5001/api/tweets/tag \
    -H "Content-Type: application/json" \
    -d "{\"tweetId\":\"$i\",\"userIdentifier\":\"user-spam\",\"isAiVote\":true}"
  echo ""
done
```

**Résultat attendu:** À partir de la 11ème requête → HTTP 429 (Too Many Requests)

---

## 🗄️ Vérifier la base de données

### Avec SQLite Browser

1. Télécharger [DB Browser for SQLite](https://sqlitebrowser.org/)
2. Ouvrir `aicontentremover.db`
3. Consulter les tables `TweetTags` et `Votes`

### Avec EF Core CLI

```bash
# Voir les migrations appliquées
dotnet ef migrations list

# Générer un script SQL
dotnet ef migrations script
```

---

## 📊 Tests de performance

### Test : Batch check de 100 tweets

```bash
time curl -k -X POST https://localhost:5001/api/tweets/check/batch \
  -H "Content-Type: application/json" \
  -d @100tweets.json
```

**Objectif:** < 200ms pour 100 tweets

---

## 🧪 Tests avec Swagger UI

1. Ouvrir `https://localhost:5001`
2. Cliquer sur **"Try it out"** pour chaque endpoint
3. Remplir les paramètres
4. Cliquer sur **"Execute"**
5. Vérifier la réponse

**Avantages de Swagger:**
- Interface visuelle
- Validation automatique
- Documentation intégrée
- Pas besoin de curl

---

## ✅ Checklist de validation complète

- [ ] Health check fonctionne
- [ ] Check d'un tweet inexistant retourne 0 votes
- [ ] Premier vote crée le tweet dans la DB
- [ ] Vote multiples incrémentent correctement
- [ ] Changement de vote met à jour les compteurs
- [ ] Seuil de 3 votes marque le tweet comme IA
- [ ] Batch check fonctionne pour 50+ tweets
- [ ] Validation rejette les TweetIds invalides
- [ ] Rate limiting bloque après 10 votes/min
- [ ] Stats globales sont correctes
- [ ] Top tweets retourne les plus votés
- [ ] API Key (si activée) bloque les requêtes non autorisées
- [ ] CORS autorise les extensions navigateur
- [ ] Migrations EF Core sont appliquées
- [ ] Logs apparaissent dans la console

---

## 🐛 Debugging

### Voir les logs détaillés

```bash
dotnet run --verbosity detailed
```

### Voir les requêtes SQL (EF Core)

Modifier `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Tester sans HTTPS (dev uniquement)

Modifier `Program.cs` : Commenter `app.UseHttpsRedirection();`

---

## 🎯 Résultat attendu final

✅ Tous les endpoints fonctionnent  
✅ La base de données SQLite est créée automatiquement  
✅ Les votes sont persistés correctement  
✅ Le rate limiting protège l'API  
✅ Swagger documente toute l'API  
✅ CORS est configuré pour les extensions  
✅ Ready pour intégration dans l'extension navigateur ! 🚀


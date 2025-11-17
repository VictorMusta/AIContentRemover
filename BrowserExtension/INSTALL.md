# ⚡ INSTALLATION RAPIDE - 5 MINUTES

## Extension AI Content Remover pour Chrome, Edge, Brave

---

## 📋 PRÉREQUIS

✅ Backend API lancé (voir dossier `AIContentRemover`)
```bash
cd AIContentRemover
dotnet run
```
➜ API sur https://localhost:5001

✅ Navigateur : Chrome, Edge ou Brave

---

## 🎨 ÉTAPE 1 : CRÉER LES ICÔNES (2 minutes)

### Option rapide : Télécharger des placeholders

1. Ouvrir ces liens dans votre navigateur :
   - https://dummyimage.com/128x128/ef4444/ffffff.png&text=AI
   - https://dummyimage.com/48x48/ef4444/ffffff.png&text=AI
   - https://dummyimage.com/16x16/ef4444/ffffff.png&text=AI

2. **Clic-droit** → **Enregistrer sous** sur chaque image

3. Renommer :
   - Première image → `icon128.png`
   - Deuxième image → `icon48.png`
   - Troisième image → `icon16.png`

4. Copier dans :
   ```
   BrowserExtension\icons\
   ```

**Ou mieux :** Utiliser https://favicon.io/favicon-generator/ pour de vraies icônes

---

## 🔧 ÉTAPE 2 : CONFIGURER L'URL DE L'API (30 secondes)

Éditer `BrowserExtension\config.js` :

```javascript
const API_CONFIG = {
  baseUrl: 'https://localhost:5001/api/tweets', // ✅ URL correcte
  apiKey: '', // Laisser vide en développement
  // ...
};
```

**⚠️ Important :** 
- En dev : `https://localhost:5001/api/tweets`
- En prod : `https://api.votredomaine.com/api/tweets`

---

## 🚀 ÉTAPE 3 : INSTALLER L'EXTENSION (1 minute)

### Sur Chrome / Brave

1. **Ouvrir les extensions**
   ```
   chrome://extensions/
   ```
   ou
   ```
   brave://extensions/
   ```

2. **Activer "Mode développeur"**
   - Toggle en haut à droite

3. **Charger l'extension**
   - Cliquer "Charger l'extension non empaquetée"
   - Sélectionner le dossier `BrowserExtension`
   - ✅ **Extension installée !**

### Sur Microsoft Edge

1. **Ouvrir les extensions**
   ```
   edge://extensions/
   ```

2. **Activer "Mode développeur"**
   - Toggle en bas à gauche

3. **Charger l'extension**
   - Cliquer "Charger l'extension décompressée"
   - Sélectionner le dossier `BrowserExtension`
   - ✅ **Extension installée !**

---

## ✅ ÉTAPE 4 : VÉRIFIER QUE ÇA FONCTIONNE (1 minute)

### Test 1 : Extension chargée

1. Aller sur https://twitter.com ou https://x.com
2. Appuyer sur **F12** (ouvrir Console)
3. Vous devriez voir :
   ```
   🚀 AI Content Remover - Extension initialisée
   🔍 Scan de X tweets...
   ```

✅ **L'extension est active !**

### Test 2 : Popup

1. Cliquer sur l'**icône de l'extension** dans la barre du navigateur
2. Vous devriez voir un popup avec :
   - Statistiques globales
   - État de connexion à l'API
   - Configuration

✅ **Le popup fonctionne !**

### Test 3 : Connexion API

Dans le popup :
- Si vous voyez "✅ API connectée" → **Parfait !**
- Si vous voyez "❌ API déconnectée" → Vérifier que le backend est lancé

---

## 🎯 UTILISATION

### Tagger un tweet comme IA

1. **Survoler n'importe quel tweet**
2. **Chercher le bouton "🤖 IA"** (en bas du tweet)
3. **Cliquer dessus**
4. Le bouton affiche "✅" → Vote enregistré !

### Voir un tweet masqué

1. **Chercher un tweet avec overlay rouge**
2. Il affiche : "🤖 Contenu IA détecté par la communauté"
3. **Cliquer "Afficher quand même"** pour le voir
4. Ou **cliquer "Pas IA"** pour downvoter

### Voir les stats

1. **Cliquer sur l'icône extension**
2. Popup affiche :
   - Nombre total de tweets taggés
   - Nombre de votes
   - Tweets masqués en session
3. **Cliquer "🔄 Rafraîchir"** pour mettre à jour

---

## 🐛 PROBLÈMES FRÉQUENTS

### ❌ "Extension ne se charge pas"

**Problème :** Icônes manquantes

**Solution :** Vérifier que ces 3 fichiers existent :
```
BrowserExtension\icons\icon16.png
BrowserExtension\icons\icon48.png
BrowserExtension\icons\icon128.png
```

---

### ❌ "API déconnectée" dans le popup

**Problème :** Backend pas lancé ou mauvaise URL

**Solution :**
1. Vérifier que `dotnet run` est lancé
2. Ouvrir https://localhost:5001/health dans le navigateur
3. Si erreur SSL, éditer `config.js` :
   ```javascript
   baseUrl: 'http://localhost:5000/api/tweets' // HTTP au lieu de HTTPS
   ```

---

### ❌ "Failed to fetch" dans Console

**Problème :** CORS ou API pas accessible

**Solution :**
1. Vérifier le backend :
   ```csharp
   app.UseCors("DevelopmentPolicy"); // Dans Program.cs
   ```
2. Recharger Twitter (F5)

---

### ❌ Aucun bouton "🤖 IA" visible

**Problème :** Sélecteurs Twitter ont changé

**Solution :**
1. Ouvrir Console (F12)
2. Vérifier qu'il n'y a pas d'erreurs
3. Si problème persiste, signaler sur GitHub

---

### ❌ "Manifest version 3 is not available"

**Problème :** Navigateur trop ancien

**Solution :** Mettre à jour Chrome/Edge/Brave vers la dernière version

---

## 📊 CHECKLIST FINALE

- [ ] Backend lancé (`dotnet run`)
- [ ] API accessible sur https://localhost:5001
- [ ] 3 icônes créées dans `icons/`
- [ ] URL configurée dans `config.js`
- [ ] Extension chargée dans le navigateur
- [ ] Console affiche "Extension initialisée"
- [ ] Popup affiche les stats
- [ ] Boutons "🤖 IA" visibles sur Twitter

✅ **Tout fonctionne ? Bravo !**

---

## 🎓 PROCHAINES ÉTAPES

1. ✅ Tester sur plusieurs tweets
2. ✅ Inviter d'autres personnes à voter
3. ✅ Voir les tweets se masquer automatiquement
4. ⏳ Déployer le backend en production
5. ⏳ Publier l'extension sur Chrome Web Store

---

## 📚 DOCUMENTATION COMPLÈTE

- **Backend** : `README.md` dans le dossier racine
- **Extension** : `BrowserExtension/README.md`
- **Tests** : `TEST-GUIDE.md`
- **Production** : `PRODUCTION-DEPLOY.md`
- **Guide extension** : `EXTENSION-GUIDE.md`

---

**Besoin d'aide ?** Consultez `EXTENSION-GUIDE.md` pour le guide complet !

**🚀 Bon test !**


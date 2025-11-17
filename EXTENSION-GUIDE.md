# 🌐 GUIDE COMPLET - CRÉER L'EXTENSION NAVIGATEUR

## AI Content Remover Extension pour Edge, Chrome et Brave

---

## 📋 TABLE DES MATIÈRES

1. [Vue d'ensemble](#vue-densemble)
2. [Structure du projet](#structure-du-projet)
3. [Création pas à pas](#création-pas-à-pas)
4. [Installation dans le navigateur](#installation-dans-le-navigateur)
5. [Tests](#tests)
6. [Publication](#publication)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 VUE D'ENSEMBLE

### Compatibilité

✅ **Edge, Chrome et Brave** utilisent tous **Chromium** → **1 seule extension** fonctionne sur les 3 !

### Ce que l'extension va faire

1. ✅ Détecte les tweets visibles sur Twitter/X
2. ✅ Interroge l'API backend en batch (optimisé)
3. ✅ Masque automatiquement les tweets taggés "IA"
4. ✅ Ajoute un bouton "Signaler comme IA" sur chaque tweet
5. ✅ Permet de voter/downvoter
6. ✅ Affiche les statistiques

---

## 📁 STRUCTURE DU PROJET

Créez un nouveau dossier `AIContentRemoverExtension` avec cette structure :

```
AIContentRemoverExtension/
├── manifest.json          ← Configuration de l'extension
├── content.js            ← Script injecté dans Twitter/X
├── background.js         ← Service worker (arrière-plan)
├── popup.html            ← Interface popup (clic sur icône)
├── popup.js              ← Logique du popup
├── popup.css             ← Styles du popup
├── config.js             ← Configuration API
└── icons/                ← Icônes de l'extension
    ├── icon16.png
    ├── icon48.png
    └── icon128.png
```

---

## 🛠️ CRÉATION PAS À PAS

### Étape 1 : Créer le dossier

```bash
mkdir AIContentRemoverExtension
cd AIContentRemoverExtension
mkdir icons
```

### Étape 2 : Créer manifest.json

Créez le fichier `manifest.json` :

```json
{
  "manifest_version": 3,
  "name": "AI Content Remover",
  "version": "1.0.0",
  "description": "Détecte et masque automatiquement les tweets taggés comme contenu IA par la communauté",
  
  "permissions": [
    "storage"
  ],
  
  "host_permissions": [
    "https://localhost:5001/*",
    "https://api.votredomaine.com/*",
    "https://twitter.com/*",
    "https://x.com/*"
  ],
  
  "background": {
    "service_worker": "background.js"
  },
  
  "content_scripts": [
    {
      "matches": [
        "https://twitter.com/*",
        "https://x.com/*"
      ],
      "js": ["config.js", "content.js"],
      "run_at": "document_idle"
    }
  ],
  
  "action": {
    "default_popup": "popup.html",
    "default_icon": {
      "16": "icons/icon16.png",
      "48": "icons/icon48.png",
      "128": "icons/icon128.png"
    }
  },
  
  "icons": {
    "16": "icons/icon16.png",
    "48": "icons/icon48.png",
    "128": "icons/icon128.png"
  },
  
  "web_accessible_resources": [
    {
      "resources": ["icons/*.png"],
      "matches": ["https://twitter.com/*", "https://x.com/*"]
    }
  ]
}
```

### Étape 3 : Créer config.js

Créez le fichier `config.js` :

```javascript
// Configuration globale de l'API
const API_CONFIG = {
  baseUrl: 'https://localhost:5001/api/tweets',
  apiKey: '', // Laisser vide si RequireApiKey = false
  batchSize: 50,
  checkInterval: 3000, // 3 secondes
  threshold: 3 // Seuil pour considérer un tweet comme IA
};
```

### Étape 4 : Créer content.js

Créez le fichier `content.js` (copiez le contenu depuis `extension-example.js`)

### Étape 5 : Créer background.js

Créez le fichier `background.js` :

```javascript
// Service Worker - Arrière-plan de l'extension

console.log('🤖 AI Content Remover - Background service démarré');

// Écouter l'installation
chrome.runtime.onInstalled.addListener((details) => {
  if (details.reason === 'install') {
    console.log('✅ Extension installée !');
    
    // Ouvrir la page d'accueil (optionnel)
    chrome.tabs.create({
      url: 'https://github.com/votre-repo/ai-content-remover'
    });
  } else if (details.reason === 'update') {
    console.log('🔄 Extension mise à jour !');
  }
});

// Gérer les messages depuis content.js
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.action === 'getStats') {
    // Récupérer les stats depuis l'API
    fetch(`${API_CONFIG.baseUrl}/stats`)
      .then(res => res.json())
      .then(data => sendResponse({ success: true, data }))
      .catch(error => sendResponse({ success: false, error: error.message }));
    
    return true; // Permet la réponse asynchrone
  }
});

// Badge pour afficher le nombre de tweets bloqués
let blockedCount = 0;

chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.action === 'incrementBlocked') {
    blockedCount++;
    chrome.action.setBadgeText({ text: blockedCount.toString() });
    chrome.action.setBadgeBackgroundColor({ color: '#ef4444' });
  } else if (request.action === 'resetBlocked') {
    blockedCount = 0;
    chrome.action.setBadgeText({ text: '' });
  }
});
```

### Étape 6 : Créer popup.html

Créez le fichier `popup.html` :

```html
<!DOCTYPE html>
<html lang="fr">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>AI Content Remover</title>
  <link rel="stylesheet" href="popup.css">
</head>
<body>
  <div class="container">
    <div class="header">
      <span class="icon">🤖</span>
      <h1>AI Content Remover</h1>
    </div>
    
    <div class="stats">
      <h2>Statistiques globales</h2>
      <div class="stat-item">
        <span class="stat-label">Tweets taggés IA</span>
        <span class="stat-value" id="totalTweets">-</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">Votes total</span>
        <span class="stat-value" id="totalVotes">-</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">Contenus IA détectés</span>
        <span class="stat-value" id="aiTweets">-</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">Session actuelle</span>
        <span class="stat-value" id="sessionBlocked">0</span>
      </div>
    </div>
    
    <div class="actions">
      <button id="refreshBtn" class="btn btn-primary">🔄 Rafraîchir</button>
      <button id="resetBtn" class="btn btn-secondary">↺ Reset session</button>
    </div>
    
    <div class="settings">
      <h3>Configuration</h3>
      <label>
        <input type="checkbox" id="autoHide" checked>
        Masquer automatiquement les tweets IA
      </label>
      <label>
        <input type="checkbox" id="showButtons" checked>
        Afficher les boutons de vote
      </label>
    </div>
    
    <div class="footer">
      <a href="https://github.com/votre-repo" target="_blank">Documentation</a>
      <span class="version">v1.0.0</span>
    </div>
  </div>
  
  <script src="config.js"></script>
  <script src="popup.js"></script>
</body>
</html>
```

### Étape 7 : Créer popup.css

Créez le fichier `popup.css` :

```css
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  width: 350px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #333;
}

.container {
  background: white;
  border-radius: 12px;
  margin: 16px;
  padding: 20px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
}

.header {
  text-align: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 2px solid #f3f4f6;
}

.icon {
  font-size: 48px;
  display: block;
  margin-bottom: 10px;
}

h1 {
  font-size: 20px;
  color: #1f2937;
}

.stats {
  margin: 20px 0;
}

.stats h2 {
  font-size: 14px;
  color: #6b7280;
  margin-bottom: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  margin: 8px 0;
  background: #f9fafb;
  border-radius: 8px;
  transition: background 0.2s;
}

.stat-item:hover {
  background: #f3f4f6;
}

.stat-label {
  font-size: 14px;
  color: #6b7280;
}

.stat-value {
  font-size: 16px;
  font-weight: 700;
  color: #ef4444;
}

.actions {
  display: flex;
  gap: 10px;
  margin: 20px 0;
}

.btn {
  flex: 1;
  padding: 10px;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 14px;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover {
  background: #2563eb;
  transform: translateY(-1px);
}

.btn-secondary {
  background: #f3f4f6;
  color: #6b7280;
}

.btn-secondary:hover {
  background: #e5e7eb;
}

.settings {
  margin: 20px 0;
  padding: 15px;
  background: #fef2f2;
  border-radius: 8px;
  border-left: 4px solid #ef4444;
}

.settings h3 {
  font-size: 14px;
  margin-bottom: 12px;
  color: #991b1b;
}

.settings label {
  display: flex;
  align-items: center;
  margin: 8px 0;
  cursor: pointer;
  font-size: 13px;
  color: #6b7280;
}

.settings input[type="checkbox"] {
  margin-right: 8px;
  cursor: pointer;
}

.footer {
  margin-top: 20px;
  padding-top: 15px;
  border-top: 2px solid #f3f4f6;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.footer a {
  color: #3b82f6;
  text-decoration: none;
  font-size: 13px;
}

.footer a:hover {
  text-decoration: underline;
}

.version {
  font-size: 11px;
  color: #9ca3af;
}

/* Animation de chargement */
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.loading {
  animation: pulse 1.5s ease-in-out infinite;
}
```

### Étape 8 : Créer popup.js

Créez le fichier `popup.js` :

```javascript
// Popup de l'extension - Interface utilisateur

// Charger les statistiques
async function loadStats() {
  try {
    const response = await fetch(`${API_CONFIG.baseUrl}/stats`);
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    
    const stats = await response.json();
    
    // Mettre à jour l'interface
    document.getElementById('totalTweets').textContent = stats.totalTweets.toLocaleString();
    document.getElementById('totalVotes').textContent = stats.totalVotes.toLocaleString();
    document.getElementById('aiTweets').textContent = stats.tweetsMarkedAsAI.toLocaleString();
    
  } catch (error) {
    console.error('Erreur lors du chargement des stats:', error);
    document.getElementById('totalTweets').textContent = 'Erreur';
    document.getElementById('totalVotes').textContent = 'Erreur';
    document.getElementById('aiTweets').textContent = 'Erreur';
  }
}

// Charger les stats de session
async function loadSessionStats() {
  const result = await chrome.storage.local.get(['sessionBlocked']);
  const count = result.sessionBlocked || 0;
  document.getElementById('sessionBlocked').textContent = count;
}

// Rafraîchir les stats
document.getElementById('refreshBtn').addEventListener('click', async () => {
  const btn = document.getElementById('refreshBtn');
  btn.classList.add('loading');
  btn.disabled = true;
  
  await loadStats();
  await loadSessionStats();
  
  btn.classList.remove('loading');
  btn.disabled = false;
});

// Reset session
document.getElementById('resetBtn').addEventListener('click', async () => {
  await chrome.storage.local.set({ sessionBlocked: 0 });
  document.getElementById('sessionBlocked').textContent = '0';
  
  // Envoyer message au background pour reset le badge
  chrome.runtime.sendMessage({ action: 'resetBlocked' });
});

// Sauvegarder les préférences
document.getElementById('autoHide').addEventListener('change', (e) => {
  chrome.storage.local.set({ autoHide: e.target.checked });
});

document.getElementById('showButtons').addEventListener('change', (e) => {
  chrome.storage.local.set({ showButtons: e.target.checked });
});

// Charger les préférences
async function loadPreferences() {
  const result = await chrome.storage.local.get(['autoHide', 'showButtons']);
  
  document.getElementById('autoHide').checked = result.autoHide !== false;
  document.getElementById('showButtons').checked = result.showButtons !== false;
}

// Initialisation au chargement
document.addEventListener('DOMContentLoaded', async () => {
  await loadStats();
  await loadSessionStats();
  await loadPreferences();
});
```

### Étape 9 : Créer les icônes

Vous avez besoin de 3 icônes (16x16, 48x48, 128x128 pixels).

**Option 1 : Utiliser un outil en ligne**
- Allez sur https://favicon.io/favicon-generator/
- Créez une icône avec le texte "AI" ou l'emoji "🤖"
- Téléchargez les différentes tailles
- Renommez-les : `icon16.png`, `icon48.png`, `icon128.png`
- Placez-les dans le dossier `icons/`

**Option 2 : Utiliser une image existante**
- Trouvez une image de robot/IA (PNG transparent de préférence)
- Redimensionnez-la avec https://imageresizer.com/
- Créez les 3 tailles nécessaires

---

## 🚀 INSTALLATION DANS LE NAVIGATEUR

### Sur Chrome / Brave

1. **Ouvrir la page des extensions**
   ```
   chrome://extensions/
   ```
   ou
   ```
   brave://extensions/
   ```

2. **Activer le mode développeur**
   - Toggle en haut à droite : "Mode développeur"

3. **Charger l'extension**
   - Cliquer sur "Charger l'extension non empaquetée"
   - Sélectionner le dossier `AIContentRemoverExtension`
   - ✅ L'extension est installée !

### Sur Microsoft Edge

1. **Ouvrir la page des extensions**
   ```
   edge://extensions/
   ```

2. **Activer le mode développeur**
   - Toggle en bas à gauche : "Mode développeur"

3. **Charger l'extension**
   - Cliquer sur "Charger l'extension décompressée"
   - Sélectionner le dossier `AIContentRemoverExtension`
   - ✅ L'extension est installée !

---

## 🧪 TESTS

### Test 1 : Vérifier que l'extension est chargée

1. Aller sur `twitter.com` ou `x.com`
2. Ouvrir la console développeur (F12)
3. Vous devriez voir : `🚀 AI Content Remover - Extension initialisée`

### Test 2 : Vérifier l'API

1. **S'assurer que le backend est lancé**
   ```bash
   cd AIContentRemover
   dotnet run
   ```

2. **Rafraîchir Twitter/X**
   - L'extension devrait scanner les tweets
   - Console : `🔍 Scan de X tweets...`

### Test 3 : Tester le masquage

1. **Tagger un tweet via l'API** (dans Swagger ou curl)
   ```bash
   curl -k -X POST https://localhost:5001/api/tweets/tag \
     -H "Content-Type: application/json" \
     -d "{\"tweetId\":\"REAL_TWEET_ID\",\"userIdentifier\":\"test\",\"isAiVote\":true}"
   ```
   (Répéter 3 fois avec différents userIdentifiers)

2. **Rafraîchir Twitter**
   - Le tweet devrait être masqué avec l'overlay rouge

### Test 4 : Tester le bouton de vote

1. Survoler un tweet non masqué
2. Cliquer sur le bouton "🤖 IA"
3. Le bouton devrait afficher "✅" puis revenir à "🤖 IA"

### Test 5 : Tester le popup

1. Cliquer sur l'icône de l'extension dans la barre
2. Vous devriez voir :
   - Les statistiques globales
   - Le nombre de tweets bloqués en session
   - Les boutons de configuration

---

## 📦 EMPAQUETAGE POUR PUBLICATION

### Préparer l'extension

1. **Nettoyer le code**
   - Retirer les console.log (optionnel)
   - Vérifier tous les fichiers

2. **Mettre à jour manifest.json**
   ```json
   {
     "host_permissions": [
       "https://api.votredomaine.com/*"
     ]
   }
   ```
   (Remplacer localhost par votre domaine de production)

3. **Créer un ZIP**
   - Sélectionner tous les fichiers DANS le dossier
   - Créer une archive ZIP
   - Nom : `ai-content-remover-v1.0.0.zip`

### Publier sur Chrome Web Store

1. **Compte développeur**
   - Aller sur https://chrome.google.com/webstore/devconsole
   - Créer un compte développeur (5$ unique)

2. **Soumettre l'extension**
   - "Nouvel élément"
   - Uploader le ZIP
   - Remplir les informations :
     - Nom : "AI Content Remover"
     - Description
     - Icônes
     - Captures d'écran
     - Catégorie : "Productivité"

3. **Attendre l'approbation** (1-3 jours)

### Publier sur Microsoft Edge Add-ons

1. **Compte développeur**
   - Aller sur https://partner.microsoft.com/dashboard/microsoftedge/
   - Créer un compte (gratuit)

2. **Soumettre l'extension**
   - Même process que Chrome
   - Réutiliser le même ZIP

3. **Attendre l'approbation** (1-3 jours)

### Brave

Brave utilise le Chrome Web Store, donc aucune action supplémentaire !

---

## 🐛 TROUBLESHOOTING

### L'extension ne se charge pas

**Erreur : "Manifest version 3 is not available"**
- Mettre à jour votre navigateur à la dernière version

**Erreur : "Could not load manifest"**
- Vérifier la syntaxe JSON du manifest.json
- Utiliser https://jsonlint.com/ pour valider

### L'extension ne détecte pas les tweets

**Problème : Console vide**
- Ouvrir F12 → Onglet "Console"
- Vérifier qu'il n'y a pas d'erreurs
- Recharger la page Twitter

**Problème : "Failed to fetch"**
- Vérifier que le backend est lancé (`dotnet run`)
- Vérifier l'URL dans `config.js`
- Vérifier les CORS du backend

### Erreur CORS

**Erreur : "has been blocked by CORS policy"**

Solution : Vérifier dans `Program.cs` du backend :
```csharp
app.UseCors("DevelopmentPolicy");
```

### Certificat SSL invalide (localhost)

**Erreur : "net::ERR_CERT_AUTHORITY_INVALID"**

Solution : Dans `manifest.json`, tester d'abord en HTTP :
```json
"host_permissions": [
  "http://localhost:5000/*"
]
```

Puis dans `config.js` :
```javascript
baseUrl: 'http://localhost:5000/api/tweets'
```

### L'extension ralentit Twitter

**Problème : Performance**

Solution : Augmenter l'intervalle de scan dans `config.js` :
```javascript
checkInterval: 5000 // 5 secondes au lieu de 2
```

---

## 🎯 PROCHAINES ÉTAPES

1. ✅ Créer les fichiers de l'extension
2. ✅ Installer en mode développeur
3. ✅ Tester sur Twitter/X avec le backend local
4. ⏳ Déployer le backend en production
5. ⏳ Mettre à jour l'URL de l'API dans config.js
6. ⏳ Créer des captures d'écran de l'extension
7. ⏳ Publier sur Chrome Web Store
8. ⏳ Publier sur Edge Add-ons

---

## 📊 RÉSUMÉ

| Étape | Fichier | Statut |
|-------|---------|--------|
| Manifest | manifest.json | ✅ À créer |
| Configuration | config.js | ✅ À créer |
| Content Script | content.js | ✅ Copier depuis extension-example.js |
| Background | background.js | ✅ À créer |
| Popup HTML | popup.html | ✅ À créer |
| Popup CSS | popup.css | ✅ À créer |
| Popup JS | popup.js | ✅ À créer |
| Icônes | icons/*.png | ✅ À générer |

---

**Votre extension est maintenant prête à être créée ! 🚀**

*Besoin d'aide ? Consultez la documentation complète dans le projet backend.*


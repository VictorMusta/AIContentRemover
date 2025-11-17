# 🤖 AI Content Remover - Extension Navigateur

Extension pour Chrome, Edge et Brave qui détecte et masque automatiquement les tweets taggés comme contenu IA par la communauté.

---

## 📦 INSTALLATION (Mode Développeur)

### 1. Prérequis
- Backend API lancé : `dotnet run` dans le dossier `AIContentRemover`
- Navigateur : Chrome, Edge ou Brave

### 2. Installation

#### Sur Chrome / Brave
1. Ouvrir `chrome://extensions/` (ou `brave://extensions/`)
2. Activer "Mode développeur" (toggle en haut à droite)
3. Cliquer sur "Charger l'extension non empaquetée"
4. Sélectionner le dossier `BrowserExtension`
5. ✅ Extension installée !

#### Sur Microsoft Edge
1. Ouvrir `edge://extensions/`
2. Activer "Mode développeur" (toggle en bas à gauche)
3. Cliquer sur "Charger l'extension décompressée"
4. Sélectionner le dossier `BrowserExtension`
5. ✅ Extension installée !

---

## ⚙️ CONFIGURATION

### Modifier l'URL de l'API

Éditer `config.js` :

```javascript
const API_CONFIG = {
  baseUrl: 'https://localhost:5001/api/tweets', // Votre URL
  apiKey: '', // Optionnel
  // ...
};
```

**En développement :** `https://localhost:5001/api/tweets`  
**En production :** `https://api.votredomaine.com/api/tweets`

### Activer l'API Key (si nécessaire)

Si votre backend a `RequireApiKey = true`, éditez `config.js` :

```javascript
const API_CONFIG = {
  apiKey: 'votre-api-key-ici',
  // ...
};
```

---

## 🎯 UTILISATION

1. **Aller sur Twitter/X**
   - https://twitter.com ou https://x.com

2. **L'extension scanne automatiquement**
   - Console : `🔍 Scan de X tweets...`
   - Les tweets taggés IA sont masqués avec un overlay rouge

3. **Voter pour un tweet**
   - Survoler un tweet
   - Cliquer sur le bouton "🤖 IA"
   - Le tweet est taggé et sera masqué si le seuil est atteint

4. **Downvoter**
   - Sur un tweet masqué, cliquer "Pas IA"
   - Si le score descend sous le seuil, le tweet est affiché à nouveau

5. **Voir les statistiques**
   - Cliquer sur l'icône de l'extension dans la barre
   - Popup avec statistiques globales et de session

---

## 🔧 STRUCTURE DES FICHIERS

```
BrowserExtension/
├── manifest.json       # Configuration Manifest V3
├── config.js          # Configuration API
├── content.js         # Script injecté dans Twitter/X
├── background.js      # Service worker arrière-plan
├── popup.html         # Interface popup
├── popup.css          # Styles popup
├── popup.js           # Logique popup
├── icons/             # Icônes (16, 48, 128 px)
└── README.md          # Ce fichier
```

---

## 🎨 CRÉER LES ICÔNES

### Méthode 1 : Générateur en ligne

1. Aller sur https://favicon.io/favicon-generator/
2. Configurer :
   - Texte : "AI"
   - Fond : #ef4444 (rouge)
   - Police : Bold
3. Télécharger et renommer :
   - `android-chrome-192x192.png` → `icon128.png`
   - `favicon-32x32.png` → `icon48.png`
   - `favicon-16x16.png` → `icon16.png`
4. Placer dans `icons/`

### Méthode 2 : Emoji robot 🤖

1. Aller sur https://emojipedia.org/robot/
2. Copier l'image ou faire screenshot
3. Redimensionner avec https://imageresizer.com/
   - Créer 3 versions : 16px, 48px, 128px
4. Placer dans `icons/`

### Méthode 3 : IA (DALL-E, Midjourney)

Prompt : "Simple flat icon of a robot head, red and white colors, minimalist, transparent background, 512x512"

---

## 🧪 TESTS

### Test 1 : Extension chargée
1. Aller sur Twitter/X
2. Ouvrir Console (F12)
3. Voir : `🚀 AI Content Remover - Extension initialisée`

### Test 2 : Connexion API
1. Vérifier que le backend est lancé
2. Cliquer sur l'icône extension
3. Le popup doit afficher les stats (pas "Erreur")

### Test 3 : Masquage automatique
1. Tagger un tweet 3 fois via l'API (Swagger ou curl)
2. Aller sur Twitter et trouver ce tweet
3. Il devrait être masqué avec l'overlay rouge

### Test 4 : Bouton de vote
1. Survoler un tweet non masqué
2. Cliquer "🤖 IA"
3. Le bouton affiche "✅" puis revient à "🤖 IA"

---

## 🐛 RÉSOLUTION DES PROBLÈMES

### L'extension ne se charge pas

**Erreur : "Manifest version 3 is not available"**
→ Mettre à jour votre navigateur

**Erreur : "Could not load manifest"**
→ Vérifier la syntaxe JSON de `manifest.json` sur https://jsonlint.com/

### Aucun tweet détecté

**Console vide**
→ Recharger la page Twitter (F5)
→ Vérifier que vous êtes bien sur twitter.com ou x.com

**Erreur : "Failed to fetch"**
→ Vérifier que le backend est lancé (`dotnet run`)
→ Vérifier l'URL dans `config.js`

### Erreur CORS

**"blocked by CORS policy"**
→ Vérifier que le backend a `app.UseCors("DevelopmentPolicy")` dans `Program.cs`

### Certificat SSL invalide

**"net::ERR_CERT_AUTHORITY_INVALID"**

Solution temporaire : Utiliser HTTP au lieu de HTTPS

Dans `config.js` :
```javascript
baseUrl: 'http://localhost:5000/api/tweets'
```

Dans `manifest.json` :
```json
"host_permissions": [
  "http://localhost:5000/*"
]
```

### L'extension ralentit Twitter

Solution : Augmenter l'intervalle dans `config.js` :
```javascript
checkInterval: 5000 // 5 secondes au lieu de 3
```

---

## 📊 FONCTIONNALITÉS

✅ Scan automatique des tweets visibles  
✅ Requêtes batch optimisées (50 tweets à la fois)  
✅ Masquage visuel avec overlay  
✅ Boutons de vote intégrés  
✅ Badge avec compteur de tweets bloqués  
✅ Popup avec statistiques détaillées  
✅ Configuration via l'interface  
✅ Stockage local des préférences  
✅ Observer pour le scroll infini  
✅ Debounce pour optimiser les performances  

---

## 🔄 MISES À JOUR

Pour mettre à jour l'extension après des modifications :

1. Éditer les fichiers nécessaires
2. Aller sur `chrome://extensions/`
3. Cliquer sur le bouton "Recharger" (↻) de l'extension

---

## 📦 EMPAQUETAGE POUR PUBLICATION

### Préparer

1. Mettre à jour `config.js` avec l'URL de production
2. Supprimer les console.log (optionnel)
3. Vérifier que toutes les icônes sont présentes

### Créer le ZIP

```bash
cd BrowserExtension
# Sur Windows
Compress-Archive -Path * -DestinationPath ai-content-remover-v1.0.0.zip

# Sur Linux/Mac
zip -r ai-content-remover-v1.0.0.zip *
```

### Publier

**Chrome Web Store :**
- https://chrome.google.com/webstore/devconsole
- Frais unique : 5$

**Microsoft Edge Add-ons :**
- https://partner.microsoft.com/dashboard/microsoftedge/
- Gratuit

**Brave :**
- Utilise le Chrome Web Store automatiquement

---

## 📄 LICENCE

MIT License - Libre d'utilisation

---

## 👤 SUPPORT

Pour les problèmes techniques :
1. Consulter le README du backend
2. Vérifier la console navigateur (F12)
3. Vérifier les logs du backend

---

**Fait avec ❤️ pour un web plus transparent**


// ================================================================
// EXEMPLE D'INTÉGRATION EXTENSION NAVIGATEUR
// AI Content Remover - Content Script
// ================================================================

// Configuration
const API_CONFIG = {
  baseUrl: 'https://localhost:5001/api/tweets',
  apiKey: '', // Optionnel, si RequireApiKey est true
  batchSize: 50,
  checkInterval: 2000 // ms
};

// ================================================================
// GESTION DE L'IDENTIFIANT UTILISATEUR
// ================================================================

/**
 * Récupère ou crée un UUID unique pour l'utilisateur
 * Stocké localement dans le storage de l'extension
 */
async function getUserIdentifier() {
  return new Promise((resolve) => {
    chrome.storage.local.get(['userIdentifier'], (result) => {
      if (result.userIdentifier) {
        resolve(result.userIdentifier);
      } else {
        // Générer un UUID v4
        const uuid = crypto.randomUUID();
        chrome.storage.local.set({ userIdentifier: uuid }, () => {
          resolve(uuid);
        });
      }
    });
  });
}

// ================================================================
// API CALLS
// ================================================================

/**
 * Headers communs pour toutes les requêtes
 */
function getHeaders() {
  const headers = {
    'Content-Type': 'application/json'
  };
  
  if (API_CONFIG.apiKey) {
    headers['X-API-Key'] = API_CONFIG.apiKey;
  }
  
  return headers;
}

/**
 * Vérifier un seul tweet
 */
async function checkSingleTweet(tweetId) {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}/check/${tweetId}`,
      { headers: getHeaders() }
    );
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la vérification du tweet:', error);
    return null;
  }
}

/**
 * Vérifier plusieurs tweets en batch (RECOMMANDÉ)
 */
async function checkTweetsBatch(tweetIds) {
  try {
    // Limiter à 100 tweets par requête
    const limitedIds = tweetIds.slice(0, 100);
    
    const response = await fetch(
      `${API_CONFIG.baseUrl}/check/batch`,
      {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify({ tweetIds: limitedIds })
      }
    );
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    
    const data = await response.json();
    return data.results;
  } catch (error) {
    console.error('Erreur lors du batch check:', error);
    return [];
  }
}

/**
 * Tagger un tweet comme IA ou Not IA
 */
async function tagTweet(tweetId, isAiVote) {
  try {
    const userIdentifier = await getUserIdentifier();
    
    const response = await fetch(
      `${API_CONFIG.baseUrl}/tag`,
      {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify({
          tweetId,
          userIdentifier,
          isAiVote
        })
      }
    );
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors du tag:', error);
    return { success: false, message: error.message };
  }
}

/**
 * Récupérer les statistiques globales
 */
async function getStats() {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}/stats`,
      { headers: getHeaders() }
    );
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des stats:', error);
    return null;
  }
}

// ================================================================
// EXTRACTION DES TWEETS DEPUIS TWITTER/X
// ================================================================

/**
 * Extraire les IDs de tweets visibles sur la page
 * Compatible Twitter et X.com
 */
function extractVisibleTweetIds() {
  const tweetIds = new Set();
  
  // Sélecteurs pour Twitter/X (peut nécessiter des ajustements)
  const tweetElements = document.querySelectorAll('article[data-testid="tweet"]');
  
  tweetElements.forEach(element => {
    // Essayer d'extraire l'ID depuis le lien du tweet
    const link = element.querySelector('a[href*="/status/"]');
    if (link) {
      const match = link.href.match(/\/status\/(\d+)/);
      if (match && match[1]) {
        tweetIds.add(match[1]);
      }
    }
  });
  
  return Array.from(tweetIds);
}

/**
 * Trouver l'élément DOM d'un tweet par son ID
 */
function findTweetElement(tweetId) {
  const links = document.querySelectorAll(`a[href*="/status/${tweetId}"]`);
  
  for (const link of links) {
    const article = link.closest('article[data-testid="tweet"]');
    if (article) {
      return article;
    }
  }
  
  return null;
}

// ================================================================
// MASQUAGE ET UI
// ================================================================

/**
 * Masquer un tweet taggé comme IA
 */
function hideTweet(tweetId, data) {
  const element = findTweetElement(tweetId);
  if (!element) return;
  
  // Ajouter une classe pour le CSS
  element.classList.add('ai-content-hidden');
  
  // Créer un overlay avec les infos
  const overlay = document.createElement('div');
  overlay.className = 'ai-content-overlay';
  overlay.innerHTML = `
    <div class="ai-content-warning">
      <span class="ai-icon">🤖</span>
      <span class="ai-text">Contenu IA détecté par la communauté</span>
      <span class="ai-score">Score: ${data.score} (${data.aiVotes} 👍 / ${data.notAiVotes} 👎)</span>
      <button class="ai-show-btn">Afficher quand même</button>
      <button class="ai-downvote-btn">Pas IA</button>
    </div>
  `;
  
  // Insérer l'overlay
  element.style.position = 'relative';
  element.insertBefore(overlay, element.firstChild);
  
  // Flouter le contenu
  Array.from(element.children).forEach(child => {
    if (child !== overlay) {
      child.style.filter = 'blur(10px)';
      child.style.pointerEvents = 'none';
    }
  });
  
  // Bouton "Afficher quand même"
  overlay.querySelector('.ai-show-btn').addEventListener('click', () => {
    element.classList.remove('ai-content-hidden');
    overlay.remove();
    Array.from(element.children).forEach(child => {
      child.style.filter = '';
      child.style.pointerEvents = '';
    });
  });
  
  // Bouton "Pas IA" (downvote)
  overlay.querySelector('.ai-downvote-btn').addEventListener('click', async () => {
    const result = await tagTweet(tweetId, false);
    if (result.success) {
      overlay.querySelector('.ai-score').textContent = 
        `Score: ${result.tweetData.score} (${result.tweetData.aiVotes} 👍 / ${result.tweetData.notAiVotes} 👎)`;
      
      // Si le score est maintenant négatif, retirer le masque
      if (result.tweetData.score < 3) {
        element.classList.remove('ai-content-hidden');
        overlay.remove();
        Array.from(element.children).forEach(child => {
          child.style.filter = '';
          child.style.pointerEvents = '';
        });
      }
    }
  });
}

/**
 * Ajouter un bouton "Tagger comme IA" sur chaque tweet
 */
function addTagButton(tweetId) {
  const element = findTweetElement(tweetId);
  if (!element || element.querySelector('.ai-tag-button')) return;
  
  // Trouver la barre d'actions (reply, retweet, like, etc.)
  const actionBar = element.querySelector('[role="group"]');
  if (!actionBar) return;
  
  const button = document.createElement('button');
  button.className = 'ai-tag-button';
  button.innerHTML = '🤖 IA';
  button.title = 'Signaler ce tweet comme contenu IA';
  
  button.addEventListener('click', async (e) => {
    e.preventDefault();
    e.stopPropagation();
    
    button.disabled = true;
    button.textContent = '⏳';
    
    const result = await tagTweet(tweetId, true);
    
    if (result.success) {
      button.textContent = '✅';
      button.style.color = '#10b981';
      
      // Rafraîchir l'affichage si nécessaire
      if (result.tweetData.isAiContent) {
        hideTweet(tweetId, result.tweetData);
      }
    } else {
      button.textContent = '❌';
      button.style.color = '#ef4444';
    }
    
    setTimeout(() => {
      button.textContent = '🤖 IA';
      button.disabled = false;
    }, 2000);
  });
  
  actionBar.appendChild(button);
}

// ================================================================
// SCAN ET SURVEILLANCE
// ================================================================

/**
 * Scanner tous les tweets visibles
 */
async function scanVisibleTweets() {
  const tweetIds = extractVisibleTweetIds();
  
  if (tweetIds.length === 0) {
    return;
  }
  
  console.log(`🔍 Scan de ${tweetIds.length} tweets...`);
  
  // Checker en batch (OPTIMISÉ)
  const results = await checkTweetsBatch(tweetIds);
  
  // Traiter les résultats
  results.forEach(tweet => {
    // Ajouter le bouton de tag
    addTagButton(tweet.tweetId);
    
    // Masquer si taggé IA
    if (tweet.isAiContent) {
      hideTweet(tweet.tweetId, tweet);
    }
  });
  
  console.log(`✅ Scan terminé: ${results.filter(t => t.isAiContent).length} tweets IA détectés`);
}

/**
 * Observer les nouveaux tweets (scroll infini)
 */
function observeNewTweets() {
  const observer = new MutationObserver((mutations) => {
    let hasNewTweets = false;
    
    mutations.forEach(mutation => {
      mutation.addedNodes.forEach(node => {
        if (node.nodeType === 1 && node.matches('article[data-testid="tweet"]')) {
          hasNewTweets = true;
        }
      });
    });
    
    if (hasNewTweets) {
      // Debounce pour éviter trop de scans
      clearTimeout(window.aiContentScanTimeout);
      window.aiContentScanTimeout = setTimeout(scanVisibleTweets, 500);
    }
  });
  
  observer.observe(document.body, {
    childList: true,
    subtree: true
  });
  
  return observer;
}

// ================================================================
// CSS INJECTION
// ================================================================

function injectStyles() {
  const style = document.createElement('style');
  style.textContent = `
    .ai-content-hidden {
      position: relative;
      min-height: 200px;
    }
    
    .ai-content-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(239, 68, 68, 0.05);
      border: 2px solid #ef4444;
      border-radius: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10;
      backdrop-filter: blur(2px);
    }
    
    .ai-content-warning {
      background: white;
      padding: 20px;
      border-radius: 12px;
      box-shadow: 0 4px 6px rgba(0,0,0,0.1);
      text-align: center;
      max-width: 400px;
    }
    
    .ai-icon {
      font-size: 48px;
      display: block;
      margin-bottom: 12px;
    }
    
    .ai-text {
      display: block;
      font-size: 16px;
      font-weight: 600;
      color: #ef4444;
      margin-bottom: 8px;
    }
    
    .ai-score {
      display: block;
      font-size: 14px;
      color: #666;
      margin-bottom: 16px;
    }
    
    .ai-show-btn, .ai-downvote-btn {
      padding: 8px 16px;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 600;
      margin: 0 4px;
      transition: all 0.2s;
    }
    
    .ai-show-btn {
      background: #3b82f6;
      color: white;
    }
    
    .ai-show-btn:hover {
      background: #2563eb;
    }
    
    .ai-downvote-btn {
      background: #f3f4f6;
      color: #374151;
    }
    
    .ai-downvote-btn:hover {
      background: #e5e7eb;
    }
    
    .ai-tag-button {
      background: transparent;
      border: 1px solid #e5e7eb;
      border-radius: 6px;
      padding: 4px 12px;
      cursor: pointer;
      font-size: 13px;
      margin-left: 8px;
      transition: all 0.2s;
    }
    
    .ai-tag-button:hover {
      background: #fef2f2;
      border-color: #ef4444;
      color: #ef4444;
    }
  `;
  
  document.head.appendChild(style);
}

// ================================================================
// INITIALISATION
// ================================================================

async function init() {
  console.log('🚀 AI Content Remover - Extension initialisée');
  
  // Injecter les styles
  injectStyles();
  
  // Scanner initial
  await scanVisibleTweets();
  
  // Observer les nouveaux tweets
  observeNewTweets();
  
  // Scanner périodiquement
  setInterval(scanVisibleTweets, API_CONFIG.checkInterval);
  
  // Afficher les stats dans la console
  const stats = await getStats();
  if (stats) {
    console.log('📊 Stats globales:', stats);
  }
}

// Démarrer quand le DOM est prêt
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}


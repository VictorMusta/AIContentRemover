// Popup de l'extension - Interface utilisateur

// Charger les statistiques depuis l'API
async function loadStats() {
  try {
    const response = await fetch(`${API_CONFIG.baseUrl}/stats`, {
      headers: getHeaders()
    });
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    
    const stats = await response.json();
    
    // Mettre à jour l'interface
    document.getElementById('totalTweets').textContent = stats.totalTweets.toLocaleString('fr-FR');
    document.getElementById('totalVotes').textContent = stats.totalVotes.toLocaleString('fr-FR');
    document.getElementById('aiTweets').textContent = stats.tweetsMarkedAsAI.toLocaleString('fr-FR');
    
    // Status API connectée
    updateApiStatus(true);
    
  } catch (error) {
    console.error('Erreur lors du chargement des stats:', error);
    document.getElementById('totalTweets').textContent = 'Erreur';
    document.getElementById('totalVotes').textContent = 'Erreur';
    document.getElementById('aiTweets').textContent = 'Erreur';
    
    // Status API déconnectée
    updateApiStatus(false, error.message);
  }
}

// Fonction helper pour les headers
function getHeaders() {
  const headers = {
    'Content-Type': 'application/json'
  };
  
  if (API_CONFIG.apiKey) {
    headers['X-API-Key'] = API_CONFIG.apiKey;
  }
  
  return headers;
}

// Mettre à jour le status de l'API
function updateApiStatus(connected, errorMessage = '') {
  const statusEl = document.getElementById('apiStatus');
  const statusText = statusEl.querySelector('.status-text');
  
  if (connected) {
    statusEl.className = 'api-status connected';
    statusText.textContent = '✅ API connectée';
  } else {
    statusEl.className = 'api-status error';
    statusText.textContent = `❌ API déconnectée: ${errorMessage}`;
  }
}

// Charger les stats de session
async function loadSessionStats() {
  const result = await chrome.storage.local.get(['sessionBlocked']);
  const count = result.sessionBlocked || 0;
  document.getElementById('sessionBlocked').textContent = count.toLocaleString('fr-FR');
}

// Rafraîchir les stats
document.getElementById('refreshBtn').addEventListener('click', async () => {
  const btn = document.getElementById('refreshBtn');
  const btnIcon = btn.querySelector('.btn-icon');
  
  btn.classList.add('loading');
  btn.disabled = true;
  
  await loadStats();
  await loadSessionStats();
  
  setTimeout(() => {
    btn.classList.remove('loading');
    btn.disabled = false;
  }, 500);
});

// Reset session
document.getElementById('resetBtn').addEventListener('click', async () => {
  if (confirm('Réinitialiser le compteur de session ?')) {
    await chrome.storage.local.set({ sessionBlocked: 0 });
    document.getElementById('sessionBlocked').textContent = '0';
    
    // Envoyer message au background pour reset le badge
    chrome.runtime.sendMessage({ action: 'resetBlocked' });
  }
});

// Sauvegarder les préférences
document.getElementById('autoHide').addEventListener('change', async (e) => {
  await chrome.storage.local.set({ autoHide: e.target.checked });
  
  // Recharger l'onglet Twitter/X actuel pour appliquer le changement
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (tab && (tab.url.includes('twitter.com') || tab.url.includes('x.com'))) {
    chrome.tabs.reload(tab.id);
  }
});

document.getElementById('showButtons').addEventListener('change', async (e) => {
  await chrome.storage.local.set({ showButtons: e.target.checked });
  
  // Recharger l'onglet Twitter/X actuel
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (tab && (tab.url.includes('twitter.com') || tab.url.includes('x.com'))) {
    chrome.tabs.reload(tab.id);
  }
});

// Charger les préférences
async function loadPreferences() {
  const result = await chrome.storage.local.get(['autoHide', 'showButtons']);
  
  document.getElementById('autoHide').checked = result.autoHide !== false;
  document.getElementById('showButtons').checked = result.showButtons !== false;
}

// Initialisation au chargement
document.addEventListener('DOMContentLoaded', async () => {
  // Afficher un état de chargement
  updateApiStatus(false, 'Connexion en cours...');
  
  // Charger toutes les données
  await Promise.all([
    loadStats(),
    loadSessionStats(),
    loadPreferences()
  ]);
  
  // Auto-refresh toutes les 10 secondes si le popup est ouvert
  setInterval(async () => {
    await loadStats();
    await loadSessionStats();
  }, 10000);
});


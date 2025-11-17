// Service Worker - Arrière-plan de l'extension

console.log('🤖 AI Content Remover - Background service démarré');

// Écouter l'installation
chrome.runtime.onInstalled.addListener((details) => {
  if (details.reason === 'install') {
    console.log('✅ Extension installée pour la première fois !');
    
    // Initialiser le stockage
    chrome.storage.local.set({
      sessionBlocked: 0,
      autoHide: true,
      showButtons: true
    });
    
    // Optionnel : Ouvrir une page d'accueil
    // chrome.tabs.create({
    //   url: 'https://github.com/votre-repo/ai-content-remover'
    // });
    
  } else if (details.reason === 'update') {
    console.log('🔄 Extension mise à jour vers la version', chrome.runtime.getManifest().version);
  }
});

// Badge pour afficher le nombre de tweets bloqués
let blockedCount = 0;

// Gérer les messages depuis content.js
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.action === 'incrementBlocked') {
    blockedCount++;
    chrome.action.setBadgeText({ 
      text: blockedCount.toString(),
      tabId: sender.tab.id
    });
    chrome.action.setBadgeBackgroundColor({ 
      color: '#ef4444'
    });
    
    // Mettre à jour le compteur de session
    chrome.storage.local.get(['sessionBlocked'], (result) => {
      const count = (result.sessionBlocked || 0) + 1;
      chrome.storage.local.set({ sessionBlocked: count });
    });
    
    sendResponse({ success: true });
    
  } else if (request.action === 'resetBlocked') {
    blockedCount = 0;
    chrome.action.setBadgeText({ text: '' });
    chrome.storage.local.set({ sessionBlocked: 0 });
    sendResponse({ success: true });
    
  } else if (request.action === 'getBlockedCount') {
    sendResponse({ count: blockedCount });
  }
  
  return true; // Permet la réponse asynchrone
});

// Réinitialiser le compteur au redémarrage du navigateur
chrome.runtime.onStartup.addListener(() => {
  blockedCount = 0;
  chrome.action.setBadgeText({ text: '' });
});


// Configuration globale de l'API
const API_CONFIG = {
  // URL de l'API backend
  // En développement : http://localhost:5000 ou https://localhost:5001
  // En production : https://api.votredomaine.com
  baseUrl: 'https://localhost:44323/api/tweets',
  
  // API Key (optionnelle - laisser vide si RequireApiKey = false dans le backend)
  apiKey: '',
  
  // Taille des batches pour les requêtes groupées
  batchSize: 50,
  
  // Intervalle de scan automatique (en millisecondes)
  checkInterval: 3000, // 3 secondes
  
  // Seuil de votes pour considérer un tweet comme IA
  threshold: 3
};


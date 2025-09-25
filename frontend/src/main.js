import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import axios from 'axios'
import App from './App.vue'
import Login from './Login.vue'
import Users from './Users.vue'
import './assets/main.css' 

// Set the API base URL using ngrok HTTPS tunnel
if (import.meta.env.PROD) {
  // Your actual ngrok URL
  axios.defaults.baseURL = 'https://subdorsal-jerica-smokelessly.ngrok-free.dev'
} else {
  // Development - use local API
  axios.defaults.baseURL = 'http://localhost:5000'
}


console.log('API Base URL:', axios.defaults.baseURL)

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', component: Login },
  { path: '/users', component: Users }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

createApp(App).use(router).mount('#app')
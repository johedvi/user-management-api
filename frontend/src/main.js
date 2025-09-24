import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import axios from 'axios'  // Add this import
import App from './App.vue'
import Login from './Login.vue'
import Users from './Users.vue'
import './assets/main.css' 


if (import.meta.env.PROD) {
  axios.defaults.baseURL = import.meta.env.VITE_API_URL
}

const routes = [
  { path: '/', redirect: '/users' },
  { path: '/login', component: Login },
  { path: '/users', component: Users }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

createApp(App).use(router).mount('#app')
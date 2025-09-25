import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import axios from 'axios'
import App from './App.vue'
import Login from './Login.vue'
import Users from './Users.vue'
import './assets/main.css' 

// Set the API base URL to your container
if (import.meta.env.PROD) {
  axios.defaults.baseURL = 'http://userapi09041259.swedencentral.azurecontainer.io:8080'
}

console.log('API Base URL:', axios.defaults.baseURL)

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
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import Login from './Login.vue'
import Users from './Users.vue'
import './assets/main.css' 

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
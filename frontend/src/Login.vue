<template>
  <div>
    <h2>Login</h2>
    <div class="form">
      <input v-model="email" placeholder="Email" type="email"><br>
      <input v-model="password" placeholder="Password" type="password"><br>
      <button @click="login">Login</button>
      <button @click="register">Register</button>
    </div>
    <p v-if="message">{{ message }}</p>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  data() {
    return {
      email: '',
      password: '',
      message: ''
    }
  },
  methods: {
    async login() {
      try {
        const response = await axios.post('/api/auth/login', {
          email: this.email,
          password: this.password
        })
        localStorage.setItem('token', response.data.token)
        this.message = 'Login successful!'
        this.$router.push('/users')
      } catch (error) {
        this.message = 'Login failed: ' + error.response.data.message
      }
    },
    async register() {
      try {
        const response = await axios.post('/api/auth/register', {
          username: this.email.split('@')[0], // Use email prefix as username
          email: this.email,
          password: this.password
        })
        localStorage.setItem('token', response.data.token)
        this.message = 'Registration successful!'
        this.$router.push('/users')
      } catch (error) {
        this.message = 'Registration failed: ' + error.response.data.message
      }
    }
  }
}
</script>
<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 bg-gradient-to-br from-blue-50 to-indigo-100">
    <div class="max-w-md w-full space-y-8">
      <!-- Header -->
      <div class="text-center">
        <div class="mx-auto w-12 h-12 bg-blue-600 rounded-full flex items-center justify-center">
          <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
          </svg>
        </div>
        <h2 class="mt-6 text-3xl font-bold text-gray-900">
          {{ isRegistering ? 'Create Account' : 'Welcome back' }}
        </h2>
        <p class="mt-2 text-sm text-gray-600">
          {{ isRegistering ? 'Sign up for a new account' : 'Sign in to your account' }}
        </p>
      </div>

      <!-- Form -->
      <div class="bg-white py-8 px-6 shadow-lg rounded-lg">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Username (only for registration) -->
          <div v-if="isRegistering">
            <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
            <input
              v-model="form.username"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Enter your username"
            />
          </div>

          <!-- Email -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              v-model="form.email"
              type="email"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Enter your email"
            />
          </div>

          <!-- Password -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              v-model="form.password"
              type="password"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Enter your password"
            />
          </div>

          <!-- First/Last Name for registration -->
          <div v-if="isRegistering" class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">First Name</label>
              <input
                v-model="form.firstName"
                type="text"
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="First name"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
              <input
                v-model="form.lastName"
                type="text"
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Last name"
              />
            </div>
          </div>

          <!-- Error Message -->
          <div v-if="errorMessage" class="p-3 bg-red-50 border border-red-200 rounded-md">
            <p class="text-sm text-red-800">{{ errorMessage }}</p>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="loading">{{ isRegistering ? 'Creating Account...' : 'Signing In...' }}</span>
            <span v-else>{{ isRegistering ? 'Create Account' : 'Sign In' }}</span>
          </button>

          <!-- Toggle Mode -->
          <div class="text-center">
            <button
              type="button"
              @click="toggleMode"
              class="text-sm text-blue-600 hover:text-blue-500 font-medium"
            >
              {{ isRegistering ? 'Already have an account? Sign in' : "Don't have an account? Sign up" }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  data() {
    return {
      isRegistering: false,
      loading: false,
      errorMessage: '',
      form: {
        username: '',
        email: '',
        password: '',
        firstName: '',
        lastName: ''
      }
    }
  },
  methods: {
    toggleMode() {
      this.isRegistering = !this.isRegistering
      this.errorMessage = ''
      Object.keys(this.form).forEach(key => this.form[key] = '')
    },

    async handleSubmit() {
      this.errorMessage = ''
      this.loading = true

      try {
        const endpoint = this.isRegistering ? '/api/auth/register' : '/api/auth/login'
        const payload = this.isRegistering ? this.form : { 
          email: this.form.email, 
          password: this.form.password 
        }

        const response = await axios.post(endpoint, payload)
        localStorage.setItem('token', response.data.token)
        this.$router.push('/users')
      } catch (error) {
        this.errorMessage = error.response?.data?.message || 
          (this.isRegistering ? 'Registration failed' : 'Login failed')
      } finally {
        this.loading = false
      }
    }
  }
}
</script>
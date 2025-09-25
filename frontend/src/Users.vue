<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900">Users Management</h1>
            <p class="mt-2 text-gray-600">Manage your application users</p>
          </div>
          <button
            @click="openCreateModal"
            class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center"
          >
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/>
            </svg>
            Add User
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span class="ml-2 text-gray-600">Loading users...</span>
      </div>

      <!-- Error Message -->
      <div v-if="errorMessage && !loading" class="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
        <p class="text-sm text-red-800">{{ errorMessage }}</p>
        <button
          @click="loadUsers"
          class="mt-2 bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded text-sm"
        >
          Retry
        </button>
      </div>

      <!-- Users Grid -->
      <div v-else-if="users.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="user in users"
          :key="user.id"
          class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow duration-200"
        >
          <!-- User Avatar -->
          <div class="flex items-center mb-4">
            <div class="w-12 h-12 bg-gradient-to-r from-blue-500 to-purple-600 rounded-full flex items-center justify-center">
              <span class="text-white font-semibold text-lg">
                {{ getUserInitials(user) }}
              </span>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-semibold text-gray-900">
                {{ getUserDisplayName(user) }}
              </h3>
              <p class="text-sm text-gray-500">@{{ user.username || 'unknown' }}</p>
            </div>
          </div>

          <!-- User Info -->
          <div class="space-y-2 mb-4">
            <div class="flex items-center text-sm text-gray-600">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207"/>
              </svg>
              {{ user.email || 'No email' }}
            </div>
            <div v-if="user.createdAt" class="flex items-center text-sm text-gray-600">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3a4 4 0 118 0v4m-4 8V9"/>
              </svg>
              Member since {{ formatDate(user.createdAt) }}
            </div>
          </div>

          <!-- Actions -->
          <div class="flex space-x-2">
            <button
              @click="editUser(user)"
              class="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-700 px-3 py-2 rounded-md text-sm font-medium transition-colors duration-200"
            >
              Edit
            </button>
            <button
              @click="deleteUser(user.id)"
              class="flex-1 bg-red-50 hover:bg-red-100 text-red-700 px-3 py-2 rounded-md text-sm font-medium transition-colors duration-200"
            >
              Delete
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="!loading" class="text-center py-12">
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"/>
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900">No users</h3>
        <p class="mt-1 text-sm text-gray-500">Get started by creating a new user.</p>
        <div class="mt-6">
          <button
            @click="openCreateModal"
            class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200"
          >
            Add your first user
          </button>
        </div>
      </div>

      <!-- Create/Edit User Modal -->
      <div v-if="showModal" class="fixed inset-0 z-50 overflow-y-auto">
        <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
          <!-- Background overlay -->
          <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" @click="closeModal"></div>

          <!-- Modal -->
          <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
                {{ isEditing ? 'Edit User' : 'Create New User' }}
              </h3>

              <form @submit.prevent="handleSubmit" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
                  <input
                    v-model="form.username"
                    type="text"
                    required
                    class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
                  <input
                    v-model="form.email"
                    type="email"
                    required
                    class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div v-if="!isEditing">
                  <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
                  <input
                    v-model="form.password"
                    type="password"
                    required
                    class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">First Name</label>
                    <input
                      v-model="form.firstName"
                      type="text"
                      class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
                    <input
                      v-model="form.lastName"
                      type="text"
                      class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                </div>

                <!-- Error Message -->
                <div v-if="modalErrorMessage" class="p-3 bg-red-50 border border-red-200 rounded-md">
                  <p class="text-sm text-red-800">{{ modalErrorMessage }}</p>
                </div>

                <!-- Buttons -->
                <div class="flex justify-end space-x-3 pt-4">
                  <button
                    type="button"
                    @click="closeModal"
                    class="bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded-md border border-gray-300 transition-colors duration-200"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    :disabled="submitting"
                    class="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <span v-if="submitting">{{ isEditing ? 'Updating...' : 'Creating...' }}</span>
                    <span v-else>{{ isEditing ? 'Update User' : 'Create User' }}</span>
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>

      <!-- Success Message -->
      <div v-if="successMessage" class="fixed top-4 right-4 bg-green-50 border border-green-200 rounded-lg p-4 shadow-lg">
        <div class="flex items-center">
          <svg class="w-5 h-5 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
          </svg>
          <p class="text-sm font-medium text-green-800">{{ successMessage }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  data() {
    return {
      users: [],
      loading: false,
      submitting: false,
      showModal: false,
      isEditing: false,
      currentUser: null,
      errorMessage: '',
      modalErrorMessage: '',
      successMessage: '',
      form: {
        username: '',
        email: '',
        password: '',
        firstName: '',
        lastName: ''
      }
    }
  },
  async mounted() {
    await this.loadUsers()
  },
  methods: {
    async loadUsers() {
      this.loading = true
      this.errorMessage = ''
      
      try {
        const token = localStorage.getItem('token')
        if (!token) {
          this.$router.push('/login')
          return
        }
        
        const response = await axios.get('/api/users', {
          headers: { Authorization: `Bearer ${token}` }
        })
        
        // Filter out any invalid user objects and ensure all users have required properties
        this.users = (response.data || []).filter(user => 
          user && 
          typeof user === 'object' && 
          (user.username || user.email) &&
          user.id
        ).map(user => ({
          ...user,
          username: user.username || '',
          email: user.email || '',
          firstName: user.firstName || '',
          lastName: user.lastName || ''
        }))
        
        console.log('Loaded users:', this.users)
        
      } catch (error) {
        console.error('Error loading users:', error)
        this.errorMessage = error.response?.data?.message || 'Failed to load users'
        
        if (error.response && error.response.status === 401) {
          localStorage.removeItem('token')
          this.$router.push('/login')
        }
      } finally {
        this.loading = false
      }
    },

    getUserInitials(user) {
      // Add comprehensive null checks to prevent errors
      if (!user || typeof user !== 'object') return '?'
      
      // Try first name + last name
      if (user.firstName && user.firstName.length > 0 && user.lastName && user.lastName.length > 0) {
        return `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()
      }
      
      // Try just first name
      if (user.firstName && user.firstName.length > 0) {
        return user.firstName[0].toUpperCase()
      }
      
      // Try username
      if (user.username && user.username.length > 0) {
        return user.username[0].toUpperCase()
      }
      
      // Try email
      if (user.email && user.email.length > 0) {
        return user.email[0].toUpperCase()
      }
      
      // Fallback
      return '?'
    },

    getUserDisplayName(user) {
      if (!user || typeof user !== 'object') return 'Unknown User'
      
      // Try first name + last name
      if (user.firstName && user.lastName) {
        return `${user.firstName} ${user.lastName}`
      }
      
      // Try just first name
      if (user.firstName) {
        return user.firstName
      }
      
      // Try username
      if (user.username) {
        return user.username
      }
      
      // Try email
      if (user.email) {
        return user.email
      }
      
      return 'Unknown User'
    },

    formatDate(dateString) {
      try {
        if (!dateString) return 'Unknown'
        return new Date(dateString).toLocaleDateString()
      } catch (error) {
        console.error('Error formatting date:', error)
        return 'Unknown'
      }
    },

    openCreateModal() {
      this.isEditing = false
      this.currentUser = null
      this.resetForm()
      this.showModal = true
    },

    editUser(user) {
      if (!user || !user.id) {
        console.error('Invalid user object for editing:', user)
        return
      }
      
      this.isEditing = true
      this.currentUser = user
      this.form = {
        username: user.username || '',
        email: user.email || '',
        password: '',
        firstName: user.firstName || '',
        lastName: user.lastName || ''
      }
      this.showModal = true
    },

    closeModal() {
      this.showModal = false
      this.resetForm()
    },

    resetForm() {
      this.form = {
        username: '',
        email: '',
        password: '',
        firstName: '',
        lastName: ''
      }
      this.modalErrorMessage = ''
    },

    async handleSubmit() {
      this.modalErrorMessage = ''
      this.submitting = true

      try {
        const token = localStorage.getItem('token')
        if (!token) {
          this.$router.push('/login')
          return
        }
        
        const headers = { Authorization: `Bearer ${token}` }

        if (this.isEditing && this.currentUser) {
          await axios.put(`/api/users/${this.currentUser.id}`, {
            username: this.form.username,
            email: this.form.email,
            firstName: this.form.firstName,
            lastName: this.form.lastName
          }, { headers })
          this.showSuccessMessage('User updated successfully!')
        } else {
          await axios.post('/api/users', this.form, { headers })
          this.showSuccessMessage('User created successfully!')
        }

        this.closeModal()
        await this.loadUsers()
        
      } catch (error) {
        console.error('Error saving user:', error)
        this.modalErrorMessage = error.response?.data?.message || 
          (this.isEditing ? 'Failed to update user' : 'Failed to create user')
      } finally {
        this.submitting = false
      }
    },

    async deleteUser(id) {
      if (!id) {
        console.error('No user ID provided for deletion')
        return
      }
      
      if (!confirm('Are you sure you want to delete this user?')) return

      try {
        const token = localStorage.getItem('token')
        if (!token) {
          this.$router.push('/login')
          return
        }
        
        await axios.delete(`/api/users/${id}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        
        this.showSuccessMessage('User deleted successfully!')
        await this.loadUsers()
        
      } catch (error) {
        console.error('Error deleting user:', error)
        this.errorMessage = error.response?.data?.message || 'Failed to delete user'
      }
    },

    showSuccessMessage(message) {
      this.successMessage = message
      setTimeout(() => {
        this.successMessage = ''
      }, 3000)
    }
  }
}
</script>
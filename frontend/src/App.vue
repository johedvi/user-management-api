<template>
  <div id="app" class="min-h-screen bg-gray-50">
    <nav v-if="isLoggedIn" class="bg-white shadow border-b border-gray-200">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-xl font-bold text-gray-900">User Management</h1>
          <div class="flex items-center space-x-4">
            <router-link 
              to="/users" 
              class="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
            >
              Users
            </router-link>
            <button 
              @click="handleLogout"
              class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-md text-sm font-medium"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </nav>
    
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <router-view />
    </main>
  </div>
</template>

<script>
export default {
  data() {
    return {
      isLoggedIn: !!localStorage.getItem('token')
    }
  },
  methods: {
    handleLogout() {
      localStorage.removeItem('token')
      this.$router.push('/login')
    }
  },
  watch: {
    $route() {
      // Update login status when route changes
      this.isLoggedIn = !!localStorage.getItem('token')
    }
  }
}
</script>
<template>
  <div class="container">
    <nav class="nav">
      <a href="/users">Users</a>
      <a href="/login">Login</a>
      <button v-if="isLoggedIn" @click="logout">Logout</button>
    </nav>
    <router-view />
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
    logout() {
      localStorage.removeItem('token')
      this.isLoggedIn = false
      this.$router.push('/login')
    }
  }
}
</script>
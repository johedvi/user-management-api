<template>
  <div>
    <h2>Users</h2>
    
    <!-- Add User Form -->
    <div class="form">
      <h3>Add New User</h3>
      <input v-model="newUser.username" placeholder="Username"><br>
      <input v-model="newUser.email" placeholder="Email"><br>
      <input v-model="newUser.password" placeholder="Password" type="password"><br>
      <button @click="addUser">Add User</button>
    </div>
    
    <!-- Users List -->
    <div class="users">
      <h3>All Users</h3>
      <div v-for="user in users" :key="user.id" class="user">
        <strong>{{ user.username }}</strong> - {{ user.email }}
        <button @click="deleteUser(user.id)" style="float: right; background: red;">Delete</button>
      </div>
    </div>
    
    <p v-if="message">{{ message }}</p>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  data() {
    return {
      users: [],
      newUser: {
        username: '',
        email: '',
        password: ''
      },
      message: ''
    }
  },
  async mounted() {
    await this.loadUsers()
  },
  methods: {
    async loadUsers() {
      try {
        const token = localStorage.getItem('token')
        const response = await axios.get('/api/users', {
          headers: { Authorization: `Bearer ${token}` }
        })
        this.users = response.data
      } catch (error) {
        this.message = 'Failed to load users'
        if (error.response.status === 401) {
          this.$router.push('/login')
        }
      }
    },
    async addUser() {
      try {
        const token = localStorage.getItem('token')
        await axios.post('/api/users', this.newUser, {
          headers: { Authorization: `Bearer ${token}` }
        })
        this.message = 'User added successfully!'
        this.newUser = { username: '', email: '', password: '' }
        await this.loadUsers()
      } catch (error) {
        this.message = 'Failed to add user: ' + error.response.data.message
      }
    },
    async deleteUser(id) {
      if (!confirm('Are you sure?')) return
      
      try {
        const token = localStorage.getItem('token')
        await axios.delete(`/api/users/${id}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        this.message = 'User deleted successfully!'
        await this.loadUsers()
      } catch (error) {
        this.message = 'Failed to delete user'
      }
    }
  }
}
</script>
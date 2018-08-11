<template>
  <div id="app">
    <v-app>
      <v-navigation-drawer app fixed clipped></v-navigation-drawer>
      <v-toolbar app clipped-left>
        <span class="title ml-3 mr-5">SnooNotes</span>
        <v-spacer></v-spacer>
        <login></login>
      </v-toolbar>
      <div id="nav">
        <router-link to="/">Home</router-link> |
        <router-link to="/about">About</router-link>
      </div>
      <router-view/>
    </v-app>
  </div>
</template>
<script>
import Login from './components/Login.vue'
export default {
  components: {
    'login': Login
  }
}
</script>

<style lang="stylus">
#app
  font-family 'Avenir', Helvetica, Arial, sans-serif
  -webkit-font-smoothing antialiased
  -moz-osx-font-smoothing grayscale
  text-align center
  color #2c3e50

#nav
  padding 30px
  a
    font-weight bold
    color #2c3e50
    &.router-link-exact-active
      color #42b983
</style>

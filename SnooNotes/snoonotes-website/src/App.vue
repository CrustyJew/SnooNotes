<template>
  <v-app>
    <v-toolbar
      app
      :clipped-left="clipped"
      color="primary"
    >
      <v-toolbar-title to="/">SnooNotes!</v-toolbar-title>
      
      <v-spacer></v-spacer>
      <v-menu offset-y>
      </v-menu>
    </v-toolbar>
    <v-content>
      <router-view/>
    </v-content>
    <main>
      <v-container fluid>
        <v-fade-transition mode="out in">
          <router-view></router-view>
        </v-fade-transition>
      </v-container>
    </main>
    <v-footer :fixed="fixed" app>
      <span>&copy; 2017</span>
    </v-footer>
  </v-app>
</template>

<script>

export default {
  name: 'App',
  data() {
    return {
      clipped: false,
      drawer: true,
      fixed: false,
      items: [{
        icon: 'bubble_chart',
        title: 'Inspire',
      }],
      miniVariant: false,
      right: true,
      rightDrawer: false,
      title: 'Vuetify.js',
    };
  },
};
</script>
<style lang="stylus">
.v-toolbar__title{
  cursor pointer
}
</style>

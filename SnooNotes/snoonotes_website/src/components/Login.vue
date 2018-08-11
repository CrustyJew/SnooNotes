<template>
<div class="text-xs-center">

    <v-menu offset-y v-if="username">
        <v-btn flat slot="activator" color="primary">Welcome {{username}}!&nbsp;<v-icon>keyboard_arrow_down</v-icon></v-btn>
        <v-list>
            <v-list-tile @click="logout">
                <v-list-tile-title>Logout</v-list-tile-title>
            </v-list-tile>
        </v-list>
    </v-menu>
    <v-btn v-else color="info">Login</v-btn>
</div>
</template>
<script>
export default {
  data: () => ({
    username: ''
  }),
  methods: {
    logout: function () {}
  }
}
</script>
<style lang="stylus" scoped>
</style>

<template>
<span>[test: {{username}} {{subreddit}}]</span>
</template>
<script>
    export default {
        name: 'user-notes',
        props:['username','subreddit','type','thingid'],
        data(){
            return{
             message: this.username,
            }
        },
    }
</script>
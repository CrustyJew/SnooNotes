<template>
<span>[test: {{username}} {{subreddit}} {{hasNotes}}]</span>
</template>
<script>
import {store} from '../redux/contentScriptStore';
    export default {
        name: 'user-notes',
        props:['username','subreddit','type','thingid'],
        data(){
            return{
             message: this.username,
            }
        },
        computed:{
            hasNotes: function() {
                return this.$select('snoonotes_info.users_with_notes').indexOf(this.username) > -1;
            }
        }
    }
</script>
<template>
  <div>
    <button v-on:click="testClick">Login</button>
    <h1>{{message}}</h1>
    <p>{{user.isLoadingUser}}</p>
  </div>
</template>

<script>
  
  import {login} from './redux/actions/user';
  import {store} from './redux/contentScriptStore';
    export default{
        name: 'test',
        data(){
            return{
             message: "omg omg omgoahsmdfsjdlasdffafaaaf",
             user: this.$select('user')
            }
        },
        methods:{
          testClick(){
            store.dispatch(login());
          }
        }
    }
</script>
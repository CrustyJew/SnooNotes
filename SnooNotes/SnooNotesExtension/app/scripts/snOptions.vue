<template>
    <span>
        <a id="sn-login" @click="login" v-if="!user.name">{{user.isLoadingUser ? 'loading...' : 'Login'}}</a>
        <a id="sn-show-options" v-if="user.name" @click="showOptions = true">SN Options</a>
        <sn-options-modal :show.sync="showOptions" :on-close.sync="closeModal"></sn-options-modal>
    </span>
</template>
<script>
import {login} from './redux/actions/user';
import {store} from './redux/contentScriptStore';
import SNOptionsModal from './snOptionsModal.vue';
export default {
    name: 'sn-options',
    components: {'sn-options-modal': SNOptionsModal},
    data(){
        return {
            user: this.$select('user'),
            showOptions: false
        }
    },
    methods:{
        login(){
            store.dispatch(login());
        },
        closeModal(){
            this.showOptions = false;
        }
    }
}
</script>
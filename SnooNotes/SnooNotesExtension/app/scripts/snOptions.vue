<template>
    <span>
        <a id="sn-login" @click="login" v-if="!user.name">{{user.isLoadingUser ? 'loading...' : 'Login'}}</a>
        <a id="sn-show-options" v-if="user.name" @click="openOptions">SN Options</a>
        <sn-options-modal :show.sync="showOptions" :sn-options.sync="snOptions" :on-close.sync="closeModal"></sn-options-modal>
    </span>
</template>
<script>
import {login} from './redux/actions/user';
import {store} from './redux/contentScriptStore';
import SNOptionsModal from './snOptionsModal.vue';
import axios from 'axios';
export default {
    name: 'sn-options',
    components: {'sn-options-modal': SNOptionsModal},
    data(){
        return {
            user: this.$select('user'),
            showOptions: false,
            snOptions: {loading:true,inactiveSubs:[]}
        }
    },
    methods:{
        login(){
            store.dispatch(login());
        },
        closeModal(){
            this.showOptions = false;
        },
        openOptions(){
            this.snOptions.loading = true;
            this.showOptions = true;

            axios.get('Account/GetInactiveModeratedSubreddits')
                .then(response => {this.snOptions.inactiveSubs = response.data; this.snOptions.loading = false;});
        }
    }
}
</script>
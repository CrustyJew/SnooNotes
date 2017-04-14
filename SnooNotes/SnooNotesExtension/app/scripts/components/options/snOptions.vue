<template>
    <span id="SNOptions">
        <a id="sn-login" @click="login" v-if="user.isLoadingUser">{{user.isLoadingUser ? 'loading...' : 'Login'}}</a>
        <a id="sn-show-options" v-if="!user.isLoadingUser" @click="openOptions">SN Options</a>
        <sn-options-modal v-if="showOptions" :show.sync="showOptions" :on-close.sync="closeModal"></sn-options-modal>
    </span>
</template>
<script>
import {login} from '../../redux/actions/user';
import {store} from '../../redux/contentScriptStore';
import SNOptionsModal from './snOptionsModal.vue';
import axios from 'axios';
export default {
    name: 'sn-options',
    components: {'sn-options-modal': SNOptionsModal},
    data(){
        return {
            user: this.$select('user'),
            showOptions: false,
            snOptions: {
                loadingInactiveSubs:true,
                loadingSubSettings:true,
            }
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
            this.snOptions.loadingInactiveSubs = true;
            this.snOptions.loadingSubSettings = true;
            this.showOptions = true;

        }
    }
}
</script>
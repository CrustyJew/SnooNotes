<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div id="SNOptionsContainer" class="modal-body">
            <div v-if="!snOptions.loading">
                <div id="SNRefreshContainer">
                    <h1>Has something gone rogue?<br />Change subreddits you moderate?<br />Activate a new sub?</h1>
                    <button type="button" id="SNRestart" class="SNBtnWarn">Refresh SnooNotes</button>    
                <br class="clearfix" />
                </div>
                <div id="SNActivateContainer">
                    <select id="SNActivateSub" v-model="activateSubName">
                        <option value="-1">---Activate a new Subreddit---</option>
                        <option v-for="sub in snOptions.inactiveSubs" v-bind:value="sub">{{sub}}</option>
                    </select>
                    <button type="button" id="SNBtnActivateSub" class="SNBtnSubmit" @click="activateSub">Activate</button>
                <br class="clearfix" />
                </div>
            </div>
            <div v-if="snOptions.loading">
                <sn-loading></sn-loading>
            </div>
        </div>
    </modal>
</template>
<script>
import SNModal from './snModal.vue';
import LoadingSpinner from './components/loadingSpinner.vue';
import axios from 'axios';

export default {
    components:{'modal': SNModal, 'sn-loading': LoadingSpinner},
    props: ['show','onClose', 'snOptions'],
    data(){
        return {
            activateSubName: "-1"
        }
    },
    methods:{
        close(){
            this.onClose();
        },
        activateSub(){
            if(this.activateSubName && this.activateSubName != "-1"){
                axios.post('subreddit',{
                    subName:this.activateSubName
                });
            }
        }
    },
    name:'sn-options-modal'
}
</script>
<style lang="scss">
#SNRefreshContainer{
    width:510px;
    margin:0 auto;
    height:74px;
    h1{
        float:left;
        font-size: 18px;
        line-height: 18px;
    }
}
#SNActivateContainer{
    margin:0 auto;
    width:310px;
}
#SNActivateSub {
    height: 32px;
    border-radius: 5px;
    option:first-of-type {
        color: darkgrey;
    }
}

#SNBtnActivateSub{
    margin-right:0px;
}

#SNRestart{
    margin-top:20px;
    margin-left:15px;
}
#SNOptionsContainer {
    margin-top: -15px;
    padding-top: 5px;
    min-height: 100px;
    max-height: 1000px;
    min-width: 100px;
    overflow: auto;
    width: 100%;
    box-sizing: border-box;
}
</style>
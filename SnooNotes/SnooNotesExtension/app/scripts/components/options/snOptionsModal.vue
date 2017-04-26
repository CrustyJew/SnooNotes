<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div id="SNOptionsContainer" class="modal-body">
    
            <div id="SNRefreshContainer">
                <h1>Has something gone rogue?<br />Change subreddits you moderate?<br />Activate a new sub?</h1>
                <button type="button" id="SNRestart" class="SNBtnWarn" @click="refresh">Refresh SnooNotes</button>
                <br class="clearfix" />
            </div>
            <div id="SNActivateContainer">
                <div v-if="!snOptions.loadingInactiveSubs">
                    <select id="SNActivateSub" v-model="activateSubName">
                        <option value="-1" disabled>---Activate a new Subreddit---</option>
                        <option v-for="sub in snOptions.inactiveSubs" v-bind:value="sub">{{sub}}</option>
                    </select>
                    <button type="button" id="SNBtnActivateSub" class="SNBtnSubmit" @click="activateSub" :disabled="activatingSub">{{activatingSub ? 'Activating...': 'Activate'}}</button>
                    <br class="clearfix" />
                </div>
                <div v-show="snOptions.loadingInactiveSubs">
                    <sn-loading></sn-loading>
                </div>
            </div>
            <div id="SNModSubs">
                <h3>Subreddits you have permissions to submit notes in</h3>
                <ul>
                    <li v-for="sub in snInfo.modded_subs">{{sub.SubName}}</li>
                </ul>
            </div>
            <div id="SNSubSettingsContainer">
                <div v-if="snOptions.loadingSubSettings">
                    <sn-loading></sn-loading>
                </div>
                <div v-if="!snOptions.loadingSubSettings && editingSubIndex == -1">
                    <div class="SNSubSettingsBtnWrapper" v-for="(sub,index) in snOptions.subSettings">
                        <button type="button" class="SNBtnAction" @click="showSettings(index)" >/r/{{sub.SubName}}</button>
                    </div>
                </div>
                <div v-if="!snOptions.loadingSubSettings && editingSubIndex > -1">
                    <sn-sub-options :sub="snOptions.subSettings[editingSubIndex]" :cancel="cancelSubOptSave" :finish="getSubSettings"></sn-sub-options>
                </div>
            </div>
        </div>
    </modal>
</template>
<script>
import SNModal from '../snModal.vue';
import LoadingSpinner from '../loadingSpinner.vue';
import SNSubOptions from './snSubOptions.vue';
import axios from 'axios';
import { store } from '../../redux/contentScriptStore';
import { refreshUser } from '../../redux/actions/user';
import Toasted from 'vue-toasted';

export default {
    components: { 'modal': SNModal, 'sn-loading': LoadingSpinner, 'sn-sub-options': SNSubOptions },
    props: ['show', 'onClose'],
    data() {
        return {
            activateSubName: "-1",
            editingSubIndex: -1,
            snOptions: {
                loadingInactiveSubs: true,
                loadingSubSettings: true
            },
            activatingSub: false,
            user: this.$select('user'),
            snInfo: this.$select('snoonotes_info as snInfo')
        }
    },
    methods: {
        close() {
            this.editingSubIndex = -1;
            this.activateSubName = "-1";
            this.onClose();
        },
        activateSub() {
            if (this.activateSubName && this.activateSubName != "-1") {
                axios.post('subreddit', {
                    subName: this.activateSubName
                }).then(()=>{
                    store.dispatch(refreshUser(true));
                    this.activatingSub = false;
                });
            }
        },
        cancelSubOptSave() {
            this.editingSubIndex = -1;
        },
        showSettings(index) {
            this.editingSubIndex = index;

        },
        getSubSettings() {
            this.snOptions.loadingSubSettings = true;
            this.editingSubIndex = -1;
            this.snOptions.subSettings = {};
            axios.get('Subreddit/admin')
                .then(response => { this.snOptions.subSettings = response.data; this.snOptions.loadingSubSettings = false; });
        },
        getInactiveSubs() {
            this.activateSubName = "-1";
            this.snOptions.loadingInactiveSubs = true;
            this.snOptions.inactiveSubs = [];
            axios.get('Account/GetInactiveModeratedSubreddits')
                .then(response => { this.snOptions.inactiveSubs = response.data; this.snOptions.loadingInactiveSubs = false; });
        },
        refresh() {
            store.dispatch(refreshUser(true))
                .then(() => {
                    this.$toasted.success("Getting a new freakin token!!");
                    this.$root.$emit('refresh');
                    this.close();
                });
        }
    },
    watch: {
        'user.isLoadingUser': function(val, oldVal){
            if(!oldVal && val){
                setTimeout(()=> {
                    this.getSubSettings();
                    this.getInactiveSubs();
                }, 100);
            }
        }
    },
    created: function () {
        this.getSubSettings();
        this.getInactiveSubs();
    },
    name: 'sn-options-modal'
}
</script>
<style lang="scss">
#SNRefreshContainer {
    width: 510px;
    margin: 0 auto;
    height: 74px;
    text-align: left;
    h1 {
        float: left;
        font-size: 18px;
        line-height: 18px;
    }
}

#SNActivateContainer {
    margin: 0 auto;
    width: 310px;
    min-height:50px;
}

#SNModSubs{
    margin:0 auto;
    width: 400px;
    margin-bottom:5px;
    h3{
        margin-bottom: 10px;
    }
    ul{
        columns: 3;
    }
}

#header {
    //force header index higher for options modal
    z-index: 2147483646;
}

.sn-loading {
    margin: 0 auto;
}

#SNActivateSub {
    height: 32px;
    border-radius: 5px;
    option:first-of-type {
        color: darkgrey;
    }
}

#SNBtnActivateSub {
    margin-right: 0px;
}

#SNRestart {
    margin-top: 20px;
    margin-left: 15px;
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

.SNSubSettingsBtnWrapper {
    width: 250px;
    padding: 20px 20px 0px 20px;
    margin: 0 auto;
    display: inline-block;
    text-align: center;
}
</style>
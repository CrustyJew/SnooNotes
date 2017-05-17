<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div id="sn-options-sidebar">
            <div id="sn-options-subopts" class="sn-options-category" :class="{active: activeTab == 'options'}" @click="activeTab = 'options'" key="options">Subreddits</div>
            <div id="sn-options-banlist" class="sn-options-category" :class="{active: activeTab == 'banlist'}" @click="activeTab='banlist'" key="banlist">Bot Ban List</div>
        </div>
    
        <div id="sn-options-panel">
            <transition name="fade" mode="out-in">
                <div id="sn-options-container" v-if="activeTab == 'options'" key="subOptContainer">
    
                    <div id="sn-refresh-container">
                        <h1>Has something gone rogue?
                            <br />Change subreddits you moderate?
                            <br />Activate a new sub?</h1>
                        <button type="button" id="sn-restart" class="sn-btn-warn" @click="refresh">Refresh SnooNotes</button>
                        <br class="clearfix" />
                    </div>
                    <div id="sn-activate-container">
                        <div v-if="!snOptions.loadingInactiveSubs">
                            <select id="sn-activate-sub" v-model="activateSubName">
                                <option value="-1" disabled>---Activate a new Subreddit---</option>
                                <option v-for="sub in snOptions.inactiveSubs" v-bind:value="sub">{{sub}}</option>
                            </select>
                            <button type="button" id="sn-btn-activate-sub" class="sn-btn-submit" @click="activateSub" :disabled="activatingSub">{{activatingSub ? 'Activating...': 'Activate'}}</button>
                            <br class="clearfix" />
                        </div>
                        <div v-show="snOptions.loadingInactiveSubs">
                            <sn-loading></sn-loading>
                        </div>
                    </div>
                    <div id="sn-mod-subs">
                        <h3>Subreddits you have permissions to submit notes in</h3>
                        <ul>
                            <li v-for="sub in snInfo.modded_subs">{{sub.SubName}}<span v-if="sub.SentinelActive">&nbsp;<i class="material-icons">visibility</i><md-tooltip md-direction="right" md-delay="400">TheSentinelBot Active</md-tooltip></span></li>
                        </ul>
                    </div>
                    <div id="sn-sub-settings-container">
                        <div v-if="snOptions.loadingSubSettings">
                            <sn-loading></sn-loading>
                        </div>
                        <div v-if="!snOptions.loadingSubSettings && editingSubIndex == -1">
                            <div class="sn-sub-settings-btn-wrapper" v-for="(sub,index) in snOptions.subSettings">
                                <button type="button" class="sn-btn-action" @click="showSettings(index)">/r/{{sub.SubName}}</button>
                            </div>
                        </div>
                        <div v-if="!snOptions.loadingSubSettings && editingSubIndex > -1">
                            <sn-sub-options :sub="snOptions.subSettings[editingSubIndex]" :cancel="cancelSubOptSave" :finish="getSubSettings"></sn-sub-options>
                        </div>
                    </div>
                </div>
    
                <banned-users v-if="activeTab == 'banlist'" key="banContainer"></banned-users>
            </transition>
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
import BannedUsersTable from '../bannedUsersTable.vue';

export default {
    components: { 'modal': SNModal, 'sn-loading': LoadingSpinner, 'sn-sub-options': SNSubOptions, 'banned-users': BannedUsersTable },
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
            snInfo: this.$select('snoonotes_info as snInfo'),
            activeTab: 'options'
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
                }).then(() => {
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
        'user.isLoadingUser': function (val, oldVal) {
            if (!oldVal && val) {
                setTimeout(() => {
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
@import "~styles/_vars.scss";

#sn-options #sn-modal {
    min-width: 810px;
    line-height: 12px;
    text-transform: none !important;
    color: initial;
    font-size: 12px;

    a {
        color: $secondary;
    }
    a.sn-close-modal {
        display: inline-block;
        cursor: pointer;
        color: white;
        background-color: $accent;
        font-weight: bold;
        padding: 2px 10px;
        margin-top: -1px;
        &:hover {
            background-image: linear-gradient(to bottom, darken($accent, 10%), darken($accent, 25%));
        }
    }
    #sn-options-sidebar {
        width: 125px;
        background-color: $light-gray;
        padding-left: 5px;
        padding-top: 15px;
        height: calc(100% - 15px);
        box-sizing: border-box;
        position: relative;
        border-right: 1px solid $dark-gray;
        float: left;
    }

    .sn-options-category {
        width: 113px;
        height: 25px;
        background-color: $secondary;
        padding: 5px;
        box-sizing: border-box;
        margin-bottom: 5px;
        float: right;
        font-weight: bold;
        cursor: pointer;
        border-radius: 5px 0 0 5px;
        border: 1px solid $dark-gray;
        border-right: none;
        position: relative;
        color: white;
        transition: background-color linear .2s;
    }

    .sn-options-category.active {
        background-color: darken($secondary, 20%);
    }

    .sn-options-category.active:after {
        content: ' ';
        height: 0;
        position: absolute;
        width: 0;
        border: 10px solid transparent;
        border-right-width: 25px;
        border-right-color: white;
        right: -1px;
        top: 50%;
        margin-top: -10px;
    }

    #sn-options-panel {
        height: calc(100% - 15px);
        display: block;
        margin-left: 125px;
        padding-left: 20px;
        padding-top: 10px;
        padding-right: 20px;
        overflow: auto;
        box-sizing: border-box;
    }

    #sn-refresh-container {
        width: 525px;
        margin: 0 auto;
        height: 74px;
        text-align: left;
        h1 {
            float: left;
            font-size: 18px;
            line-height: 18px;
        }
    }

    #sn-activate-container {
        margin: 0 auto;
        width: 310px;
        min-height: 50px;
    }

    #sn-mod-subs {
        margin: 0 auto;
        width: 400px;
        margin-bottom: 5px;
        h3 {
            margin-bottom: 10px;
        }
        ul {
            columns: 3;
        }
        li i.material-icons{
            font-size:18px;
            vertical-align: middle;
            color:$secondary;
        }
    }

    #header {
        //force header index higher for options modal
        z-index: 2147483646;
    }

    .sn-loading {
        margin: 0 auto;
    }

    #sn-activate-sub {
        height: 32px;
        border-radius: 5px;
        option:first-of-type {
            color: darkgrey;
        }
    }

    #sn-btn-activate-sub {
        margin-right: 0px;
    }

    #sn-restart {
        margin-top: 20px;
        margin-left: 15px;
    }

    #sn-options-container {
        margin-top: -15px;
        padding-top: 5px;
        min-height: 100px;
        max-height: 1000px;
        min-width: 585px;
        overflow: auto;
        width: 100%;
        box-sizing: border-box;
    }

    .sn-sub-settings-btn-wrapper {
        width: 250px;
        padding: 20px 20px 0px 20px;
        margin: 0 auto;
        display: inline-block;
        text-align: center;
    }
}
</style>
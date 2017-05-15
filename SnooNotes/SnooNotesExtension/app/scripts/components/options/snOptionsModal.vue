<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div id="SNOptionsSidebar">
            <div id="SNOptionsSubOpts" class="SNOptionsCategory" :class="{active: activeTab == 'options'}" @click="activeTab = 'options'" key="options">Subreddits</div>
            <div id="SNOptionsPlaceholder1" class="SNOptionsCategory" :class="{active: activeTab == 'banlist'}" @click="activeTab='banlist'" key="banlist">Bot Ban List</div>
        </div>
    
        <div id="SNOptionsPanel">
            <transition name="fade" mode="out-in">
                <div id="SNOptionsContainer" v-if="activeTab == 'options'" key="subOptContainer">
    
                    <div id="SNRefreshContainer">
                        <h1>Has something gone rogue?
                            <br />Change subreddits you moderate?
                            <br />Activate a new sub?</h1>
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
                                <button type="button" class="SNBtnAction" @click="showSettings(index)">/r/{{sub.SubName}}</button>
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

#SNOptions #SNModal {
    min-width: 810px;
    line-height: 12px;
    text-transform: none !important;
    color: initial;
    font-size: 12px;

    a {
        color: $secondary;
    }
    a.SNCloseModal {
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
    #SNOptionsSidebar {
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

    .SNOptionsCategory {
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

    .SNOptionsCategory.active {
        background-color: darken($secondary, 20%);
    }

    .SNOptionsCategory.active:after {
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

    #SNOptionsPanel {
        height: calc(100% - 15px);
        display: block;
        margin-left: 125px;
        padding-left: 20px;
        padding-top: 10px;
        padding-right: 20px;
        overflow: auto;
        box-sizing: border-box;
    }

    #SNOptionsContents {
        margin: 0 auto;
        max-width: 1000px;
        padding-bottom: 20px;
        padding-right: 20px;
    }


    #SNRefreshContainer {
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

    #SNActivateContainer {
        margin: 0 auto;
        width: 310px;
        min-height: 50px;
    }

    #SNModSubs {
        margin: 0 auto;
        width: 400px;
        margin-bottom: 5px;
        h3 {
            margin-bottom: 10px;
        }
        ul {
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
        min-width: 585px;
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
}
</style>
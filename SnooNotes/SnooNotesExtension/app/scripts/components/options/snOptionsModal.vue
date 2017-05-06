<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div id="SNOptionsSidebar">
            <div id="SNOptionsSubOpts" class="SNOptionsCategory" :class="{active: activeTab == 'options'}" @click="activeTab = 'options'" key="options">Subreddits</div>
            <div id="SNOptionsPlaceholder1" class="SNOptionsCategory" :class="{active: activeTab == 'banlist'}" @click="activeTab='banlist'" key="banlist">Bot Ban List</div>
        </div>
        <transition-group name="fade">
        <div id="SNOptionsPanel" v-if="activeTab == 'options'" key="optPanel">
            <div id="SNOptionsContainer">
    
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
                            <button type="button" class="SNBtnAction" @click="showSettings(index)">/r/{{sub.SubName}}</button>
                        </div>
                    </div>
                    <div v-if="!snOptions.loadingSubSettings && editingSubIndex > -1">
                        <sn-sub-options :sub="snOptions.subSettings[editingSubIndex]" :cancel="cancelSubOptSave" :finish="getSubSettings"></sn-sub-options>
                    </div>
                </div>
            </div>
        </div>
        <div id="SNBanListPanel"  v-if="activeTab == 'banlist'" key="banPanel">
            <md-table-card>
  <md-toolbar>
    <h1 class="md-title">Nutrition</h1>
    <md-button class="md-icon-button">
      <md-icon>filter_list</md-icon>
    </md-button>

    <md-button class="md-icon-button">
      <md-icon>search</md-icon>
    </md-button>
  </md-toolbar>

  <md-table-alternate-header md-selected-label="selected">
    <md-button class="md-icon-button">
      <md-icon>delete</md-icon>
    </md-button>

    <md-button class="md-icon-button">
      <md-icon>more_vert</md-icon>
    </md-button>
  </md-table-alternate-header>

  <md-table md-sort="calories">
    <md-table-header>
      <md-table-row>
        <md-table-head md-sort-by="dessert">Dessert (100g serving)</md-table-head>
        <md-table-head md-sort-by="type" width="100px">Type</md-table-head>
        <md-table-head md-sort-by="calories" md-numeric md-tooltip="The total amount of food energy and the given serving size">Calories (g)</md-table-head>
        <md-table-head md-sort-by="fat" md-numeric>Fat (g)</md-table-head>
        <md-table-head>
          <md-icon>message</md-icon>
          <span>Comments</span>
        </md-table-head>
      </md-table-row>
    </md-table-header>

    <md-table-body>
      <md-table-row v-for="(row, rowIndex) in nutrition" :key="rowIndex" :md-item="row" md-selection>
        <md-table-cell v-for="(column, columnIndex) in row" :key="columnIndex" :md-numeric="columnIndex !== 'dessert' && columnIndex !== 'comment' && columnIndex !== 'type'">
          <span v-if="columnIndex === 'comment'">{{ column }}</span>

          <md-button class="md-icon-button" v-if="columnIndex === 'comment'">
            <md-icon>edit</md-icon>
          </md-button>

          <md-select
            placeholder="Type"
            :name="'type' + columnIndex"
            :id="'type' + columnIndex"
            v-model="nutrition[rowIndex].type"
            v-if="columnIndex === 'type'">
            <md-option value="ice_cream">Ice Cream</md-option>
            <md-option value="pastry">Pastry</md-option>
            <md-option value="other">Other</md-option>
          </md-select>

          <span v-if="columnIndex !== 'type' && columnIndex !== 'comment'">{{ column }}</span>
        </md-table-cell>
      </md-table-row>
    </md-table-body>
  </md-table>
</md-table-card>
        </div>
        </transition-group>
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
            nutrition:[{"dessert":"Frozen yogurt","type":"ice_cream","calories":"159","fat":"6.0","comment":"Icy"},{"dessert":"Ice cream sandwich","type":"ice_cream","calories":"237","fat":"9.0","comment":"Super Tasty"},{"dessert":"Eclair","type":"pastry","calories":"262","fat":"16.0","comment":""},{"dessert":"Cupcake","type":"pastry","calories":"305","fat":"3.7","comment":""},{"dessert":"Gingerbread","type":"other","calories":"356","fat":"16.0","comment":""}],
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
    height: 100%;
    display: block;
    margin-left: 125px;
    padding-left: 20px;
    padding-top: 10px;
    padding-right: 20px;
    overflow: auto;
    margin-bottom: -25px;
}

#SNOptionsContents {
    margin: 0 auto;
    max-width: 1000px;
    padding-bottom: 20px;
    padding-right: 20px;
}


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
</style>
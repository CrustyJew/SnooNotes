<template>
    <div>
        <div class="sn-media-analysis-display" v-if="showAnalysis" :style="displayStyle" v-draggable="'.sn-header'">
            <div class="sn-header">
                <a class="sn-close-note sn-close" @click="selfClose">X</a>
            </div>
            <h1>/r/{{subreddit}}</h1>
            <div class="sn-media-analysis">
                <div class="sn-sentinel-ban-info">
                    <div class="sn-loading" v-if="loadingBanned">
                        <sn-loading></sn-loading>
                    </div>
                    <div class="sn-error" v-else-if="bannedChannelError">
                        There was an error determining if there was a banned channel for this thing. Try again later or yell at
                    <a href="https://reddit.com/u/meepster23" target="_blank">/u/meepster23</a>.
                    </div>
                    <div v-else>
                        <h2>Sentinel Bot Banned Media</h2>
                        <h3 v-if="sentinelBanInfo.length == 0">No banned media channels detected!</h3>
                        <table v-else>
                            <thead>
                                <tr>
                                    <td>Channel Name</td>
                                    <td class="sn-media-platform">Media Platform</td>
                                    <td class="sn-blacklist-by">Blacklisted By</td>
                                    <td class="sn-blacklist-on">Blacklisted On</td>
                                    <td class="sn-global-ban">Global Ban</td>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="ban in sentinelBanInfo" :key="ban.mediaChannelID + '|' + ban.mediaPlatform">
                                    <td class="sn-media-author">
                                        <a :href="'https://layer7.solutions/blacklist/reports/#type=channel&subject='+ban.mediaChannelID" target="_blank">{{ban.mediaAuthor}}</a>
                                    </td>
                                    <td>{{ban.mediaPlatform}}</td>
                                    <td>{{ban.blacklistBy}}</td>
                                    <td class="sn-date">{{new Date(ban.blacklistDateUTC).toLocaleString().replace(', ', '\n')}}</td>
                                    <td class="sn-global-ban">
                                        <i class="material-icons" v-if="ban.globalBan">gavel</i>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
    
                <div class="sn-loading" v-if="loadingAnalysis">
                    <sn-loading></sn-loading>
                </div>
                <div class="sn-error" v-if="(error) && !loadingAnalysis">There was an error loading the analysis for this thing. Try again later or yell at
                    <a href="https://reddit.com/u/meepster23" target="_blank">/u/meepster23</a>.
                </div>
    
                <div class="sn-media-analysis-results" v-if="!error && !loadingAnalysis && analysisResponse.subName">
                    <div v-if="analysisResponse">
                        <h2>Analsyis results for
                            <a :href="analysisResponse.permaLink" target="_blank">{{analysisResponse.thingType}}</a> by
                            <a :href="'https://reddit.com/u/'+analysisResponse.author" target="_blank">/u/{{analysisResponse.author}}</a>
                        </h2>
    
                        <div class="sn-thing-details">
                            {{thingDetailsMessage}}
                        </div>
                        <div v-for="analysis in analysisResponse.analysis" class="sn-analysis-media-results" :key="analysis.mediaPlatform + '|' + analysis.mediaID">
                            <h3>Media Platform: {{analysis.mediaPlatform}} | Media ID: {{analysis.mediaID}} | Channel:
                                <a :href="'https://layer7.solutions/blacklist/reports/#type=channel&subject='+analysis.mediaChannelID" target="_blank">{{analysis.mediaChannelName}}</a>
                            </h3>
                            <table>
                                <thead>
                                    <tr>
                                        <td class="sn-module">Module Name</td>
                                        <td class="sn-score">Score</td>
                                        <td class="sn-reason">Reason</td>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="score in analysis.scores" :key="score">
                                        <td class="sn-module">{{score.module}}</td>
                                        <td class="sn-score">{{score.score}}</td>
                                        <td class="sn-reason">{{score.reason}}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
                <div v-else-if="!error && !loadingAnalysis">
                    <h2>No analysis results found for this thing.. Sorry!</h2>
                </div>
                <div class="sn-retry" v-if="!loadingAnalysis">
                    <button type="button" class="sn-btn-action" @click="loadAnalysis">Reload/Retry</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import LoadingSpinner from './loadingSpinner.vue';
import { draggable } from './directives/draggable';
import axios from 'axios';
import { dirtbagBaseUrl } from '../config';

export default {
    directives: { 'draggable': draggable },
    components: { 'sn-loading': LoadingSpinner },
    props: ['showAnalysis', 'subreddit', 'thingid', 'displayStyle', 'close'],
    data: function () {
        return {

            loadingAnalysis: false,
            loadingBanned: false,
            sentinelBanInfo: [],
            analysisResponse: {},

            error: false,
            bannedChannelError: false
        }
    },
    methods: {
        selfClose: function () {
            document.removeEventListener('click', this.clickWatch, true);
            this.close();
        },
        clickWatch: function (e) {
            if (!this.$el.contains(e.target)) this.selfClose();
        },
        loadAnalysis: function () {
            this.error = false;
            this.loadingAnalysis = true;
            this.loadingBanned = true;
            this.bannedChannelError = false;

            axios.get(dirtbagBaseUrl + 'Analysis/' + this.subreddit, { params: { thingID: this.thingid } })
                .then((d) => {
                    this.analysisResponse = d.data;
                    this.$nextTick(() => {

                        this.loadingAnalysis = false;
                    })
                }, (e) => { this.error = true; this.loadingAnalysis = false; })

            axios.get(dirtbagBaseUrl + 'SentinelBan/' + this.subreddit + '/' + this.thingid)
                .then((d) => {
                    this.sentinelBanInfo = d.data;
                    this.$nextTick(() => {
                        this.loadingBanned = false;
                    });
                }, () => {
                    this.loadingBanned = false;
                    this.bannedChannelError = true;
                });
        }
    },
    computed: {
        watchChange: function () {
            return this.subreddit, this.thingid, this.showAnalysis, Date.now();
        },
        thingDetailsMessage: function () {
            let msg = "";
            if (!this.analysisResponse) return msg;
            if (this.analysisResponse.action == "None") {
                msg += "No action was taken on this " + this.analysisResponse.thingType;

            }
            else {
                msg += "This " + this.analysisResponse.thingType + " was " + (this.analysisResponse.action == "Remove" ? "REMOVED" : "REPORTED");
            }
            msg += " with a " + (this.analysisResponse.thingType == "Comment" ? "high " : "") + "score of " + this.analysisResponse.highScore;
            return msg;
        }
    },
    watch: {
        watchChange() {
            if (this.showAnalysis) {
                this.loadAnalysis()
            }
        },
        showAnalysis: function (val, oldVal) {
            if (!oldVal && val) {
                document.addEventListener('click', this.clickWatch, true);
            }
            else if (oldVal && !val) {
                document.removeEventListener('click', this.clickWatch, true);
            }
        }

    },
    beforeDestroy: function () {
        document.removeEventListener('click', this.clickWatch, true);
    }
}
</script>

<style lang="scss">
@import "~styles/_vars.scss";
.sn-media-analysis-display {
    .sn-header {
        margin-left: -10px;
        margin-right: -10px;
        margin-bottom: 10px;
        .sn-close {
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
    }
    h1 {
        font-size: 24px;
        font-weight: bold;
        color: $primary;
    }
    h2 {
        font-size: 18px;
        color: $secondary;
        a {
            color: black;

            &:hover {
                text-decoration: underline;
            }
        }
    }
    h3 {
        a {
            color: $secondary;

            &:hover {
                text-decoration: underline;
            }
        }
    }
    .sn-sentinel-ban-info {
        margin-bottom: 10px;

        .sn-date {
            white-space: pre;
        }
        .sn-media-author {
            font-weight: bold;
        }
        table {
            margin-top: 5px;

            thead {
                font-weight: bold;
            }
        }
        .sn-media-platform {
            width: 100px;
        }
        .sn-blacklist-on {
            width: 75px;
        }
        .sn-global-ban {
            width: 50px;
        }
        td {
            text-align: center;
            i {
                cursor: default;
                color: $accent;
            }
            a {
                color: $secondary;
                &:hover {
                    text-decoration: underline;
                }
            }
        }
    }
    .sn-media-analysis-results {
        .sn-thing-details {
            font-size: 18px;
            margin: 12px 0px;
        }
        .sn-analysis-media-results {
            border: 1px solid $light-gray;
            border-radius: 10px;
            padding: 10px;
            box-shadow: 2px 2px 5px 0px $dark-gray;
            h3 {
                font-size: 12px;
                margin-bottom: 10px;
            }
            thead {
                font-weight: bold;
            }
            table {
                .sn-module {
                    width: 150px;
                }
                .sn-score {
                    width: 150px;
                }
            }
        }
    }
    .sn-error {
        padding: 5px;
        border-radius: 5px;
    }
    .sn-retry {
        margin-top: 15px;
    }
}
</style>

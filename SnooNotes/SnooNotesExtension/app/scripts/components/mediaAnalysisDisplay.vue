<template>
    <div class="sn-media-analysis-display" v-if="showAnalysis" :style="displayStyle" v-draggable="'.sn-header'">
        <div class="sn-header">
            <a class="sn-close-note sn-close" @click="selfClose">X</a>
        </div>
        <div class="sn-media-analysis">
            <div class="sn-loading" v-if="loadingAnalysis">
                <sn-loading></sn-loading>
            </div>
            <div class="sn-error" v-if="error">There was an error loading the analysis for this thing. Try again later or yell at
                <a href="https://reddit.com/u/meepster23" target="_blank">/u/meepster23</a>.
            </div>
            <div class="sn-media-analysis-results" v-if="!error && !loadingAnalysis">
                {{this.analysis}}
            </div>
            <div class="sn-retry" v-if="!loadingAnalysis">
                <button type="button" class="sn-btn-action" @click="loadAnalysis">Reload/Retry</button>
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
    props:['showAnalysis','subreddit','thingid','displayStyle', 'close'],
    data: function () {
        return {

            loadingAnalysis: false,

            analysis: {},

            error: false
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

            axios.get(dirtbagBaseUrl + 'Analysis/' + this.subreddit, { thingID: this.thingid })
                .then((d) => {
                    this.analysis = d;
                    this.loadingAnalysis = false;
                }, (e) => { this.error = true; this.loadingAnalysis = false; })
        }
    },
    computed:{
        watchChange: function(){
            return this.subreddit, this.thingid, this.showAnalysis, Date.now();
        }
    },
    watch:{
        watchChange(){
            this.loadAnalysis()
        },
        showAnalysis: function(val,oldVal){
            if(!oldVal && val){
                document.addEventListener('click',this.clickWatch,true);
            }
            else if(oldVal  && !val){
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
}
</style>

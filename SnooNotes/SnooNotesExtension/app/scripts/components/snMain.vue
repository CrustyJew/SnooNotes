<template>
    <div id="sn-main">
        <!--<note-display></note-display>-->
        <div>
            <!--<note-thing v-for="thingid in thingIDs" :thingid="thingid"></note-thing>-->
            <media-analysis v-for="thingid in thingIDs" :thingid="thingid" :subs="mediaAnalysisSubs" :key="thingid" v-on:showMediaAnalysis="showMediaAnalysis"></media-analysis>
            <media-analysis-display :show-analysis="mediaAnalysis.showAnalysis" :subreddit="mediaAnalysis.subreddit" :thingid="mediaAnalysis.thingid" :display-style="mediaAnalysis.displayStyle" :close.sync="closeMediaAnalysis"></media-analysis-display>
        </div>
    </div>
</template>
<script>
//import { getNotesForUsers } from './redux/actions/notes';
import mediaAnalysis from './mediaAnalysis.vue';
import mediaAnalysisDisplay from './mediaAnalysisDisplay.vue';
import _ from 'lodash';

export default {
    components: { 'media-analysis': mediaAnalysis, 'media-analysis-display': mediaAnalysisDisplay },
    data() {
        return {
            thingIDs: [],
            modSubs: this.$select('snoonotes_info.modded_subs as modSubs'),
            mediaAnalysis: {
                showAnalysis: false,
                subreddit: "",
                thingid: "",
                displayStyle: {
                    display: 'none',
                    position: 'absolute',
                    top: '0px',
                    right: '0px',
                },
            }
        }
    },
    methods: {
        showMediaAnalysis: function (args) {
            this.mediaAnalysis.subreddit = args.subreddit;
            this.mediaAnalysis.thingid = args.thingid;
            this.mediaAnalysis.displayStyle.top = args.event.pageY + 5 + 'px';
            this.mediaAnalysis.displayStyle.left = args.event.pageX + 15 + 'px'

            this.mediaAnalysis.displayStyle.display = 'block';
            this.mediaAnalysis.showAnalysis = true;
            document.addEventListener('click', this.clickWatch, true);
        },
        closeMediaAnalysis: function(){
            this.mediaAnalysis.showAnalysis = false;
        }
    },
    computed: {
        mediaAnalysisSubs: function () {
            return this.modSubs.filter((s) => { return s.SentinelActive }).map((s) => { return s.SubName });
        }
    },
    mounted: function () {
        this.$on('AddThings', (e) => {
            this.thingIDs = this.thingIDs.concat(e.thingIDs);
        });
        this.$on('RemoveThings', (e) => {
            this.thingIDs = _.without(this.thingIDs, e.thingIDs);
        })
    }
}
</script>

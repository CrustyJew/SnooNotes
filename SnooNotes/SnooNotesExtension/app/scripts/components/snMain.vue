<template>
    <div id="sn-main">
        <div>
            <media-analysis-display ref="mediaAnalysisDisplay"></media-analysis-display>
            <note-display ref="noteDisplay"></note-display>
            <action-hist-display ref="actionHistoryDisplay"></action-hist-display>
        </div>
    </div>
</template>
<script>
import mediaAnalysis from "./mediaAnalysis.vue";
import mediaAnalysisDisplay from "./mediaAnalysisDisplay.vue";
import userNoteDisplay from "./userNotesDisplay.vue";
import actionHistoryDisplay from "./actionHistoryDisplay.vue";
import _ from "lodash";

export default {
  components: {
    "media-analysis": mediaAnalysis,
    "media-analysis-display": mediaAnalysisDisplay,
    "note-display": userNoteDisplay,
    "action-hist-display": actionHistoryDisplay
  },
  data() {
    return {
      thingIDs: [],
      modSubs: this.$select("snoonotes_info.modded_subs as modSubs"),
      mediaAnalysis: {
        showAnalysis: false,
        subreddit: "",
        thingid: "",
        displayStyle: {
          display: "none",
          position: "absolute",
          top: "0px",
          right: "0px"
        }
      }
    };
  },
  methods: {
    closeMediaAnalysis: function() {
      this.mediaAnalysis.showAnalysis = false;
    },
    injectNewThing: function(author, subreddit, url, node, thing, event) {
      this.$refs.noteDisplay.processNewThing(author, subreddit, url, node);
      this.$refs.mediaAnalysisDisplay.processNewThing(
        author,
        subreddit,
        url,
        node,
        thing,
        event
      );
      this.$refs.actionHistoryDisplay.processNewThing(
        author,
        subreddit,
        url,
        node,
        thing,
        event
      );
    }
  },
  computed: {
    mediaAnalysisSubs: function() {
      return Object.keys(this.modSubs).filter((key) => this.subreddits[key].SentinelActive)
      // return this.modSubs
      //   .filter(s => {
      //     return s.SentinelActive;
      //   })
      //   .map(s => {
      //     return s.SubName;
      //   });
    }
  },
  mounted: function() {
    this.$on("AddThings", e => {
      this.thingIDs = this.thingIDs.concat(e.thingIDs);
    });
    this.$on("RemoveThings", e => {
      this.thingIDs = _.without(this.thingIDs, e.thingIDs);
    });
  }
};
</script>

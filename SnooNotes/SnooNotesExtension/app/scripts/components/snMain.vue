<template>
    <div id="sn-main">
        <div>
            <media-analysis-display ref="mediaAnalysisDisplay" :show-analysis="mediaAnalysis.showAnalysis" :subreddit="mediaAnalysis.subreddit" :thingid="mediaAnalysis.thingid" :display-style="mediaAnalysis.displayStyle" :close.sync="closeMediaAnalysis"></media-analysis-display>
            <note-display ref="noteDisplay"></note-display>
        </div>
    </div>
</template>
<script>
import { mediaProviders } from '../config';
import mediaAnalysis from "./mediaAnalysis.vue";
import mediaAnalysisDisplay from "./mediaAnalysisDisplay.vue";
import userNoteDisplay from "./userNotesDisplay.vue";
import _ from "lodash";

export default {
  components: {
    "media-analysis": mediaAnalysis,
    "media-analysis-display": mediaAnalysisDisplay,
    "note-display": userNoteDisplay
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
      this.$refs.noteDisplay.injectNewUserNotesComponent(
        author,
        subreddit,
        url,
        node
      );
      this.checkForMedia(author,subreddit,url,node,thing,event);
      //this.$refs.mediaAnalysisDsiplay.injectNewMediaAnalysisComponent(author, subreddit, url, node);
    },
    checkForMedia: function(author, subreddit, url, node, thing, event) {
      if (event) {
        //jsapi event driven / redesign
      } else {
        //old reddit
        let thingid = thing.attributes["data-fullname"].value;
        if (thing.attributes['data-subreddit'] && (thing.attributes['data-type'].value == 'link')) {
            //link submission or self post
            this.subreddit = thing.attributes['data-subreddit'].value;
            
            let domain = thing.attributes['data-domain'].value.toLowerCase();
            if (mediaProviders.findIndex(mp => mp == domain) > -1) {
                this.$refs.mediaAnalysisDisplay.injectNewMediaAnalysisComponent(author, subreddit, url, node,thing);
            }
            else {
                if (thing.classList.contains('self')) {
                    let childarray = [...thing.children];
                    let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
                    let expando = entry.querySelector('.expando');
                    if(!expando){
                        //no expando, just a self post with no body, ignore.
                        return;
                    }
                    else if (!expando.classList.contains('expando-unitialized')) {
                        //expando already loaded, process as normal
                        if(this.checkText(expando.querySelector('.usertext-body'))){
                            this.$refs.mediaAnalysisDsiplay.injectNewMediaAnalysisComponent(author, subreddit, url, node);
                        }
                    }
                    else {
                        //expando not loaded, observe and wait
                        var obs = new MutationObserver(_.bind(function (mutations) {
                            mutations.forEach((mutation) => {
                                if (!mutation.target.classList.contains('expando-unitialized')) {
                                    obs.disconnect();
                                    this.checkText(mutation.target.querySelector('.usertext-body'));
                                }
                            })
                        }, this));
                        obs.observe(expando, { attributes: true });
                    }
                }
                else {
                    //not in media providers and not a self post
                    this.hasMedia = false;
                }
            }
        }
        else if (thing.attributes['data-subreddit']) {
            //comment or message
            this.subreddit = thing.attributes['data-subreddit'].value;
            let childarray = [...thing.children];
            let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
            this.checkText(entry.querySelector('.usertext-body'));
        }
      }
    },
    checkText: function (el) {
            el.querySelectorAll('a').forEach(_.bind((link) => {
                let hostname = link.hostname.toLowerCase();
                if (hostname.startsWith('www.')) {
                    hostname = hostname.substring(4, hostname.length);
                }
                if (mediaProviders.findIndex(mp => mp == hostname) > -1) {
                    this.hasMedia = true;
                }
            }, this));
        }
  },
  computed: {
    mediaAnalysisSubs: function() {
      return this.modSubs
        .filter(s => {
          return s.SentinelActive;
        })
        .map(s => {
          return s.SubName;
        });
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

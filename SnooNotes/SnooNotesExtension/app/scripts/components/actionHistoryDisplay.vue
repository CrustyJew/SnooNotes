<template>
  <transition name="fade">
      <div v-if="showHist" class="sn-action-history-display" :style="displayStyle" v-draggable="'.sn-header'">
            <div class="sn-header">
                <a class="sn-close-note sn-close" @click="selfClose">X</a>
            </div>
            <h1>Mod Log:</h1>
            <div class="sn-action-history">
                <div class="sn-loading" v-if="loadingHist">
                    <sn-loading></sn-loading>
                </div>
                <div class="sn-error" v-else-if="error">
                        There was an error retrieving the mod action history for this thing. Try again later or yell at
                    <a href="https://reddit.com/u/meepster23" target="_blank">/u/meepster23</a>.
                </div>
                <div v-else>
                    <h3 v-if="actions.length == 0">Caught the mods sleepin. No actions!</h3>
                    <table v-else>
                        <thead>
                            <tr>
                                <td class="sn-action-hist-time sn-table-header">Time</td>
                                <td class="sn-action-hist-mod sn-table-header">Mod</td>
                                <td class="sn-action-hist-action sn-table-header">Action</td>
                                <td class="sn-action-reason sn-table-header">Action Reason</td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="act in actions" :key="act.id">
                                <td>{{new Date(act.Timestamp).toLocaleString()}}</td>
                                <td>{{act.Mod}}</td>
                                <td>{{act.ModAction}}</td>
                                <td>{{act.ActionReason}}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
      </div>
  </transition>
</template>

<script>
import Vue from "vue";
import { draggable } from "./directives/draggable";
import ActionHistory from "./actionHistory.vue";
import LoadingSpinner from "./loadingSpinner.vue";
import axios from "axios";
import { apiBaseUrl } from "../config";
export default {
  name: "action-history-display",
  //props: ['username', 'subreddit', 'url', 'showNotes'],
  directives: { draggable: draggable },
  components: { "sn-loading": LoadingSpinner },
  data() {
    return {
      showHist: false,
      subreddit: "",
      thingid: "",
      displayStyle: {
        display: "none",
        position: "absolute",
        top: "0px",
        right: "0px"
      },
      loadingHist: false,
      error: false,
      actions: []
    };
  },
  methods: {
    selfClose: function() {
      document.removeEventListener("click", this.clickWatch, true);
      this.showHist = false;
    },
    clickWatch: function(e) {
      if (!this.$el.contains(e.target)) this.selfClose();
    },
    showActionHistory: function(args) {
      this.subreddit = args.subreddit;
      this.thingid = args.thingid;
      this.displayStyle.top = args.event.pageY + 5 + "px";
      this.displayStyle.left = args.event.pageX + 15 + "px";

      this.displayStyle.display = "block";
      this.showHist = true;
      document.addEventListener("click", this.clickWatch, true);
      this.loadHist();
    },
    loadHist: function() {
      this.error = false;
      this.loadingHist = true;

      axios
        .get(apiBaseUrl + "ModAction/" + this.subreddit + "/thing/" + this.thingid)
        .then(
          d => {
            this.actions = d.data;
            this.$nextTick(() => {
              this.loadingHist = false;
            });
          },
          e => {
            this.error = true;
            this.loadingHist = false;
          }
        );
    },
    processNewThing: function(author, subreddit, url, node, thing, event) {
      if (event) {
        //jsapi event driven / redesign
      } else {
        let histElem = document.createElement("action-history");
        histElem.setAttribute(
          "thingid",
          thing.attributes["data-fullname"].value
        );
        histElem.setAttribute("subreddit", subreddit);
        node.appendChild(histElem);
        new Vue({
          components: { "action-history": ActionHistory },
          parent: this
        }).$mount(histElem);
      }
    }
  },
  beforeDestroy: function() {
    document.removeEventListener("click", this.clickWatch, true);
  }
};
</script>

<style lang="scss">
@import "~styles/_vars.scss";
.sn-action-history-display {
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
        background-image: linear-gradient(
          to bottom,
          darken($accent, 10%),
          darken($accent, 25%)
        );
      }
    }
  }
  .sn-table-header{
    font-weight: bold;
  }
}
</style>
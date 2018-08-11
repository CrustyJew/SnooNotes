<template>
    <span v-if="activeSub" class="sn-action-history" @click.stop>
        <i class="material-icons" @click="showActionHist">update</i>
    </span>
    <span v-else class="sn-no-action-hist"></span>
</template>
<script>
export default {
  props: ["thingid", "subreddit"],
  data() {
    return {
      curSub: this.$select(
        "snoonotes_info.modded_subs." +
          this.subreddit.toLowerCase() +
          " as curSub"
      )
    };
  },
  methods: {
    showActionHist: function(e) {
      this.$parent.$parent.showActionHistory({
        subreddit: this.subreddit,
        thingid: this.thingid,
        event: e
      });
    }
  },
  computed: {
    activeSub: function() {
      return this.curSub && this.curSub.SentinelActive;
    }
  }
};
</script>
<style lang="scss">
.sn-action-history i {
  cursor: pointer;
  font-size:22px;
}
</style>

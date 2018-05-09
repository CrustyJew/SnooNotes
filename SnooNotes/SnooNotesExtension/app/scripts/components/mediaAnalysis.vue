<template>
    <span v-if="hasMedia && activeSub" class="sn-media-analysis" @click.stop>
        <i class="material-icons" @click="showAnalysis">visibility</i>
    </span>
    <span v-else class="sn-no-media"></span>
</template>
<script>
import { mediaProviders } from '../config';
import _ from 'lodash';
export default {
    props: ['thingid', 'subreddit'],
    data() {
        return {
            hasMedia: true,
            curSub: this.$select('snoonotes_info.modded_subs.'+this.subreddit.toLowerCase()+' as curSub')
        }
    },
    methods: {
        showAnalysis: function (e) {
            this.$parent.$parent.showMediaAnalysis( { subreddit: this.subreddit, thingid: this.thingid, event: e });
        }

    },
    computed:{
        activeSub: function(){
            return this.curSub && this.curSub.SentinelActive;
        }
    },
    mounted: function () {
        /*let analysisElement = this.$el;
        this.$el.parentNode.removeChild(this.$el);
        let thing = window.document.querySelector('#thing_' + this.thingid);

        let childarray = [...thing.children];
        let entries = childarray.filter((c) => { return c.classList.contains('entry') });
        let entry = entries.length > 0 ? entries[0] : null;
        let authElem = entry ? entry.querySelector('.tagline a.author') : null;
        if (authElem && authElem.classList.contains('moderator')) authElem = null;
        let author = null;
        if (!authElem) return;
        authElem.parentNode.insertBefore(analysisElement, authElem.nextSibling);
        if (thing.attributes['data-subreddit'] && (thing.attributes['data-type'].value == 'link')) {
            //link submission or self post
            this.subreddit = thing.attributes['data-subreddit'].value;
            
            let domain = thing.attributes['data-domain'].value.toLowerCase();
            if (mediaProviders.findIndex(mp => mp == domain) > -1) {
                this.hasMedia = true;
            }
            else {
                if (thing.classList.contains('self')) {
                    let childarray = [...thing.children];
                    let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
                    let expando = entry.querySelector('.expando');
                    if(!expando){
                        //no expando, just a self post with no body, ignore.
                    }
                    else if (!expando.classList.contains('expando-unitialized')) {
                        //expando already loaded, process as normal
                        this.checkText(expando.querySelector('.usertext-body'));
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
        */
    }
}

</script>
<style lang="scss">
.sn-media-analysis i {
    cursor: pointer;
}
</style>

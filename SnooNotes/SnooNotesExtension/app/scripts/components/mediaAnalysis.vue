<template>
    <span v-if="hasMedia">
        !!!</span>
</template>
<script>
import { mediaProviders } from '../config';
import _ from 'lodash';
export default {
    props: ['thingid'],
    data() {
        return {
            hasMedia: false,
            subreddit: ''
        }
    },
    methods: {
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
    mounted: function () {
        let analysisElement = this.$el;
        this.$el.parentNode.removeChild(this.$el);
        let thing = window.document.querySelector('#thing_' + this.thingid);

        let authElem = thing.querySelector('.thing > .entry > .tagline a.author');
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
                    if (!expando.classList.contains('expando-unitialized')) {
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
    }
}

</script>

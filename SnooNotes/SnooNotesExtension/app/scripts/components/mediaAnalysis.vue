<template>
    <span v-if="hasMedia">!!!</span>
</template>
<script>
import { mediaProviders } from '../config';
export default {
    props: ['thingid'],
    data() {
        return new {
            hasMedia: false
        }
    },
    methods: {
        checkText:function(el){
            el.querySelectorAll('').map((link)=>{
                let hostname = link.hostname.toLowerCase();
                if(hostname.startsWith('www.')){
                    hostname = hostname.substring(4,hostname.length);
                }
                if (mediaProviders.contains(hostname)){
                    this.hasMedia = true;
                }
            })
        } 
        
    },
    mounted:function() {
        let analysisElement = this.$el;
        this.$el.parentNode.removeChild(this.$el);
        let thing = body.document.querySelector('#thing_' + this.thingid);

        let authElem = thing.querySelector('.thing > .entry > .tagline a.author');
        if (authElem && authElem.classList.contains('moderator')) authElem = null;
        authElem.parentNode.insertBefore(analysisElement, authElem.nextSibling);
        if (thing.attributes['data-subreddit'] && (thing.attributes['data-type'].value == 'link')) {
            let domain = thing.attributes['data-domain'].value;
            if (mediaProviders.contains(domain.toLowerCase())) {
                this.hasMedia = true;
            }
            else {
                if (thing.classList.contains('self')) {
                    let childarray = [...thing.children];
                    let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
                    let expando = entry.querySelector('.expando');
                    var obs = new MutationObserver(function (mutations) {
                        mutations.forEach((mutation)=>{
                            if(!mutation.target.classList.contains('expando-unitialized')){
                                obs.disconnect();
                                this.checkText(mutation.target.querySelector('.usertext-body'));
                            }
                        })
                    });
                }
                this.hasMedia = false;
            }
        }
        else {
            let childarray = [...thing.children];
            let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
            let permlink = entry.querySelector('a.bylink').attributes['data-href-url'].value;
            let postid = permlink.substr(permlink.indexOf('comments/') + 9, 6); //post id is after comments/
            let commentRootURL = 'https://reddit.com/r/' + thing.attributes['data-subreddit'].value + '/comments/' + postid + '/.../';
            url = commentRootURL + thing.attributes['data-fullname'].value.replace('t1_', '');
        }
    }
}

</script>

import { on } from '../utilities/onEventDelegate.js';
import axios from 'axios';

export class SentinelBanModule {
    constructor() {
        this.eventListenerUnbinds = [];
        this.subreddits = [];
        this.userBanEnabled = false;
    }

    initModule() {
        if (document.body.matches('body.comments-page,body.listing-page')) {
            //only bind on listing and comment pages to avoid extra listeners
            this.eventListenerUnbinds.push(on(document.body, 'click', '.remove-button .main > a', (e) => { this.vanillaRemove(e) }));
            this.eventListenerUnbinds.push(on(document.body, 'click', '.SNBotBanPrompt a', (e) => { this.executeBan(e) }));
        }
    }

    refreshModule(subs, hasConfig) {
        subs.forEach((sub)=>{
            this.subreddits.push({name: sub.SubName, isAdmin: sub.IsAdmin, hasSentinel: sub.SentinelActive});
        })

        this.userBanEnabled = hasConfig;
    }

    unbindModule() {
        let unbind = this.eventListenerUnbinds.pop();
        do {
            unbind();
            unbind = this.eventListenerUnbinds.pop();
        } while (unbind)
    }

    vanillaRemove(event) {

        let target = event.srcElement || event.target;
        let action = target.closest('form').querySelector('input[name="executed"]').value;
        let thing = target.closest('.thing');
        let oldElem = target.closest('ul').querySelector('.SNBotBanPrompt.SNBotBan' + action);
        if (oldElem) oldElem.remove();
        let user = thing.attributes['data-author'].value;
        let sub = thing.attributes['data-subreddit'].value;

        if (!this.userBanEnabled && (this.subreddits.length == 0 || this.subreddits.findIndex(sr => sr.name.toLowerCase() == sub.name.toLowerCase()) == -1))
            //if no user bans and the sub isn't in sentinel bot, don't render anything.
            return;

        let domain = thing.attributes['data-domain'];
        domain = domain ? domain.value.toLowerCase() : '';
        let url = '';
        if (domain && (domain == 'youtube.com' || domain == 'youtu.be' || domain == 'm.youtube.com')) {
            url = thing.attributes['data-url'].value;
        }

        let thingURL = "";
        if (thing.attributes['data-type'].value == 'link' || thing.attributes['data-type'].value == 'message') {
            thingURL = "https://reddit.com/r/" + thing.attributes['data-subreddit'].value + '/' + thing.attributes['data-fullname'].value.replace('t3_', '');
        }
        else {
            let commentRootURL = 'https://reddit.com/r/' + thing.attributes['data-subreddit'].value + '/comments/' + thing.closest('.nestedlisting').id.replace('siteTable_t3_', '') + '/.../';
            thingURL = commentRootURL + thing.attributes['data-fullname'].value.replace('t1_', '');
        }
        let banElem = this.getBanElement(sub, user, url, action, thingURL);
        if (banElem) {
            let injectElem = document.createElement('li');
            injectElem.className = 'SNBotBanPrompt SNBotBan' + action;
            injectElem.appendChild(banElem);

            target.closest('ul').appendChild(injectElem);
        }
    }

    executeBan(event) {
        let target = event.srcElement || event.target;

        let sub = target.attributes['sn-bb-sub'].value;
        let user = target.attributes['sn-bb-user'];
        user = user ? user.value : '';
        let channel = target.attributes['sn-bb-channel'];
        channel = channel ? channel.value : '';
        let reason = target.attributes['sn-bb-reason'].value;
        let thingurl = target.attributes['sn-bb-thingurl'].value;

        target.textContent = 'banning';
        if (channel) {
            axios.post('BotBan/' + sub + '/Channel', { ChannelURL: channel, BanReason: reason, ThingURL: thingurl })
                .then((d) => {
                    let el = document.createElement('span');
                    el.textContent = d.data == "True" ? 'Banned!' : 'Already Banned!';
                    target.replaceWith(el);
                }, () => {
                    target.textContent = 'Error!';
                })
        }
        else if (user) {
            axios.post('BotBan/' + sub + '/User', { UserName: user, BanReason: reason, ThingURL: thingurl })
                .then((d) => {
                   let el = document.createElement('span');
                    el.textContent = d.data == "True" ? 'Banned!' : 'Already Banned!';
                    target.replaceWith(el);
                }, () => {
                    target.textContent = 'Error!';
                })
        }
    }

    getBanElement(sub, user, url, reason, thingURL) {
        let banElem = document.createElement('span');
        banElem.className = 'SNSentinelBan';
        banElem.appendChild(document.createTextNode('Bot Ban (' + reason + '): '));
        let render = false;

        let index = this.subreddits.findIndex(sr=> sr.name.toLowerCase() == sub.name.toLowerCase());
        if(index <= -1 || !this.subreddits[index].isAdmin) return null;
        
        let subreddit = this.subreddits[index];

        if (this.userBanEnabled) {
            render = true;
            let userBanElem = document.createElement('a');
            userBanElem.setAttribute('sn-bb-sub', sub);
            userBanElem.setAttribute('sn-bb-user', user);
            userBanElem.setAttribute('sn-bb-reason', reason);
            userBanElem.setAttribute('sn-bb-thingurl', thingURL);
            userBanElem.textContent = 'User';
            banElem.appendChild(userBanElem);
        }
        if (url && subreddit.hasSentinel) {
            render = true;
            banElem.appendChild(document.createTextNode('\u00A0|\u00A0'));
            let chanBanElem = document.createElement('a');
            chanBanElem.setAttribute('sn-bb-sub', sub);
            chanBanElem.setAttribute('sn-bb-reason', reason);
            chanBanElem.setAttribute('sn-bb-thingurl', thingURL);
            chanBanElem.setAttribute('sn-bb-channel', url);
            chanBanElem.textContent = 'Channel';
            banElem.appendChild(chanBanElem);
        }
        if (render)
            return banElem;
        else
            return null;
    }
}
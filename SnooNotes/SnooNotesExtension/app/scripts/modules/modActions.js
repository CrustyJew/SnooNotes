
import axios from 'axios';

export class ModActionsModule {
    constructor() {
        this.modSubs = [];
    }

    initModule() {
        document.body.addEventListener('click', (e)=>{
            if(!e.target.matches('.big-mod-buttons > span > .pretty-button')) return;
            this.prettyAction(e)
        });
        chrome.runtime.onMessage.addListener((message)=>{
            if(message.method == "modAction") this.receiveModAction(message.req);
        });
    }

    refreshModule(subs) {
        this.modSubs = subs.map((s) => { return s.SubName });
    }

    prettyAction(e) {
        let btn = e.target;
        let thing = btn.closest('.thing');
        let sub = thing.attributes['data-subreddit'].value;
        let thingid = thing.attributes['data-fullname'].value;
        let reason = btn.attributes['data-event-action'];
        if (!reason) {
            let form = btn.closest('form');
            if(form){
                reason = form.querySelector('.option.main a').attr('data-event-action');
            }
            else{
                reason = btn.textContent;
            }
        }
        else {
            reason = reason.value;
        }

        this.reportAction(sub, thingid, reason);
    }

    reportAction(sub, thingid, reason) {
        if (this.modSubs.indexOf(sub) > -1) {
            axios.post('ModAction/' + sub, { ThingID: thingid, Action: reason });
        }
    }

    receiveModAction(req) {
        let thing = document.body.querySelector('#thing_' + req.thingID);
        if(!thing) return; //doesn't see thing on page
        let childarray = [...thing.children];
        let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
        let prevAction = entry.querySelector('.sn-mod-action');
        //remove previous action if there was one
        if (prevAction) prevAction.remove();

        if (req.action == "remove") {
            thing.classList.toggle('spam', true);
            thing.classList.toggle('removed', true);
            thing.classList.toggle('spammed', false);
            thing.classList.toggle('approved', false);
        }
        else if (req.action == 'spam') {
            thing.classList.toggle('spam', true);
            thing.classList.toggle('removed', false);
            thing.classList.toggle('spammed', true);
            thing.classList.toggle('approved', false);
        }
        else if (req.action == 'approve') {
            thing.classList.toggle('spam', false);
            thing.classList.toggle('removed', false);
            thing.classList.toggle('spammed', false);
            thing.classList.toggle('approved', true);
        }
        else {
            thing.classList.toggle('spam', false);
            thing.classList.toggle('removed', false);
            thing.classList.toggle('spammed', false);
            thing.classList.toggle('approved', false);
            thing.classList.toggle('sn-mod-action-warning', true);
        }

        let bigButtons = entry.querySelector('ul.flat-list .big-mod-buttons');
        let actionElem = document.createElement('span');
        actionElem.classList.add('sn-mod-action');
        actionElem.textContent = req.action + ' by ' + req.mod;
        if (bigButtons) {
            bigButtons.parentNode.insertBefore(actionElem, bigButtons.nextSibling);
        }
        else {
            let nodes = entry.querySelector('li');
            let lastItem = nodes[nodes.length - 1]; 
            lastItem.parentNode.insertBefore(actionElem, lastItem.nextSibling);
        }
    }
}
import { on } from '../utilities/onEventDelegate.js';
import axios from 'axios';

export class ModActionsModule {
    constructor() {
        this.modSubs = [];
    }

    initModule() {
        on(document.body, 'click', '.big-mod-buttons > span > .pretty-button', prettyAction)
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
            reason = btn.closest('form').querySelector('.option.main a').attr('data-event-action');
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
        let thing = document.body.querySelector('.id-' + req.thingID);
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

        let bigButtons = entry.querySelectorAll('ul.flat-list .big-mod-buttons');
        let actionElem = document.createElement('span');
        actionElem.classList.add('sn-mod-action');
        actionElem.textContent = req.action + ' by ' + req.mod;
        if(bigButtons.length > 0){
            bigButtons.parentNode.insertBefore(actionElem, bigButtons.nextSibling);
        }
        else{
            let lastItem = entry.querySelector('li:last');
            lastItem.parentNode.insertBefore(actionElem, lastItem.nextSibling);
        }
    }
}
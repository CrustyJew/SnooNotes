import axios from 'axios';

export class BanNotesModule {
    constructor(modsubs) {
        /*this.subreddits = modsubs.reduce((acc, sub) => {
            acc[sub.SubName] = { PermBanID: sub.Settings.PermBanID, TempBanID: sub.Settings.TempBanID };
            return acc;
        }, {});*/
        this.subreddits = modsubs;
    }

    initModule() {
        document.body.addEventListener('click', (e) => {
            if(!e.target.matches('.mod-popup .save')) return;
            let target = e.target;
            let popup = target.closest('div.mod-popup');
            let meta = popup.querySelector('div.meta');
            let sub = meta.querySelector('label.subreddit').textContent;
            let action = popup.querySelector('select.mod-action').value;

            if (action == "ban" && this.subreddits[sub]) {
                let dur = popup.querySelector('input.ban-duration').value;
                let type = dur ? this.subreddits[sub].TempBanID : this.subreddits[sub].PermBanID;

                if (!type || type < 0) return;

                let user = meta.querySelector('label.user').textContent;
                let thing = meta.querySelector('label.thing_id').textContent;
                let link = "";
                if (thing.startsWith('t3_')) {
                    //submission
                    link = 'https://reddit.com/r/' + sub + thing.replace('t3_', '/');
                }
                else if (thing.startsWith('t1_')) {
                    //comment
                    thing = document.querySelector('#thing_' + thing);
                    let childarray = [...thing.children];
                    let entry = childarray.filter((c) => { return c.classList.contains('entry') })[0];
                    let permlink = entry.querySelector('a.bylink').attributes['data-href-url'].value;
                    let postid = permlink.substr(permlink.indexOf('comments/') + 9, 6); //post id is after comments/
                    let commentRootURL = 'https://reddit.com/r/' + thing.attributes['data-subreddit'].value + '/comments/' + postid + '/.../';
                    link = commentRootURL + thing.attributes['data-fullname'].value.replace('t1_', '');
                }
                else if (thing.startsWith('t4_')) {
                    link = 'https://reddit.com/message/' + thing.replace('t4_', '');
                }
                else {
                    //hope it's new modmail
                    link = 'https://mod.reddit.com/mail/perma/' + thing;
                }

                let message = 'Note: ' + popup.querySelector('.ban-note').value + ' - '
                    + (dur ? dur + ' days' : 'Permanent') + '\n\n'
                    + 'Message: ' + popup.querySelector('.ban-message').value;

                axios.post('Note', { NoteTypeID: type, SubName: sub, Message: message, AppliesToUsername: user, Url: link })
                    .then(() => {
                    }, () => {
                        //console.error('Error saving ban note');
                    })
            }
        })
    }

    refreshModule(modsubs) {
        this.subreddits = modsubs;
    }
}


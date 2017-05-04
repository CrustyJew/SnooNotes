import Vue from 'vue'
import UserNotes from './components/userNotes.vue';
import SNOptions from './components/options/snOptions.vue';
import axios from 'axios';
import { SNAxiosInterceptor } from './utilities/snAxiosInterceptor';
import { apiBaseUrl } from './config';
import Toasted from 'vue-toasted';
import { reduxStore } from './redux/contentScriptStore';
import { getNotesForUsers } from './redux/actions/notes';
import { BanNotesModule } from './modules/banNotes';
import { SentinelBanModule } from './modules/sentinelBan';


export const snInterceptor = new SNAxiosInterceptor(reduxStore);
axios.defaults.baseURL = apiBaseUrl;
axios.interceptors.request.use((req) => { return snInterceptor.interceptRequest(req); });

Vue.use(Toasted, { position: 'bottom-right', duration: 2500 });

let usersWithNotes = [];
let requestedAuthors = [];
let newAuthorRequest = [];
const banNotesModule = new BanNotesModule([]);
const sentinelBanModule = new SentinelBanModule();

//dont start render until store is connected properly
const unsub = reduxStore.subscribe(() => {
    unsub();
    let state = reduxStore.getState();

    banNotesModule.refreshModule(state.snoonotes_info.modded_subs);
    banNotesModule.initModule();

    sentinelBanModule.refreshModule(state.snoonotes_info.modded_subs, state.user.hasConfig);
    sentinelBanModule.initModule();

    const options = new Vue({ render: h => h(SNOptions) }).$mount();
    options.$on('refresh', () => {
        let authorsReq = [];
        let things = document.querySelector('.thing');
        for (let i = 0; i < things.length; i++) {
            let authElem = things[i].querySelector('p.tagline a.author');
            if (authElem) {
                let author = things[i].attributes['data-author'].value;
                if (!author) {
                    author = authElem.textContent;
                }
                if (usersWithNotes.indexOf(author) != -1) {
                    authorsReq.push(author);
                }
            }
        }
        if (authorsReq.length > 0) {
            getNotesForUsers(reduxStore.dispatch, authorsReq);
        }

        //new modmail
        requestedAuthors = [];
        let authElems = document.body.querySelectorAll('a.Message__author,a.ThreadPreview__author');
        authElems.forEach(function (authElem) {
            let author = authElem.textContent.replace(/u\//, '');
            if (requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1 && usersWithNotes.indexOf(author) != -1) {
                newAuthorRequest.push(author);
            }
        });
        GetNotesForAuthors();
    })
    const userElem = document.querySelector('#header-bottom-right > .user');
    if (userElem) {
        userElem.parentNode.insertBefore(options.$el, userElem.nextSibling);
    }

    usersWithNotes = state.snoonotes_info.users_with_notes;
    reduxStore.subscribe(() => {
        usersWithNotes = reduxStore.getState().snoonotes_info.users_with_notes;
        sentinelBanModule.refreshModule(state.snoonotes_info.modded_subs, state.user.hasConfig);
        banNotesModule.refreshModule(state.snoonotes_info.modded_subs);
    });

    let authorsReq = InjectIntoThingsClass();
    authorsReq = concatUnique(authorsReq, InjectIntoUserPage());
    window.setTimeout(function () {
        InjectIntoNewModmail()
    }, 2000)
    if (authorsReq.length > 0) {
        getNotesForUsers(reduxStore.dispatch, authorsReq);
    }
})

const InjectIntoThingsClass = () => {
    let things = document.querySelectorAll('.thing');

    let target = document.body.querySelector('#siteTable');
    if (target) {
        let observer = new MutationObserver(
            function (mutations) {
                let authors = [];
                mutations.forEach(function (mutation) {
                    if (mutation.addedNodes && mutation.addedNodes.length > 0) {
                        let node = mutation.addedNodes[0];
                        if (node.querySelector) {
                            node.querySelectorAll('.thing').forEach(function (thing) {
                                let author = BindNewThingsClassUserNotesElement(thing);
                                if (usersWithNotes.indexOf(author) != -1 && authors.indexOf(author) == -1 && requestedAuthors.indexOf(author) == -1) {
                                    authors.push(author);
                                }
                            })
                        }
                    }
                    if (mutation.removedNodes && mutation.removedNodes.length > 0) {
                        let node = mutation.removedNodes[0];
                        if (node.querySelectorAll) {
                            node.querySelectorAll('.SNUserNotes').forEach(function (sn) {
                                sn.__vue__.$destroy();
                            });
                        }
                    }
                })
                GetNotesForAuthors(authors);
            }
        )
        observer.observe(target, { childList: true, subtree: true });

        let authors = [];
        for (let i = 0; i < things.length; i++) {
            let author = BindNewThingsClassUserNotesElement(things[i]);
            if (usersWithNotes.indexOf(author) != -1 && authors.indexOf(author) == -1) {
                authors.push(author);
            }

        }
        return authors;
    }
    return [];
}

const BindNewThingsClassUserNotesElement = (thing) => {
    let authElem = thing.querySelector('.thing > .entry > .tagline a.author');
    if (authElem && authElem.classList.contains('moderator')) authElem = null;
    let author = null;
    if (authElem) {
        author = thing.attributes['data-author'];
        if (!author || authElem.parentElement.classList.contains('recipient')) {
            author = authElem.textContent;
        }
        else {
            author = author.value;
        }
        let url = "";
        if (thing.attributes['data-type'].value == 'link' || thing.attributes['data-type'].value == 'message') {
            url = "https://reddit.com/r/" + thing.attributes['data-subreddit'].value + '/' + thing.attributes['data-fullname'].value.replace('t3_', '');
        }
        else {
            let commentRootURL = 'https://reddit.com/r/' + thing.attributes['data-subreddit'].value + '/comments/' + thing.closest('.nestedlisting').id.replace('siteTable_t3_', '') + '/.../';
            url = commentRootURL + thing.attributes['data-fullname'].value.replace('t1_', '');
        }

        let noteElem = document.createElement('user-notes');
        noteElem.setAttribute('username', author);
        noteElem.setAttribute('subreddit', thing.attributes['data-subreddit'].value);
        noteElem.setAttribute('url', url);
        authElem.parentNode.insertBefore(noteElem, authElem.nextSibling);
        new Vue({ components: { 'user-notes': UserNotes } }).$mount(authElem.parentNode);

    }
    thing.className = thing.className + ' SNDone';
    return author;
}


const InjectIntoUserPage = () => {
    let authors = [];
    let userHeader = document.querySelector('body.profile-page div.side div.titlebox>h1');
    if (userHeader) {
        if (usersWithNotes.indexOf(userHeader.textContent) != -1) {
            authors.push(userHeader.textContent)
        }
        let noteElem = document.createElement('user-notes');
        noteElem.setAttribute('username', userHeader.textContent);
        noteElem.setAttribute('url', 'https://reddit.com/u/' + userHeader.textContent)
        noteElem.setAttribute('class', 'SNUserpageNote');
        userHeader.parentNode.insertBefore(noteElem, userHeader.nextSibling);
        new Vue({ components: { 'user-notes': UserNotes } }).$mount(userHeader.parentElement);
    }
    return authors;
}

const InjectIntoNewModmail = () => {
    let target = document.querySelector('div.App');
    if (target) {
        let observer = new MutationObserver(
            function (mutations) {
                let newAuthorRequest = [];
                mutations.forEach(function (mutation) {
                    if (mutation.addedNodes && mutation.addedNodes.length > 0) {
                        let node = mutation.addedNodes[0];
                        if (node.tagName == "ARTICLE" && !node.classList.contains("SNDone")) {
                            let author = BindNewModmailUserNoteElement(mutation.addedNodes[0]);
                            if (usersWithNotes.indexOf(author) != -1 && requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1) {
                                newAuthorRequest.push(author);
                            }
                        }
                        if (node.tagName == "DIV") {
                            node.querySelectorAll('article:not(.SNDone)').forEach((article) => {
                                let author = BindNewModmailUserNoteElement(article);
                                if (usersWithNotes.indexOf(author) != -1 && requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1) {
                                    newAuthorRequest.push(author);
                                }
                            })
                        }
                    }
                    if (mutation.removedNodes && mutation.removedNodes.length > 0) {
                        let node = mutation.removedNodes[0];
                        if (node.querySelectorAll) {
                            node.querySelectorAll('.SNUserNotes').forEach(function (sn) {
                                sn.__vue__.$destroy();
                            });
                        }
                    }

                });
                GetNotesForAuthors(newAuthorRequest);
            });
        let newAuthorRequest = [];

        document.querySelectorAll('article').forEach(function (a) {
            let author = BindNewModmailUserNoteElement(a);
            if (usersWithNotes.indexOf(author) != -1 && requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1) {
                newAuthorRequest.push(author);
            }
        })
        GetNotesForAuthors(newAuthorRequest);

        observer.observe(target, { childList: true, subtree: true });
    }

}

const concatUnique = (array1, array2) => {
    let toReturn = [];
    toReturn = toReturn.concat(array1);
    for (let i = 0; i < array2.length; i++) {
        if (toReturn.indexOf(array2[i]) == -1) {
            toReturn.push(array2[i]);
        }
    }
    return toReturn;
}

const BindNewModmailUserNoteElement = (article) => {
    let authElems = article.querySelectorAll('a.Message__author:not(.m-mod),a.ThreadPreview__author:not(.m-mod)');
    let author = null;
    authElems.forEach(function (authElem) {
        author = authElem.textContent.replace(/u\//, '');

        let url = 'https://mod.reddit.com' + article.querySelector('.m-link,a.ThreadPreview__time').attributes['href'].value;
        let threadCommunity = document.querySelector('a.ThreadTitle__community');
        let sub = null;
        if (threadCommunity) sub = threadCommunity.textContent;
        let noteElem = document.createElement('span');
        noteElem.setAttribute('username', author);
        noteElem.setAttribute('subreddit', sub);
        noteElem.setAttribute('url', url);
        noteElem.setAttribute('is', 'user-notes');

        let vueinst = new Vue({ components: { 'user-notes': UserNotes } }).$mount(noteElem);
        authElem.parentNode.insertBefore(vueinst.$el, authElem.nextSibling);
    })
    article.className = article.className + ' SNDone';
    return author;
}

const GetNotesForAuthors = (authors) => {
    if (!authors || authors.length == 0) return;
    let thisReq = [].concat(authors);
    requestedAuthors = concatUnique(requestedAuthors, thisReq);
    getNotesForUsers(reduxStore.dispatch, thisReq);
}
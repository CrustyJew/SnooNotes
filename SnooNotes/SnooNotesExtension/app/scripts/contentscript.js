import Vue from 'vue';
import SNOptions from './components/options/snOptions.vue';
import axios from 'axios';
import {
  SNAxiosInterceptor
} from './utilities/snAxiosInterceptor';
import {
  apiBaseUrl
} from './config';
import Toasted from 'vue-toasted';
import {
  reduxStore
} from './redux/contentScriptStore';
import {
  getNotesForUsers
} from './redux/actions/notes';
import {
  BanNotesModule
} from './modules/banNotes';
import {
  SentinelBanModule
} from './modules/sentinelBan';
import {
  ModActionsModule
} from './modules/modActions';
import {
  jsapiListener
} from './SNListener';
import VueMaterial from 'vue-material';
import SNMain from './components/snMain.vue';

export const snInterceptor = new SNAxiosInterceptor(reduxStore);
axios.defaults.baseURL = apiBaseUrl;
axios.interceptors.request.use((req) => {
  return snInterceptor.interceptRequest(req);
});

let bodyobvs = new MutationObserver(function () {
  if (document.body.classList.contains("md-theme-default")) {
    document.body.classList.remove('md-theme-default');
  }

})
bodyobvs.observe(document.body, {
  attributes: true
});
// Vue.use(VueMaterial.MdCore);
// Vue.use(VueMaterial.MdIcon);
// Vue.use(VueMaterial.MdTable);
// Vue.use(VueMaterial.MdSelect);
// Vue.use(VueMaterial.MdButton);
Vue.use(VueMaterial);
Vue.use(Toasted, {
  position: 'bottom-right',
  duration: 2500
});

let usersWithNotes = [];
let requestedAuthors = [];
let newAuthorRequest = [];
let snMain = {};
const banNotesModule = new BanNotesModule([]);
const sentinelBanModule = new SentinelBanModule();
const modActionsModules = new ModActionsModule();

//dont start render until store is connected properly
const unsub = reduxStore.subscribe(() => {
  unsub();
  let state = reduxStore.getState();

  banNotesModule.refreshModule(state.snoonotes_info.modded_subs);
  banNotesModule.initModule();

  sentinelBanModule.refreshModule(state.snoonotes_info.modded_subs, state.user.hasConfig);
  sentinelBanModule.initModule();

  modActionsModules.refreshModule(state.snoonotes_info.modded_subs);
  modActionsModules.initModule();

  // var userNotesDisplay = document.createElement('div');
  // userNotesDisplay.id = "sn-notes-display";
  // document.body.appendChild(userNotesDisplay);
  // new Vue(userNoteDisplay).$mount(userNotesDisplay);

  var mainEl = document.createElement('div');
  mainEl.id = "sn-main";
  document.body.appendChild(mainEl);
  snMain = new Vue(SNMain).$mount(mainEl);
  window.snMain = snMain;
  const options = new Vue({
    render: h => h(SNOptions)
  }).$mount();
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
        if (author) {
          author = author.toLowerCase();
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
      if (author) {
        author = author.toLowerCase();
      }
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
  if (document.querySelector('div[data-reactroot]') && window.location.hostname.toLowerCase() != "mod.reddit.com") {
    //redesign
    window.setTimeout(function () {
      if (!jsapiListener.snMain) {
        jsapiListener.snMain = snMain;
      }
      jsapiListener.start();
    }, 2000);
  } else {
    let authorsReq = InjectIntoThingsClass();
    authorsReq = concatUnique(authorsReq, InjectIntoUserPage());
    window.setTimeout(function () {
      InjectIntoNewModmail()
    }, 2000)
    if (authorsReq.length > 0) {
      getNotesForUsers(reduxStore.dispatch, authorsReq);
    }
  }
})

const InjectIntoThingsClass = () => {
  let target = document.body.querySelector('div.content');
  if (target) {
    let observer = new MutationObserver(
      function (mutations) {
        let authors = [];
        mutations.forEach(function (mutation) {
          if (mutation.addedNodes && mutation.addedNodes.length > 0) {
            for (var i = 0; i < mutation.addedNodes.length; i++) {
              let node = mutation.addedNodes[i];
              if (node.querySelector) {
                if (node.classList.contains('thing')) {
                  if (!node.classList.contains('sn-done')) {
                    let author = BindNewThingsClassUserNotesElement(node);
                    if (usersWithNotes.indexOf(author) != -1 && authors.indexOf(author) == -1 && requestedAuthors.indexOf(author) == -1) {
                      authors.push(author);
                    }
                  }
                }
                node.querySelectorAll('.thing').forEach(function (thing) {
                  if (!thing.classList.contains('sn-done')) {
                    let author = BindNewThingsClassUserNotesElement(thing);
                    if (usersWithNotes.indexOf(author) != -1 && authors.indexOf(author) == -1 && requestedAuthors.indexOf(author) == -1) {
                      authors.push(author);
                    }
                  }
                })
              }
            }
          } //ignore removals because nothing on the "main" page currently gets removed.
          // if (mutation.removedNodes && mutation.removedNodes.length > 0) {
          //     let node = mutation.removedNodes[0];
          //     if (node.querySelectorAll) {
          //         node.querySelectorAll('.sn-user-notes').forEach(function (sn) {
          //             sn.__vue__.$destroy();
          //         });
          //     }
          // }
        })
        GetNotesForAuthors(authors);
      }
    )
    observer.observe(target, {
      childList: true,
      subtree: true
    });

    let things = document.querySelectorAll('.thing');
    snMain.$emit('AddThings', {
      thingIDs: (Array.from(things)).map((t) => {
        if (t.attributes['data-fullname']) return t.attributes['data-fullname'].value;
      }).filter(Boolean)
    });
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
  let childarray = [...thing.children];
  let entries = childarray.filter((c) => {
    return c.classList.contains('entry')
  });
  let entry = entries.length > 0 ? entries[0] : null;
  let authElem = entry ? entry.querySelector('.tagline a.author') : null;
  if (authElem && authElem.classList.contains('moderator')) authElem = null;
  let author = null;
  if (authElem) {
    author = thing.attributes['data-author'];
    if (!author || authElem.parentElement.classList.contains('recipient')) {
      author = authElem.textContent;
    } else {
      author = author.value;
    }
    let url = "";
    if (thing.attributes['data-subreddit'] && (thing.attributes['data-type'].value == 'link')) {
      url = "https://reddit.com/r/" + thing.attributes['data-subreddit'].value + '/' + thing.attributes['data-fullname'].value.replace('t3_', '');
    } else if (thing.attributes['data-type'].value == 'message') {
      url = "https://reddit.com/message/messages/" + thing.attributes['data-fullname'].value.replace('t4_', '');
    } else {

      let permlink = entry.querySelector('a.bylink').attributes['href'].value;
      let postid = permlink.substr(permlink.indexOf('comments/') + 9, 6); //post id is after comments/
      let commentRootURL = 'https://reddit.com/r/' + thing.attributes['data-subreddit'].value + '/comments/' + postid + '/.../';
      url = commentRootURL + thing.attributes['data-fullname'].value.replace('t1_', '');
    }

    let noteElemTarget = document.createElement('span');
    authElem.parentNode.insertBefore(noteElemTarget, authElem.nextSibling);
    snMain.injectNewThing(author, thing.attributes['data-subreddit'].value, url, noteElemTarget, thing)
    //snMain.$refs.noteDisplay.injectNewUserNotesComponent(author, thing.attributes['data-subreddit'].value, url, noteElemTarget);

  }
  thing.className = thing.className + ' sn-done';
  if (author) {
    author = author.toLowerCase();
  }
  return author;
}


const InjectIntoUserPage = () => {
  let authors = [];
  let userHeader = document.querySelector('body.profile-page div.side div.titlebox>h1');
  if (userHeader) {
    if (usersWithNotes.indexOf(userHeader.textContent.toLowerCase()) != -1) {
      authors.push(userHeader.textContent.toLowerCase())
    }
    let noteElemTarget = document.createElement('span');
    userHeader.parentNode.insertBefore(noteElemTarget, userHeader.nextSibling);

    snMain.injectNewThing(userHeader.textContent, '', 'https://reddit.com/u/' + userHeader.textContent, noteElemTarget, null, true);
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
            if (node.tagName == "ARTICLE" && !node.classList.contains("sn-done")) {
              let author = BindNewModmailUserNoteElement(mutation.addedNodes[0]);
              if (usersWithNotes.indexOf(author) != -1 && requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1) {
                newAuthorRequest.push(author);
              }
            }
            if (node.tagName == "DIV") {
              node.querySelectorAll('article:not(.sn-done)').forEach((article) => {
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
              node.querySelectorAll('.sn-user-notes').forEach(function (sn) {
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

    observer.observe(target, {
      childList: true,
      subtree: true
    });
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
    let noteElemTarget = document.createElement('span');

    authElem.parentNode.insertBefore(noteElemTarget, authElem.nextSibling);

    snMain.injectNewThing(author, sub, url, noteElemTarget, null, true);
  })
  article.className = article.className + ' sn-done';
  if (author) {
    author = author.toLowerCase();
  }
  return author;
}

const GetNotesForAuthors = (authors) => {
  if (!authors || authors.length == 0) return;
  let thisReq = [].concat(authors);
  requestedAuthors = concatUnique(requestedAuthors, thisReq);
  getNotesForUsers(reduxStore.dispatch, thisReq);
}

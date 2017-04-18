console.log('\'Allo \'Allo! Content script');

import Vue from 'vue'
import UserNotes from './components/userNotes.vue';
import SNOptions from './components/options/snOptions.vue';
import axios from 'axios';
import {SNAxiosInterceptor} from './utilities/snAxiosInterceptor';
import {apiBaseUrl} from './config';
import Toasted from 'vue-toasted';
import {reduxStore} from './redux/contentScriptStore';
import {getNotesForUsers} from './redux/actions/notes';
import {on} from './utilities/onEventDelegate';
import {BanNotesModule} from './modules/banNotes';


export const snInterceptor = new SNAxiosInterceptor(reduxStore);
axios.defaults.baseURL = apiBaseUrl;
axios.interceptors.request.use((req)=>{return snInterceptor.interceptRequest(req);});

Vue.use(Toasted,{position:'bottom-right',duration:2500});

let usersWithNotes = [];
let requestedAuthors = [];
let newAuthorRequest = [];
const banNotesModule = new BanNotesModule([]);
//dont start render until store is connected properly
const unsub = reduxStore.subscribe(()=>{
    unsub();
    let state = reduxStore.getState();

    banNotesModule.refreshModule(state.snoonotes_info.modded_subs);
    
    const options = new Vue({render: h=>h(SNOptions)}).$mount();
    options.$on('refresh',()=>{
        let authorsReq = [];
        let things = document.querySelector('.thing');
        for (let i = 0; i < things.length; i++){
            let authElem = things[i].querySelector('p.tagline a.author');
            if(authElem){
                let author = things[i].attributes['data-author'].value;
                if(!author){
                    author = authElem.textContent;
                }
                if(usersWithNotes.indexOf(author) != -1){
                    authorsReq.push(author);
                }
            }
        }
        if (authorsReq.length > 0){
            getNotesForUsers(reduxStore.dispatch,authorsReq);
        }

        //new modmail
        requestedAuthors = [];
        let authElems = article.querySelectorAll('a.Message__author,a.ThreadPreview__author');
        authElems.forEach(function(authElem){
            let author = authElem.textContent.replace(/u\//,'');
            if(requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1 && usersWithNotes.indexOf(author) != -1){
                newAuthorRequest.push(author);
            }
        });
        NewModmailAuthorRequest();
    })
    const userElem = document.querySelector('#header-bottom-right > .user');
    if(userElem){
        userElem.parentNode.insertBefore(options.$el,userElem.nextSibling);
    }

    usersWithNotes = state.snoonotes_info.users_with_notes;
    reduxStore.subscribe(()=>{
        usersWithNotes = reduxStore.getState().snoonotes_info.users_with_notes;
    });

    let authorsReq = InjectIntoThingsClass();
    authorsReq = concatUnique(authorsReq, InjectIntoUserPage());
    window.setTimeout(function(){
    InjectIntoNewModmail()}
, 2000)
    if (authorsReq.length > 0){
        getNotesForUsers(reduxStore.dispatch,authorsReq);
    }
})

const InjectIntoThingsClass = () => {
    let things = document.querySelectorAll('.thing');
    let commentRootURL;

    let authors = [];
    for (let i = 0; i < things.length; i++){
        let authElem = things[i].querySelector('p.tagline a.author');
        if(authElem){
            let author = things[i].attributes['data-author'].value;
            if(!author){
                author = authElem.textContent;
            }
            if(usersWithNotes.indexOf(author) != -1 && authors.indexOf(author) == -1){
                authors.push(author);
            }
            let url = "";
            if (things[i].attributes['data-type'].value == 'link'){
                url = "https://reddit.com/r/" + things[i].attributes['data-subreddit'].value + '/' + things[i].attributes['data-fullname'].value.replace('t3_','');
            }
            else{
                if(!commentRootURL) commentRootURL = 'https://reddit.com/r/'+ things[i].attributes['data-subreddit'].value + '/comments/' + things[i].closest('.nestedlisting').id.replace('siteTable_t3_','') + '/.../';
                url = commentRootURL + things[i].attributes['data-fullname'].value.replace('t1_','');
            }
            
            let noteElem = document.createElement('user-notes');
            noteElem.setAttribute('username',author);
            noteElem.setAttribute('subreddit',things[i].attributes['data-subreddit'].value);
            noteElem.setAttribute('url',url);
            authElem.parentNode.insertBefore(noteElem,authElem.nextSibling);
            new Vue({components:{'user-notes':UserNotes}}).$mount(things[i]);
        }
    }
    return authors;
}

const InjectIntoUserPage = () => {
    let authors = [];
    let userHeader = document.querySelector('body.profile-page div.side div.titlebox>h1');
    if(userHeader){
        if(usersWithNotes.indexOf(userHeader.textContent)!= -1){
            authors.push(userHeader.textContent)
        }
        let noteElem = document.createElement('user-notes');
            noteElem.setAttribute('username',userHeader.textContent);
            noteElem.setAttribute('url','https://reddit.com/u/'+userHeader.textContent)
            noteElem.setAttribute('class','SNUserpageNote');
        userHeader.parentNode.insertBefore(noteElem,userHeader.nextSibling);
        new Vue({components:{'user-notes':UserNotes}}).$mount(userHeader.parentElement);
    }
    return authors;
}

const InjectIntoNewModmail = () => {
    let authors = [];
    let target = document.querySelector('div.App');
    if(target){
        var observer = new MutationObserver(
            function(mutations){
                mutations.forEach(function(mutation){
                    if(mutation.addedNodes && mutation.addedNodes.length > 0){
                        let node = mutation.addedNodes[0];
                        if(node.tagName == "ARTICLE" && !node.classList.contains("SNDone") ){
                            BindNewModmailUserNoteElement(mutation.addedNodes[0]);
                        }
                        if(node.tagName == "DIV"){
                            node.querySelectorAll('article:not(.SNDone)').forEach((article)=>{
                                BindNewModmailUserNoteElement(article);
                            })
                        }
                    }

                });
                NewModmailAuthorRequest();
            });
        
        document.querySelectorAll('article').forEach(function(a){
            BindNewModmailUserNoteElement(a);
            NewModmailAuthorRequest();
        })

        observer.observe(target,{childList:true,subtree:true});
    }
    
}

const concatUnique = (array1, array2 )=>{
    let toReturn = [];
    toReturn = toReturn.concat(array1);
    for(let i=0; i<array2.length; i++){
        if(toReturn.indexOf(array2[i]) == -1){
            toReturn.push(array2[i]);
        }
    }
    return toReturn;
}

const BindNewModmailUserNoteElement = (article) =>{
    let authElems = article.querySelectorAll('a.Message__author,a.ThreadPreview__author');
    authElems.forEach(function(authElem){
        let author = authElem.textContent.replace(/u\//,'');
        if(requestedAuthors.indexOf(author) == -1 && newAuthorRequest.indexOf(author) == -1 && usersWithNotes.indexOf(author) != -1){
            newAuthorRequest.push(author);
        }
        let url = 'https://mod.reddit.com' + article.querySelector('.m-link,a.ThreadPreview__time').attributes['href'].value;
        let threadCommunity = document.querySelector('a.ThreadTitle__community');
        let sub = null;
        if(threadCommunity) sub = threadCommunity.textContent;
        let noteElem = document.createElement('span');
            noteElem.setAttribute('username',author);
            noteElem.setAttribute('subreddit',sub);
            noteElem.setAttribute('url',url);
            noteElem.setAttribute('is','user-notes');
            
            let vueinst = new Vue({components:{'user-notes':UserNotes}, destroyed:function(){console.warn('vue destroyed')}}).$mount(noteElem);
            authElem.parentNode.insertBefore(vueinst.$el,authElem.nextSibling);
    })
    article.className = article.className + ' SNDone';
}

const NewModmailAuthorRequest = () =>{
    let thisReq = [].concat(newAuthorRequest);
    requestedAuthors = concatUnique(requestedAuthors,thisReq);
    newAuthorRequest = [];
    if(thisReq.length <= 0) return;
    getNotesForUsers(reduxStore.dispatch,thisReq);
}
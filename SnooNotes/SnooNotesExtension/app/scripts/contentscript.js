console.log('\'Allo \'Allo! Content script');

import Vue from 'vue'
import UserNotes from './userNotes.vue';
import SNOptions from './components/options/snOptions.vue';
import axios from 'axios';
import {snInterceptor} from './utilities/snAxiosInterceptor';
import {apiBaseUrl} from './config';

import {reduxStore} from './redux/contentScriptStore';

axios.defaults.baseURL = apiBaseUrl;
axios.interceptors.request.use((req)=>{return snInterceptor.interceptRequest(req);});

//dont start render until store is connected properly
const unsub = reduxStore.subscribe(()=>{
    unsub();
    var NotesComponent = Vue.extend({template:'<user-notes></user-notes>'})
    var things = document.querySelectorAll('.thing');
    for (var i = 0; i < things.length; i++){
        var notes = new Vue({render: h => h(UserNotes)}).$mount();
        var authElem = things[i].querySelector('a.author');
        if(authElem){
            authElem.parentNode.insertBefore(notes.$el,authElem.nextSibling);
        }
    }

    var options = new Vue({render: h=>h(SNOptions)}).$mount();
    var userElem = document.querySelector('#header-bottom-right > .user');
    userElem.parentNode.insertBefore(options.$el,userElem.nextSibling);
})
console.log('\'Allo \'Allo! Content script');

import Vue from 'vue'
import Test from './test.vue'
import UserNotes from './userNotes.vue';
import SNOptions from './snOptions.vue';

import {reduxStore} from './redux/contentScriptStore';

var elem = document.createElement('div');
elem.id = 'SnooNotes';
elem.innerHTML = '<h1>oh shit son</h1>';
document.body.appendChild(elem);

//dont start render until store is connected properly
const unsub = reduxStore.subscribe(()=>{
    unsub();
    var NotesComponent = Vue.extend({template:'<user-notes></user-notes>'})
    var v = new Vue({ el: '#SnooNotes', render: h => h(Test) });
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
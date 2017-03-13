console.log('\'Allo \'Allo! Content script');

import Vue from 'vue'
import Test from './test.vue'
import {reduxStore} from './redux/contentScriptStore';

var elem = document.createElement('div');
elem.id = 'SnooNotes';
elem.innerHTML = '<h1>oh shit son</h1>';
document.body.appendChild(elem);

//dont start render until store is connected properly
const unsub = reduxStore.subscribe(()=>{
    unsub();
    var v = new Vue({ el: '#SnooNotes', render: h => h(Test) });
})
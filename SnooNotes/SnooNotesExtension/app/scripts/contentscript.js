console.log('\'Allo \'Allo! Content script');

import Vue from 'vue'
import Test from './test.vue'

var elem = document.createElement('div');
elem.id = 'SnooNotes';
elem.innerHTML = '<h1>oh shit son</h1>';
document.body.appendChild(elem);

var v = new Vue({ el: '#SnooNotes', render: h => h(Test) });
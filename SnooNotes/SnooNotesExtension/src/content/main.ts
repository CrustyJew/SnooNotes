import Vue = require('vue')
import {TestComponent} from './test'

var elem = document.createElement('div');
elem.id = 'SnooNotes';
elem.innerHTML = '<h1>oh shit son</h1>';
document.body.appendChild(elem);

var v = new TestComponent({ el: '#SnooNotes' });
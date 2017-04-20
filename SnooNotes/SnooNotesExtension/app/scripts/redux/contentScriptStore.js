import Vue from 'vue';
import { Store } from 'react-chrome-redux';
import Revue from 'revue';

export const reduxStore = new Store({ portName: 'SnooNotesExtension' });
export const store = new Revue(Vue, reduxStore);


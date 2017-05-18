import Vue from 'vue';
import { Store } from 'react-chrome-redux';
import Revue from 'revue';

export const reduxStore = new Store({ portName: 'SnooNotesExtension' },{},null);
reduxStore.extensionId = chrome.runtime.id;
export const store = new Revue(Vue, reduxStore);


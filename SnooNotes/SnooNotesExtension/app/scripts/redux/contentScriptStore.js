import Vue from 'vue';
  import {Store} from 'react-chrome-redux';
  import Revue from 'revue';
  import {createStore} from 'redux';
  
  const reduxStore = new Store({ portName: 'SnooNotesExtension'});
  export const store = new Revue(Vue, reduxStore);

  
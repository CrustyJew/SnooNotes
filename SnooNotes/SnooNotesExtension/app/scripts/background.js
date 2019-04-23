// Enable chromereload by uncommenting this line:
//import 'chromereload/devonly';


import { userFound, silentRenewError, sessionTerminated, userExpiring, userSignedOut } from './redux/actions/user';
import { store, loadUser } from './redux/store';
import { userManager } from './utilities/userManager';
//import { snUpdate, hubConnection } from './libs/snUpdatesHub';
//import {hubConnection} from 'signalr-no-jquery';
import { apiBaseUrl, signalrBaseUrl } from './config';
import axios from 'axios';
import { SNAxiosInterceptor } from './utilities/snAxiosInterceptor';
import { gotNewNote, gotDeleteNote } from './redux/actions/notes';
import { getModSubs } from './redux/actions/snoonotesInfo';
import * as signalR from '@aspnet/signalr';


export const snInterceptor = new SNAxiosInterceptor(store);
axios.defaults.baseURL = apiBaseUrl;
axios.interceptors.request.use((req) => { return snInterceptor.interceptRequest(req); });

const onUserLoaded = (user) => {
  store.dispatch(userFound(user));
};

// event callback when silent renew errored
const onSilentRenewError = (error) => {
  store.dispatch(silentRenewError(error));
};

// event callback when the access token expired
const onAccessTokenExpired = () => {
  loadUser(store.dispatch);
};

// event callback when the user is logged out
const onUserUnloaded = () => {
  store.dispatch(sessionTerminated());
};

// event callback when the user is expiring
const onAccessTokenExpiring = () => {
  store.dispatch(userExpiring());
}

// event callback when the user is signed out
const onUserSignedOut = () => {
  store.dispatch(userSignedOut());
}

userManager.clearStaleState();
//userManager.removeUser();
userManager.events.addUserLoaded(onUserLoaded);
userManager.events.addSilentRenewError(onSilentRenewError);
userManager.events.addAccessTokenExpired(onAccessTokenExpired);
userManager.events.addAccessTokenExpiring(onAccessTokenExpiring);
userManager.events.addUserUnloaded(onUserUnloaded);
userManager.events.addUserSignedOut(onUserSignedOut);


// const hubConn = hubConnection(signalrBaseUrl, { useDefaultPath: false });
// const snUpdate = hubConn.createHubProxy('SnooNoteUpdates');
var curToken = "";

const snUpdate = new signalR.HubConnectionBuilder().withUrl(signalrBaseUrl,{accessTokenFactory: ()=> curToken}).build();

snUpdate.on('addNewNote', (note) => {
  store.dispatch(gotNewNote(note));
})
snUpdate.on('deleteNote', (user, noteID, outOfNotes) => {
  store.dispatch(gotDeleteNote(user, noteID, outOfNotes));
})
snUpdate.on('refreshNoteTypes', () => {
  store.dispatch(getModSubs());
});
snUpdate.on('modAction', (thingID, mod, action) => {
  chrome.tabs.query({}, function (tabs) {
    for (var i = 0; i < tabs.length; i++) {
      chrome.tabs.sendMessage(tabs[i].id, { "method": "modAction", "req": { "thingID": thingID, "mod": mod, "action": action } });
    }
  });
})
store.subscribe(() => {
  let newToken = store.getState().user.access_token;
  if (newToken != curToken) {
   
    curToken = newToken;
    if (!curToken) {
      snUpdate.stop();
    }
    else {
      //if (snUpdate.connection.state != 4) { hubConnection.stop(); }
      //if(snUpdate.connection.state == 4){
        snUpdate.start()
          .then(function () { console.log('SignalR connected, connection ID=' + snUpdate.connection.id); },
          function () { console.log('SignalR could not connect')})

    //}
  }

  }
});
snUpdate.connection.onclose(() => {
  console.warn('Socket disconnected');
  if (curToken) {
    setTimeout(() => {
      snUpdate.start()
        .then(function () { console.log('SignalR reconnected, connection ID=' + snUpdate.connection.id); },
        function () { console.log('SignalR could not connect'); });
    }, 2500 + (10 * Math.Floor(Math.random() * 100)))
  }
})

snUpdate.start();
// snUpdate.start({ jsonp: true })
// .done(function(){ console.log('SignalR connected, connection ID=' + connection.id); })
// .fail(function(){ console.log('SignalR could not connect'); });
    //$.connection.hub.start().then(function () { console.log('Connected socket'); }, function (e) { console.log(e.toString()) });
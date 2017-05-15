// Enable chromereload by uncommenting this line:
import 'chromereload/devonly';


import { userExpired, userFound, silentRenewError, sessionTerminated, userExpiring, userSignedOut } from './redux/actions/user';
import { store } from './redux/store';
import { userManager } from './utilities/userManager';
import { snUpdate, hubConnection } from './libs/snUpdatesHub';
//import {hubConnection} from 'signalr-no-jquery';
import { apiBaseUrl } from './config';
import axios from 'axios';
import { SNAxiosInterceptor } from './utilities/snAxiosInterceptor';
import { gotNewNote, gotDeleteNote } from './redux/actions/notes';
import { getModSubs } from './redux/actions/snoonotesInfo';

export const snInterceptor = new SNAxiosInterceptor(store);
axios.defaults.baseURL = apiBaseUrl;
axios.interceptors.request.use((req) => { return snInterceptor.interceptRequest(req); });

chrome.runtime.onInstalled.addListener(function () {
  //console.log('previousVersion', details.previousVersion);
});

const onUserLoaded = (user) => {
  store.dispatch(userFound(user));
};

// event callback when silent renew errored
const onSilentRenewError = (error) => {
  store.dispatch(silentRenewError(error));
};

// event callback when the access token expired
const onAccessTokenExpired = () => {
  userManager.clearStaleState();
  userManager.removeUser();
  store.dispatch(userExpired());

};

// event callback when the user is logged out
const onUserUnloaded = () => {
  userManager.clearStaleState();
  userManager.removeUser();
  store.dispatch(sessionTerminated());
};

// event callback when the user is expiring
const onAccessTokenExpiring = () => {
  store.dispatch(userExpiring());
}

// event callback when the user is signed out
const onUserSignedOut = () => {
  userManager.clearStaleState();
  userManager.removeUser();
  store.dispatch(userSignedOut());
}

userManager.clearStaleState();
userManager.events.addUserLoaded(onUserLoaded);
userManager.events.addSilentRenewError(onSilentRenewError);
userManager.events.addAccessTokenExpired(onAccessTokenExpired);
userManager.events.addAccessTokenExpiring(onAccessTokenExpiring);
userManager.events.addUserUnloaded(onUserUnloaded);
userManager.events.addUserSignedOut(onUserSignedOut);


// const hubConn = hubConnection(signalrBaseUrl, { useDefaultPath: false });
// const snUpdate = hubConn.createHubProxy('SnooNoteUpdates');
var curToken = "";
snUpdate.client.addNewNote = (note) => {
  store.dispatch(gotNewNote(note));
}
snUpdate.client.deleteNote = (user, noteID, outOfNotes) => {
  store.dispatch(gotDeleteNote(user, noteID, outOfNotes));
}
snUpdate.client.refreshNoteTypes = () => {
  store.dispatch(getModSubs());
}
store.subscribe(() => {
  let newToken = store.getState().user.access_token;
  if (newToken != curToken) {
    hubConnection.qs = { token: newToken };
    curToken = newToken;
    if(!curToken) {
      hubConnection.stop();
    }
    else{
      if (snUpdate.connection.state != 4) { hubConnection.stop(); }

      hubConnection.start({ jsonp: false })
        .done(function () { console.log('SignalR connected, connection ID=' + hubConnection.id); })
        .fail(function () { console.log('SignalR could not connect'); });
    }
    
  }
});
hubConnection.disconnected(() => {
  console.warn('Socket disconnected');
  if (curToken) {
    setTimeout(() => {
      hubConnection.start()
        .done(function () { console.log('SignalR reconnected, connection ID=' + hubConnection.id); })
        .fail(function () { console.log('SignalR could not connect'); });
    }, 5000)
  }
})
// snUpdate.start({ jsonp: true })
// .done(function(){ console.log('SignalR connected, connection ID=' + connection.id); })
// .fail(function(){ console.log('SignalR could not connect'); });
    //$.connection.hub.start().then(function () { console.log('Connected socket'); }, function (e) { console.log(e.toString()) });
// Enable chromereload by uncommenting this line:
// import 'chromereload/devonly';
import { userExpired, userFound, silentRenewError, sessionTerminated, userExpiring, userSignedOut } from './redux/actions/user';
import store from './redux/store';
import {userManager} from './utilities/userManager';



chrome.runtime.onInstalled.addListener(function (details) {
  console.log('previousVersion', details.previousVersion);
});

chrome.browserAction.setBadgeText({text: '\'Allo'});

console.log('\'Allo \'Allo! Event Page for Browser Action');

userManager.events.addUserLoaded(onUserLoaded);
    userManager.events.addSilentRenewError(onSilentRenewError);
    userManager.events.addAccessTokenExpired(onAccessTokenExpired);
    userManager.events.addAccessTokenExpiring(onAccessTokenExpiring);
    userManager.events.addUserUnloaded(onUserUnloaded);
    userManager.events.addUserSignedOut(onUserSignedOut);

  const onUserLoaded = (user) => {
    store.dispatch(userFound(user));
  };

  // event callback when silent renew errored
  const onSilentRenewError = (error) => {
    store.dispatch(silentRenewError(error));
  };

  // event callback when the access token expired
  const onAccessTokenExpired = () => {
    store.dispatch(userExpired());
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
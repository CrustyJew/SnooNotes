﻿import CustomRedirectManager from './utilities/customRedirectNavigator';
//import CustomWebStorageStateStore from './utilities/customWebStorageStateStore';
import CustomIFrameNavigator from './utilities/customIFrameNavigator'; 

export const authBaseUrl = 'https://snoonotes.com/Auth/';
export const apiBaseUrl = 'https://snoonotes.com/api/';
export const signalrBaseUrl = 'https://snoonotes.com/SnooNotesHub';
export const dirtbagBaseUrl = 'https://dirtbag.snoonotes.com/api/';
// export const authBaseUrl = 'https://localhost:5000/Auth/';
// export const apiBaseUrl = 'https://localhost:5001/api/';
// export const signalrBaseUrl = 'https://localhost:5001/SnooNotesHub';
// export const dirtbagBaseUrl = 'https://localhost:5002/api/';

export const mediaProviders = ['youtube.com','youtu.be','m.youtube.com', 'vimeo.com', 'vid.me','twitter.com','twitch.tv','clips.twitch.tv', 'soundcloud.com','facebook.com','etsy.com','dailymotion.com', 'dai.ly']

const redirectNav = new CustomRedirectManager();
//const customStore = new CustomWebStorageStateStore();
const customiframeNavigator = new CustomIFrameNavigator();

export const userManagerConfig = {
  client_id: 'snoonotes',
  redirect_uri: authBaseUrl + 'redux_callback.html',
  response_type: 'token id_token',
  scope: 'openid profile snoonotes dirtbag',
  authority: authBaseUrl,
  silent_redirect_uri: authBaseUrl + 'redux_silent_renew.html',
  automaticSilentRenew: true,
  filterProtocolClaims: true,
  loadUserInfo: true,
  redirectNavigator: redirectNav,
  monitorSession: false,
  silentRequestTimeout: 15000,
  checkSessionInterval: 15000,
  iframeNavigator: customiframeNavigator
  //userStore: customStore
};


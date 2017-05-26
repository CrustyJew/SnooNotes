import CustomRedirectManager from './utilities/customRedirectNavigator';
//import CustomWebStorageStateStore from './utilities/customWebStorageStateStore';
import CustomIFrameNavigator from './utilities/customIFrameNavigator'; 

export const authBaseUrl = 'https://snoonotes.com/Auth/';
export const apiBaseUrl = 'https://snoonotes.com/api/';
export const signalrBaseUrl = 'https://snoonotes.com/signalr';
const redirectNav = new CustomRedirectManager();
//const customStore = new CustomWebStorageStateStore();
const customiframeNavigator = new CustomIFrameNavigator();

export const userManagerConfig = {
  client_id: 'snoonotes',
  redirect_uri: authBaseUrl + 'redux_callback.html',
  response_type: 'token id_token',
  scope: 'openid profile snoonotes',
  authority: authBaseUrl,
  silent_redirect_uri: authBaseUrl + 'redux_silent_renew.html',
  automaticSilentRenew: true,
  filterProtocolClaims: true,
  loadUserInfo: true,
  redirectNavigator: redirectNav,
  monitorSession: true,
  silentRequestTimeout: 15000,
  checkSessionInterval: 15000,
  iframeNavigator: customiframeNavigator
  //userStore: customStore
};


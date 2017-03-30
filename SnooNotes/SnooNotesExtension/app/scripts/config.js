import CustomRedirectManager from './utilities/customRedirectNavigator';
import CustomWebStorageStateStore from './utilities/customWebStorageStateStore';
import CustomIFrameNavigator from './utilities/customIFrameNavigator'; 

export const authBaseUrl = 'http://localhost:5000/Auth/';
export const apiBaseUrl = 'http://localhost:5001/api/';
export const signalrBaseUrl = 'http://localhost:5001/signalr';
const redirectNav = new CustomRedirectManager();
const customStore = new CustomWebStorageStateStore();
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
  checkSessionInterval: 15000,
  iframeNavigator: customiframeNavigator
  //userStore: customStore
};


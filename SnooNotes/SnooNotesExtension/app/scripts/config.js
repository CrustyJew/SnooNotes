import {CustomRedirectManager} from './utilities/customRedirectNavigator';
export const baseUrl = 'http://localhost:5001/';
const redirectNav = new CustomRedirectManager();
export const userManagerConfig = {
  client_id: 'snoonotes',
  redirect_uri: baseUrl + 'redux_callback.html',
  response_type: 'token id_token',
  scope: 'openid profile snoonotes',
  authority: 'http://localhost:5000/Auth',
  silent_redirect_uri: baseUrl + 'redux_silent_renew.html',
  automaticSilentRenew: true,
  filterProtocolClaims: true,
  loadUserInfo: true,
  redirectNavigator: redirectNav
};



export const baseUrl = 'http://localhost:5001/';

export const userManagerConfig = {
  client_id: 'SnooNotesExtension',
  redirect_uri: baseUrl + 'redux_callback.html',
  response_type: 'token id_token',
  scope: 'openid profile dirtbag',
  authority: 'http://localhost:5000/Auth',
  silent_redirect_uri: baseUrl + 'redux_silent_renew.html',
  automaticSilentRenew: true,
  filterProtocolClaims: true,
  loadUserInfo: true,
  signin_url: 'http://localhost:5000/Auth/connect/authorize?client_id=snoonotes&response_type=token&scope=profile%20openid&redirect_uri=http://localhost:5001/ExtensionRedirect'
};


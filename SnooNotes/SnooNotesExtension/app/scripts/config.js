
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
};

export const userManager = new UserManager(userManagerConfig);

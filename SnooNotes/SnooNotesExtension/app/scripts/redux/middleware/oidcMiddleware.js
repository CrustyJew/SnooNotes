//https://github.com/maxmantz/redux-oidc
//https://github.com/maxmantz/redux-oidc/blob/master/LICENSE
// The MIT License (MIT)

// Copyright (c) 2016 Maximilian Mantz

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

import { userExpired, userFound, loadingUser } from '../actions/user';

// store the user here to prevent future promise calls to getUser()
export let storedUser = null;

// helper function to set the stored user manually (for testing)
export function setStoredUser(user) {
  storedUser = user;
}

// helper function to remove the stored user manually (for testing)
export function removeStoredUser() {
  storedUser = null;
}

export function errorCallback(error) {
  console.error('Error in oidcMiddleware', error);
}

export function middlewareHandler(next, action, userManager) {
    let token = userManager.getAccessToken();
  if (!token) {
    next(userExpired());
    }
  
  return next(action);
}

// the middleware creator function
export function createOidcMiddleware(userManager) {
  if (!userManager || !userManager.getUser) {
    throw new Error('You must provide a user manager!');
  }

  // the middleware
  return (store) => (next) => (action) => {
    middlewareHandler(next, action, userManager).catch(errorCallback);
  }
};
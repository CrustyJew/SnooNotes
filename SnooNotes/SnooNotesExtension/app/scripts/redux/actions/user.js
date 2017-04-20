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

export const USER_EXPIRED = 'USER_EXPIRED';
export const REDIRECT_SUCCESS = 'REDIRECT_SUCCESS';
export const USER_LOADED = 'USER_LOADED';
export const SILENT_RENEW_ERROR = 'SILENT_RENEW_ERROR';
export const SESSION_TERMINATED = 'SESSION_TERMINATED';
export const USER_EXPIRING = 'USER_EXPIRING';
export const USER_FOUND = 'USER_FOUND';
export const LOADING_USER = 'LOADING_USER';
export const USER_SIGNED_OUT = 'USER_SIGNED_OUT';
export const LOGIN = "LOGIN";
export const SILENT_RENEW_SUCCESS = "SILENT_RENEW_SUCCESS";
export const REFRESH_USER = "REFRESH_USER";
export const REFRESH_USER_ALIAS = "REFRESH_USER_ALIAS";

// dispatched when the existing user expired
export function userExpired() {
  return {
    type: USER_EXPIRED
  };
}

// dispatched after a successful redirect callback
export function redirectSuccess(url) {
  return {
    type: REDIRECT_SUCCESS,
    payload: url
  };
}

// dispatched when a user has been found in storage
export function userFound(user) {
  return {
    type: USER_FOUND,
    payload: user
  };
}

// dispatched when silent renew fails
// payload: the error
export function silentRenewError(error) {
  return {
    type: SILENT_RENEW_ERROR,
    payload: error
  };
}

export function silentRenewSuccess(url) {
  return {
    type: SILENT_RENEW_SUCCESS,
    payload: url
  }
}

// dispatched when the user is logged out
export function sessionTerminated() {
  return {
    type: SESSION_TERMINATED
  };
}

// dispatched when the user is expiring (just before a silent renew is triggered)
export function userExpiring() {
  return {
    type: USER_EXPIRING
  };
}

// dispatched when a new user is loading
export function loadingUser(tabid = "") {
  return {
    type: LOADING_USER,
    payload: tabid
  };
}

export function userSignedOut() {
  return {
    type: USER_SIGNED_OUT
  };
}

export function login() {
  return {
    type: LOGIN
  };
}

export function refreshUser(contentScript = false) {
  if (contentScript) {
    return {
      type: REFRESH_USER_ALIAS
    }
  }
  return {
    type: REFRESH_USER
  };
}
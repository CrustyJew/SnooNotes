//import {USER_LOGIN, USER_LOGOUT} from '../actions'
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
import {
    USER_EXPIRED,
    REDIRECT_SUCCESS,
    USER_FOUND, USER_NOT_FOUND,
    SILENT_RENEW_ERROR,
    SESSION_TERMINATED,
    LOADING_USER,
    USER_SIGNED_OUT,
    REFRESH_USER
} from '../actions/user';

const initialState = {
    access_token: null,
    name: "",
    isLoadingUser: false
};

export default function userReducer(state = initialState, action) {
    switch (action.type) {
        case USER_EXPIRED:
            return Object.assign({}, initialState);
        case SILENT_RENEW_ERROR:
            return Object.assign({}, { ...state }, { isLoadingUser: false });
        case SESSION_TERMINATED:
        case USER_SIGNED_OUT:
        case USER_NOT_FOUND:
            return Object.assign({}, initialState);
        case REDIRECT_SUCCESS:
        case USER_FOUND:
            return Object.assign({}, {
                access_token: action.payload.access_token,
                name: action.payload.profile.name, //todo move this to action creator, specific logic shouldn't be here
                isLoadingUser: false
            });
        case LOADING_USER:
            return Object.assign({}, { ...state }, { isLoadingUser: true, last_tab_id: action.payload });
        case REFRESH_USER:
            return Object.assign({}, { ...state }, { isLoadingUser: true });
        default:
            return state;
    }
}





/*var initialState = {
    loggedIn : false,
    moderatedSubreddits : [],
    isFetching = false
}

const logIn = (state = initialState, action) => {
    if(!action.subreddits){
        return initialState;
    }
    return {
        loggedIn: true,
        moderatedSubreddits = action.subreddits,
        isFetching = false
    };
}

const logOut = (state, action) => {
    return initialState;
}

const userReducer =  (state, action) =>{
    switch (action.type){
        case USER_LOGOUT:
            return logOut(state, action);
        case USER_LOGIN:
            return logIn(state, action); 

        default:
            return state;
    }
}

export default userReducer;*/
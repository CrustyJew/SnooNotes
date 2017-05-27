import { createStore, applyMiddleware } from 'redux';
import { userManager } from '../utilities/userManager';
import { composeWithDevTools } from 'remote-redux-devtools'
import thunk from 'redux-thunk'
import { wrapStore, alias } from 'react-chrome-redux';
import { refreshUser, REFRESH_USER_ALIAS, LOGIN, REDIRECT_SUCCESS, sessionTerminated } from './actions/user';
import reducer from './reducers/index';
import { loadingUser, stopLoadingUser } from './actions/user';
import { getModSubs, getUsersWithNotes, forceRefresh } from './actions/snoonotesInfo';

const initialState = { user: { user: null, isLoadingUser: false } };


const bg_aliases = {
    [LOGIN]: (action) => {
        return (dispatch) => {
            loadUser(dispatch, true, action);
        }
    },
    [REDIRECT_SUCCESS]: (req) => {
        return (dispatch) => {
            let state = store.getState();
            userManager.signinRedirectCallback(req.payload).then(() => {
                chrome.tabs.update(state.user.last_tab_id, { active: true }, () => {
                    chrome.tabs.remove(req._sender.tab.id);
                });
                dispatch(getModSubs());
            }, (err) => { console.warn(err) });
        }
    },
    [REFRESH_USER_ALIAS]: () => {
        return (dispatch) => {
            dispatch(refreshUser());
            userManager.signinSilent()
                .then(() => {
                    forceRefresh(dispatch).then(() => {
                        initUser(dispatch);
                    })
                }, () => {
                    console.error('Error getting new token');
                    dispatch(sessionTerminated());
                })
        }
    }
}
//const enhancer = applyMiddleware(alias(bg_aliases),thunk, createOidcMiddleware(userManager));
//const store = createStore(reducer,initialState,enhancer);

export const initUser = (dispatch) => {
    dispatch(getModSubs());
    dispatch(getUsersWithNotes());
}

export const store = createStore(reducer, initialState, composeWithDevTools(
    applyMiddleware(alias(bg_aliases), thunk)//, createOidcMiddleware(userManager))
)
)

wrapStore(store, { portName: "SnooNotesExtension" });

export const loadUser = (dispatch, displayLogin = false, action = {}) => {
    dispatch(loadingUser());
    userManager.getUser().then((u) => {
        if (u && !u.expired) {
            initUser(dispatch);
        }
        else {
            userManager.removeUser().then(() => {
                userManager.signinSilent().then(() => {
                    initUser(dispatch);
                }, () => {
                    userManager.clearStaleState();
                    if(displayLogin){
                        dispatch(loadingUser(action._sender.tab.id));
                        userManager.signinRedirect();
                    }
                    else{
                        dispatch(stopLoadingUser())
                    }
                });
            })
        }
    })
}

loadUser(store.dispatch);

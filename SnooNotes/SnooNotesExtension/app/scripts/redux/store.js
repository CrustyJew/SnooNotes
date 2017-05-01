import { createStore, applyMiddleware } from 'redux';
import { userManager } from '../utilities/userManager';
import { composeWithDevTools } from 'remote-redux-devtools'
import thunk from 'redux-thunk'
import { wrapStore, alias } from 'react-chrome-redux';
import { refreshUser, REFRESH_USER_ALIAS, LOGIN, REDIRECT_SUCCESS, sessionTerminated } from './actions/user';
import reducer from './reducers/index';
import { loadingUser, userFound } from './actions/user';
import { getModSubs, getUsersWithNotes, forceRefresh } from './actions/snoonotesInfo';

const initialState = { user: { user: null, isLoadingUser: false } };


const bg_aliases = {
    [LOGIN]: (action) => {
        return (dispatch) => {
            dispatch(loadingUser());
            return userManager.getUser().then((u) => {
                if (u) {
                    initUser(dispatch, u);
                } else {
                    userManager.signinSilent().then((user) => {
                        initUser(dispatch, user);
                    }, () => {
                        dispatch(loadingUser(action._sender.tab.id));
                        userManager.signinRedirect();
                    })
                }
            })
        }
    },
    [REDIRECT_SUCCESS]: (req) => {
        return (dispatch) => {
            userManager.signinRedirectCallback(req.payload).then((user) => {
                let state = store.getState();
                chrome.tabs.update(state.user.last_tab_id, { active: true }, () => {
                    chrome.tabs.remove(req._sender.tab.id);
                });
                dispatch(userFound(user));
                dispatch(getModSubs());
            }, (err) => { console.warn(err) });
        }
    },
    [REFRESH_USER_ALIAS]: () => {
        return (dispatch) => {
            dispatch(refreshUser());
            userManager.signinSilent()
                .then((user) => {
                    forceRefresh(dispatch).then(()=>{
                        initUser(dispatch, user);
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

export const initUser = (dispatch, user) => {
    dispatch(userFound(user));
    dispatch(getModSubs());
    dispatch(getUsersWithNotes());
}

export const store = createStore(reducer, initialState, composeWithDevTools(
    applyMiddleware(alias(bg_aliases), thunk)//, createOidcMiddleware(userManager))
)
)

wrapStore(store, { portName: "SnooNotesExtension" });

userManager.getUser().then((u) => {
    if (u) {
        initUser(store.dispatch, u);
    }
    else {
        userManager.signinSilent().then((user) => {
            initUser(store.dispatch, user);
        }, () => {
            userManager.clearStaleState();
            console.log('user not logged in')
        });
    }
})

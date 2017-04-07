import { createStore, combineReducers, applyMiddleware, compose } from 'redux';
import { createOidcMiddleware }  from './middleware/oidcMiddleware';
import {userManager} from '../utilities/userManager';
import {composeWithDevTools} from 'remote-redux-devtools'
import thunk from 'redux-thunk'
import { wrapStore, alias } from 'react-chrome-redux';
import {refreshUser,REFRESH_USER_ALIAS, login, LOGIN, REDIRECT_SUCCESS, SILENT_RENEW_SUCCESS, sessionTerminated} from './actions/user';
import reducer from './reducers/index';
import {loadingUser, userFound, silentRenewError} from './actions/user';
import {getModSubs, getUsersWithNotes} from './actions/snoonotesInfo';
import {getNotesForUsers} from './actions/notes';
import {Log} from 'oidc-client';

const initialState = {user:{user:null,isLoadingUser:false}};


const bg_aliases = {
    [LOGIN]: (action)=>{
        return (dispatch)=>{
            dispatch(loadingUser());
        return userManager.getUser().then((u)=>{
            if(u){
                initUser(dispatch,u);
            }else{
                userManager.signinSilent().then((user)=>{
                    initUser(dispatch,user);
                },(error)=>{
                    dispatch(loadingUser(action._sender.tab.id));
                    userManager.signinRedirect();
                })
            }
        })
        }
    },
    [REDIRECT_SUCCESS]: (req)=>{
        return (dispatch)=>{
        userManager.signinRedirectCallback(req.payload).then((user)=>{
            let state = store.getState();
            chrome.tabs.update(state.user.last_tab_id, {active:true},()=>{
                chrome.tabs.remove(req._sender.tab.id);
            });
            dispatch(userFound(user));
            dispatch(getModSubs());
        },(err)=>{console.warn(err)});
        }
    },
    [REFRESH_USER_ALIAS]: (req)=>{
        return(dispatch)=>{
            dispatch(refreshUser());
            userManager.signinSilent()
            .then((user)=>{
                initUser(dispatch,user);
            },(error)=>{
                console.error('Error getting new token');
                dispatch(sessionTerminated());
            })
        }
    }
}
//const enhancer = applyMiddleware(alias(bg_aliases),thunk, createOidcMiddleware(userManager));
//const store = createStore(reducer,initialState,enhancer);

const initUser = (dispatch, user) =>{
    dispatch(userFound(user));
    dispatch(getModSubs());
    dispatch(getUsersWithNotes());
}

export const store = createStore(reducer,initialState,composeWithDevTools(
    applyMiddleware(alias(bg_aliases),thunk)//, createOidcMiddleware(userManager))
     )
 )

wrapStore(store, { portName: "SnooNotesExtension" });

userManager.getUser().then((u)=>{
    if(u){
        initUser(store.dispatch,u);
    }
    else{
        userManager.signinSilent().then((user)=>{
            initUser(store.dispatch,user);
        },(error)=>{
            console.log('user not logged in')
        });
    }
})

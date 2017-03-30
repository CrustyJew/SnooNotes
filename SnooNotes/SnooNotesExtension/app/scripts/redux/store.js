import { createStore, combineReducers, applyMiddleware, compose } from 'redux';
import { createOidcMiddleware }  from './middleware/oidcMiddleware';
import {userManager} from '../utilities/userManager';
import {composeWithDevTools} from 'remote-redux-devtools'
import thunk from 'redux-thunk'
import { wrapStore, alias } from 'react-chrome-redux';
import {login, LOGIN, REDIRECT_SUCCESS, SILENT_RENEW_SUCCESS} from './actions/user';
import reducer from './reducers/index';
import {loadingUser, userFound, silentRenewError} from './actions/user';
import {getModSubs} from './actions/snoonotesInfo';
import {Log} from 'oidc-client';
import {apiMiddleware} from 'redux-api-middleware';
import {apiHeadersMiddleware} from './middleware/apiHeadersMiddleware';

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
    }
}
//const enhancer = applyMiddleware(alias(bg_aliases),thunk, createOidcMiddleware(userManager));
//const store = createStore(reducer,initialState,enhancer);

const initUser = (dispatch, user) =>{
    dispatch(userFound(user));
    dispatch(getModSubs());
}

export const store = createStore(reducer,initialState,composeWithDevTools(
    applyMiddleware(alias(bg_aliases),thunk,apiHeadersMiddleware,apiMiddleware)//, createOidcMiddleware(userManager))
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

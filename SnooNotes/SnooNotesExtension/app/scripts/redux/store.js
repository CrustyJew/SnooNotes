import { createStore, combineReducers, applyMiddleware, compose } from 'redux';
import { createOidcMiddleware }  from './middleware/oidcMiddleware';
import { UserManager } from 'oidc-client';
import {userManager} from '../utilities/userManager';
import {composeWithDevTools} from 'remote-redux-devtools'
import thunk from 'redux-thunk'
import { wrapStore, alias } from 'react-chrome-redux';
import {login, LOGIN} from './actions/user';
import reducer from './reducers/index';
import {loadingUser, userFound, silentRenewError} from './actions/user';

const initialState = {user:{user:null,isLoadingUser:false}};


const bg_aliases = {
    [LOGIN]: ()=>{
        return (dispatch)=>{
            dispatch(loadingUser());
        userManager.login()
            /*.then((user) => {
                dispatch(userFound(user));
            }, (error)=>{
                dispatch(silentRenewError(error));
            })*/
        
        }
    }
}
//const enhancer = applyMiddleware(alias(bg_aliases),thunk, createOidcMiddleware(userManager));
//const store = createStore(reducer,initialState,enhancer);

const store = createStore(reducer,initialState,composeWithDevTools(
    applyMiddleware(alias(bg_aliases),thunk)//, createOidcMiddleware(userManager))
     )
 )

wrapStore(store, { portName: "SnooNotesExtension" });
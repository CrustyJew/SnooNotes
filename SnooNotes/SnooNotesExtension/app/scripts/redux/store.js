import { createStore, combineReducers, applyMiddleware, compose } from 'redux';
import { createOidcMiddleware }  from './middleware/oidcMiddleware';
import { UserManager } from 'oidc-client';
import {userManager} from '../utilities/userManager';
import composeWithDevTools from 'remote-redux-devtools'

import reducer from './reducers/index';

const initialState = {};
/*
const createStoreWithMiddleware = compose(
    applyMiddleware(createOidcMiddleware(new UserManager(userManagerConfig)))
)(createStore(
window.__REDUX_DEVTOOLS_EXTENSION__ && window.__REDUX_DEVTOOLS_EXTENSION__()));

const store = createStoreWithMiddleware(reducer, initialState);
*/


const store = createStore(reducer,composeWithDevTools(
    applyMiddleware(createOidcMiddleware(userManager))
    )
)


export default store;
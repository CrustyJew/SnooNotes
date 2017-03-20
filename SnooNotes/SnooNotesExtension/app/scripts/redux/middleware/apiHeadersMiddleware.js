import { CALL_API } from 'redux-api-middleware';
import { userManager } from '../../utilities/userManager';

export const apiHeadersMiddleware = store => next => action =>{
    if(!action || !action.hasOwnProperty(CALL_API)){
        return next(action);
    }

    let callAPI = action[CALL_API];
    if(!callAPI.hasOwnProperty('headers')){
        callAPI.headers = {};
    }
    callAPI.headers['Content-Type'] = 'application/json';
    let curState = store.getState();
    if( curState.user && curState.user.access_token){
        callAPI.headers['Authorization'] = 'Bearer ' + curState.user.access_token;
    }

    return next(action);
}
import {CALL_API, getJSON} from 'redux-api-middleware';
import {apiBaseUrl} from '../../config';

export const SET_MOD_SUBS = "SET_MOD_SUBS";
export const SET_USERS_WITH_NOTES = "SET_USERS_WITH_NOTES";

export const getModSubs = () => {
    return {
        [CALL_API]:{
            endpoint : apiBaseUrl + 'Account/GetModeratedSubreddits',
            method: 'GET',
            types: ['REQUEST',
            {
                type: SET_MOD_SUBS,
                payload: (action,state,res)=>{
                   return getJSON(res);
                }
            },'FAILURE']
        }
    }
}
import {CALL_API, getJSON} from 'redux-api-middleware';
import {apiBaseUrl} from '../../config';
import axios from 'axios';

export const SET_MOD_SUBS = "SET_MOD_SUBS";
export const SET_USERS_WITH_NOTES = "SET_USERS_WITH_NOTES";

export const getModSubs = ()=> { 
    return (dispatch) => {
        axios.get('Subreddit')
        .then(response=>{
            dispatch({type: SET_MOD_SUBS, payload: response.data
            });
        });
    }
}

export const getUsersWithNotes = () =>{
    return (dispatch) =>{
        axios.get('Note/GetUsernamesWithNotes')
        .then(response =>{
            dispatch({type: SET_USERS_WITH_NOTES, payload: response.data});
        })
    }
}
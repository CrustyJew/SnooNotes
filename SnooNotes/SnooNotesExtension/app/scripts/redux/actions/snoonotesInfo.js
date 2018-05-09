import axios from 'axios';

export const SET_MOD_SUBS = "SET_MOD_SUBS";
export const SET_USERS_WITH_NOTES = "SET_USERS_WITH_NOTES";

export const getModSubs = () => {
    return (dispatch) => {
        axios.get('Subreddit')
            .then(response => {
                var subs = response.data.reduce((map,s)=>(map[s.SubName.toLowerCase()] = s, map),{});
                dispatch({
                    type: SET_MOD_SUBS, payload: subs
                });
            });
    }
}

export const getUsersWithNotes = () => {
    return (dispatch) => {
        axios.get('Note/GetUsernamesWithNotes')
            .then(response => {
                dispatch({ type: SET_USERS_WITH_NOTES, payload: response.data.map(u => u.toLowerCase()) });
            })
    }
}

export const forceRefresh = (dispatch) => {
    return axios.post('Account/ForceRefresh')
        .then(() => {
            dispatch({ type: "FORCED_REFRESH", payload: {} })
        });
}
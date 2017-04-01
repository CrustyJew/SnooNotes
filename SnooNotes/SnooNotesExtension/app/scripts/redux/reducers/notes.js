import {GET_NOTES_FOR_USERS, GOT_NEW_NOTE} from '../actions/notes';

const initialState = {};

export const notesReducer = (state = initialState, action) => {
    switch(action.type){
        case GET_NOTES_FOR_USERS:{
            return Object.assign({}, ...state, action.payload);
        }
        case GOT_NEW_NOTE:
            return {};
        default: return state;
    }
}

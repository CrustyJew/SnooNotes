import {GET_NOTES_FOR_USERS, GOT_NEW_NOTE} from '../actions/notes';

const initialState = {};

export const notesReducer = (state = initialState, action) => {
    switch(action.type){
        case GET_NOTES_FOR_USERS:{
            return Object.assign({}, state, action.payload);
        }
        case GOT_NEW_NOTE:
            return Object.assign({}, state, {[action.payload.appliesToUsername]: [...state[action.payload.appliesToUsername],action.payload.note]});
        default: return state;
    }
}

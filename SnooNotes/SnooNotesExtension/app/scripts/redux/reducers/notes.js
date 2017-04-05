import {GET_NOTES_FOR_USERS, GOT_NEW_NOTE, DELETE_NOTE, LOADING_NOTES_FOR_USERS} from '../actions/notes';
import {removeProperty} from '../../utilities/immutableFunctions';
const initialState = {};

export const notesReducer = (state = initialState, action) => {
    switch(action.type){
        case GET_NOTES_FOR_USERS:{
            return Object.assign({}, state, action.payload);
        }
        case GOT_NEW_NOTE:
            return Object.assign({}, state, {[action.payload.appliesToUsername]: (state[action.payload.appliesToUsername] || [] ).concat(action.payload.note)});
        case DELETE_NOTE:{
            if(action.payload.outOfNotes){
                return removeProperty(state, action.payload.appliesToUsername);
            }else{
                return Object.assign({},state, 
                    {[action.payload.appliesToUsername]: 
                        state[action.payload.appliesToUsername].filter(note=> note.NoteID != action.payload.noteID)
                    })
            }
        }
        case LOADING_NOTES_FOR_USERS:{
            return Object.assign({}, state, action.payload);
        }
        default: return state;
    }
}

import {SET_MOD_SUBS, SET_USERS_WITH_NOTES} from '../actions/snoonotesInfo';
import {GOT_NEW_NOTE, DELETE_NOTE} from '../actions/notes';
import {USER_SIGNED_OUT} from '../actions/user';

const initialState = {
    modded_subs: [],
    users_with_notes: []
}

export default function snoonotesInfoReducer(state = initialState, action) {
  switch (action.type) {
    case SET_MOD_SUBS:
        return Object.assign({},{...state}, {modded_subs: action.payload});
    case SET_USERS_WITH_NOTES:
        return Object.assign({},{...state},{users_with_notes: action.payload});
    case GOT_NEW_NOTE:{
        if(state.users_with_notes.indexOf(action.payload.appliesToUsername) == -1){
            return Object.assign({},state,{users_with_notes: state.users_with_notes.concat(action.payload.appliesToUsername)});
        }
        return state;
    }
    case DELETE_NOTE:{
        if(action.payload.outOfNotes){
            return Object.assign({},{...state},{users_with_notes: state.users_with_notes.filter(u=>u != action.payload.appliesToUsername)});
        }
        return state;
    }
    case USER_SIGNED_OUT:
      return Object.assign({}, initialState);
    default:
        return state;
  }
}
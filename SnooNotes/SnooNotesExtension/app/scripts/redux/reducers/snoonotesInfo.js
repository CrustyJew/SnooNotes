import {SET_MOD_SUBS, SET_USERS_WITH_NOTES} from '../actions/snoonotesInfo';

const initialState = {
    modded_subs: [],
    users_with_notes: []
}

export default function snoonotesInfoReducer(state = initialState, action) {
  switch (action.type) {
    case SET_MOD_SUBS:
        return Object.assign({},{...state}, {modded_subs:action.payload});
    case SET_USERS_WITH_NOTES:
        return Object.assign({},{...state},{users_with_notes: action.payload});
    default:
        return state;
  }
}
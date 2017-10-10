import { combineReducers } from 'redux';
import userReducer from './user';
import {snoonotesInfoReducer} from './snoonotesInfo';
import { notesReducer } from './notes';
import { optionsReducer } from './options'

const reducer = combineReducers({
    user: userReducer,
    snoonotes_info: snoonotesInfoReducer,
    notes: notesReducer,
    options: optionsReducer
});

export default reducer;
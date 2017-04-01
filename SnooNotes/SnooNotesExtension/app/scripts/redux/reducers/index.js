import { combineReducers } from 'redux';
import userReducer from './user';
import snoonotesInfoReducer from './snoonotesInfo';
import {notesReducer} from './notes';

const reducer = combineReducers({
    user : userReducer,
    snoonotes_info : snoonotesInfoReducer,
    notes : notesReducer
});

export default reducer;
import { combineReducers } from 'redux';
import userReducer from './user';
import snoonotesInfoReducer from './snoonotesInfo';

const reducer = combineReducers({
    user : userReducer,
    snoonotes_info : snoonotesInfoReducer
});

export default reducer;
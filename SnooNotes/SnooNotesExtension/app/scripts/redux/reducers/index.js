import { combineReducers } from 'redux';
import userReducer from './user';

const reducer = combineReducers({
    user : userReducer
});

export default reducer;
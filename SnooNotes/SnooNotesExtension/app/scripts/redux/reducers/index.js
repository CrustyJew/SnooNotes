import { combineReducers } from 'redux';
import userReducer from './user';

const reducer = combineReducers({
    oidc: userReducer
});

export default reducer;
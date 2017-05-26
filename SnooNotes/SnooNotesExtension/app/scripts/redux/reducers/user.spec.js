import deepFreeze from 'deep-freeze';
import {
    USER_EXPIRED,
    REDIRECT_SUCCESS,
    USER_FOUND, USER_NOT_FOUND,
    SILENT_RENEW_ERROR,
    SESSION_TERMINATED,
    LOADING_USER,
    USER_SIGNED_OUT,
    REFRESH_USER
} from '../actions/user';
import userReducer from './user';
let initialState = {
    access_token: null,
    name: "",
    isLoadingUser: false
};

describe('User Reducer', () => {
    it('Should return initial state', () => {
        expect(userReducer(undefined, {})).to.eql(initialState);
    })
    describe('USER_EXPIRED', () => {
        it('Should reset to initial on expire', () => {
            let state = {access_token:'token',name:'testuser',isLoadingUser:false}
            deepFreeze(state);
            expect(userReducer(state, { type: USER_EXPIRED, payload: {} })).to.eql(initialState);
        })
    })
    describe('USER_FOUND', () => {
        it('Should assign values from payload for user', () => {
            let payload = {
                type: USER_FOUND,
                payload: {
                    name: 'user1',
                    access_token: 'asdf',
                    isCabal: true,
                    hasConfig: true,
                    hasWiki: true,
                }
            };
            deepFreeze(payload);
            deepFreeze(initialState);
            let result = userReducer(initialState, payload);
            expect(result).to.eql({
                name: 'user1',
                access_token: 'asdf',
                isCabal: true,
                hasConfig: true,
                hasWiki: true, isLoadingUser: false
            })
        })
    })
    describe('REFRESH_USER',()=>{
        it('Should set user to loading',()=>{
            let payload = {type: REFRESH_USER, payload:{}}
            deepFreeze(payload);
            deepFreeze(initialState);
            expect(userReducer(initialState, payload)).has.property('isLoadingUser').to.equal(true);
        })
    })
})
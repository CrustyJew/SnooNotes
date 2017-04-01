import {notesReducer} from './notes';
import {GET_NOTES_FOR_USERS, GOT_NEW_NOTE} from '../actions/notes';

describe('Notes Reducer',()=>{
    it('Should return initial state',()=>{
        expect(
            notesReducer(undefined,{})
        )
        .toEqual({})
    })
})

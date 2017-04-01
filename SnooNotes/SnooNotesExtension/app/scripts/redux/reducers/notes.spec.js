require('babel-polyfill');
import {notesReducer} from './notes';
import {GET_NOTES_FOR_USERS, GOT_NEW_NOTE} from '../actions/notes';

var notes={
   "videosmods":[
       {"NoteID":45778,
       "NoteTypeID":5,
       "SubName":"videos",
       "Submitter":"Meepster23",
       "Message":"Test",
       "Url":"https://reddit.com//user/videosmods",
       "Timestamp":"2016-07-28T09:30:45.42Z",
       "ParentSubreddit":null}
    ]
}

var newNote ={
    "videosmods":[
       {"NoteID":2,
       "NoteTypeID":4,
       "SubName":"videos",
       "Submitter":"Meepster23",
       "Message":"Test2",
       "Url":"https://reddit.com//user/videosmods",
       "Timestamp":"2017-07-28T09:30:45.42Z",
       "ParentSubreddit":null}
    ]
}

var newNoteNewUser ={
    "videosmods2":[
       {"NoteID":3,
       "NoteTypeID":4,
       "SubName":"videos",
       "Submitter":"Meepster23",
       "Message":"Test3",
       "Url":"https://reddit.com//user/videosmods",
       "Timestamp":"2017-07-28T09:30:45.42Z",
       "ParentSubreddit":null}
    ]
}

describe('Notes Reducer',()=>{
    it('Should return initial state',()=>{
        expect(
            notesReducer(undefined,{})
        )
        .to.eql({})
    })
    it('Should store server loaded notes',()=>{
        expect(
            notesReducer({},{type:GET_NOTES_FOR_USERS, payload:notes})
        )
        .to.eql(notes)
    })
    it('Should override existing notes if reloading from server',()=>{
        expect(
            notesReducer(notes,{type:GET_NOTES_FOR_USERS, payload:newNote})
        )
        .to.eql(newNote)
    })
    it('Should not override existing notes if loading new user from server',()=>{
        let result = notesReducer(notes,{type:GET_NOTES_FOR_USERS, payload:newNoteNewUser});
        expect(result).has.property("videosmods")
        expect(result).has.property("videosmods2")
    })
    it('Should add note from websocket to existing user',()=>{
        expect(
            notesReducer(notes,{type:GOT_NEW_NOTE,payload:{appliesToUsername:"videosmods",note:newNote.videosmods}})
        )
        .has.property("videosmods")
        .to.have.lengthOf(2)
    })
})

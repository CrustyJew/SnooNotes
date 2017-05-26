import deepFreeze from 'deep-freeze';
import { notesReducer } from './notes';
import { GET_NOTES_FOR_USERS, GOT_NEW_NOTE, DELETE_NOTE, LOADING_NOTES_FOR_USERS } from '../actions/notes';


var notes = {
    "videosmods": [
        {
            "NoteID": 45778,
            "NoteTypeID": 5,
            "SubName": "videos",
            "Submitter": "Meepster23",
            "Message": "Test",
            "Url": "https://reddit.com//user/videosmods",
            "Timestamp": "2016-07-28T09:30:45.42Z",
            "ParentSubreddit": null
        }
    ]
}

var newNote = {
    "videosmods": [
        {
            "NoteID": 2,
            "NoteTypeID": 4,
            "SubName": "videos",
            "Submitter": "Meepster23",
            "Message": "Test2",
            "Url": "https://reddit.com//user/videosmods",
            "Timestamp": "2017-07-28T09:30:45.42Z",
            "ParentSubreddit": null
        }
    ]
}

var newNoteNewUser = {
    "videosmods2": [
        {
            "NoteID": 3,
            "NoteTypeID": 4,
            "SubName": "videos",
            "Submitter": "Meepster23",
            "Message": "Test3",
            "Url": "https://reddit.com//user/videosmods",
            "Timestamp": "2017-07-28T09:30:45.42Z",
            "ParentSubreddit": null
        }
    ]
}

describe('Notes Reducer', () => {
    it('Should return initial state', () => {
        expect(
            notesReducer(undefined, {})
        )
            .to.eql({})
    })
    describe('GET_NOTES_FOR_USERS', () => {
        it('Should store server loaded notes', () => {
            var state = {};
            deepFreeze(state);
            expect(
                notesReducer(state, { type: GET_NOTES_FOR_USERS, payload: notes })
            )
                .to.eql(notes)
        })
        it('Should override existing notes if reloading from server', () => {
            deepFreeze(notes);
            expect(
                notesReducer(notes, { type: GET_NOTES_FOR_USERS, payload: newNote })
            )
                .to.eql(newNote)
        })
        it('Should not override existing notes if loading new user from server', () => {
            deepFreeze(notes);
            let result = notesReducer(notes, { type: GET_NOTES_FOR_USERS, payload: newNoteNewUser });
            expect(result).has.property("videosmods")
            expect(result).has.property("videosmods2")
        })
    })
    describe('GOT_NEW_NOTE', () => {
        it('Should add note from websocket to existing user', () => {
            deepFreeze(notes);
            let results = notesReducer(notes, { type: GOT_NEW_NOTE, payload: { appliesToUsername: "videosmods", note: newNote.videosmods } });
            expect(results)
                .has.property("videosmods")
                .to.have.lengthOf(2)
            expect(results.videosmods[0]).to.eql(notes.videosmods[0])
            expect(results.videosmods[1]).to.eql(newNote.videosmods[0])
            //expect(results)
        })
        it('Should create a new user entry if empty', () => {
            deepFreeze(newNote);
            let results = notesReducer({}, { type: GOT_NEW_NOTE, payload: { appliesToUsername: "videosmods", note: newNote.videosmods } });
            expect(results)
                .has.property("videosmods")
                .to.have.lengthOf(1);
        })
    })
    describe('DELETE_NOTE', () => {
        it('Should remove user if ran out of notes', () => {
            deepFreeze(notes);
            expect(
                notesReducer(notes, { type: DELETE_NOTE, payload: { appliesToUsername: "videosmods", noteID: 45778, outOfNotes: true } }))
                .not.has.property("videosmods");
        })
        it('Should remove note from array', () => {
            let state = Object.assign({}, notes, { "videosmods": notes.videosmods.concat(newNote.videosmods) });
            deepFreeze(state);
            expect(
                notesReducer(state, { type: DELETE_NOTE, payload: { appliesToUsername: "videosmods", noteID: 45778, outOfNotes: false } })
            )
                .has.property("videosmods")
                .eql(newNote.videosmods);
        })
    })
    describe('LOADING_NOTES_FOR_USERS', () => {
        it('Should add user with loading status if they do not exist in notes object', () => {
            let state = {};
            deepFreeze(state);
            expect(
                notesReducer(state, { type: LOADING_NOTES_FOR_USERS, payload: { videosmods: { loading: true } } })
            )
                .has.property('videosmods')
                .has.property('loading')
        })
        it('Should remove existing notes from loading user', () => {
            deepFreeze(notes);
            expect(
                notesReducer(notes, { type: LOADING_NOTES_FOR_USERS, payload: { videosmods: { loading: true } } })
            )
                .has.property('videosmods')
                .has.property('loading');
        });
    })
})

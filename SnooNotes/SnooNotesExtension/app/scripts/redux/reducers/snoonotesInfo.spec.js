import deepFreeze from 'deep-freeze';
import { snoonotesInfoReducer } from './snoonotesInfo';
import { SET_MOD_SUBS, SET_USERS_WITH_NOTES } from '../actions/snoonotesInfo';
import { GOT_NEW_NOTE, DELETE_NOTE } from '../actions/notes';
const initialInfo = {
    modded_subs: [{ "SubredditID": 1, "SubName": "testsub1", "Active": true, "SentinelActive": false, "BotSettings": null, "Settings": { "AccessMask": 79, "NoteTypes": [{ "NoteTypeID": 39, "SubName": "testsub1", "DisplayName": "Good Contributor", "ColorCode": "008000", "DisplayOrder": 0, "Bold": false, "Italic": false, "IconString": null, "Disabled": false }] } }],
    users_with_notes: ['user1']
}

describe('SnooNotesInfo Reducer', () => {
    it('Should return initial state', () => {
        let result = snoonotesInfoReducer(undefined, {});
        expect(result).has.property('modded_subs').to.have.lengthOf(0)
        expect(result).has.property('users_with_notes').to.have.lengthOf(0)
    })
    describe('SET_MOD_SUBS', () => {
        it('Should change modded subs', () => {
            deepFreeze(initialInfo);
            const newsub = [{ "SubredditID": 2, "SubName": "testsub2", "Active": true, "Settings": { "AccessMask": 80, "NoteTypes": [{ "NoteTypeID": 40, "SubName": "testsub2", "DisplayName": "asdf", "ColorCode": "111111", "DisplayOrder": 0, "Bold": false, "Italic": false, "IconString": null, "Disabled": false }] } }]
            deepFreeze(newsub);
            let result = snoonotesInfoReducer(initialInfo, { type: SET_MOD_SUBS, payload: newsub });
            expect(result).has.property('modded_subs').to.have.lengthOf(1);
            expect(result.modded_subs[0]).to.eql(newsub[0]);
            expect(result).has.property('users_with_notes').to.have.lengthOf(1);
            expect(result.users_with_notes[0]).to.equal('user1');
        })
    })

    describe('SET_USERS_WITH_NOTES', () => {
        it('Should change users with notes', () => {
            deepFreeze(initialInfo);
            const newusers = ['user2'];
            deepFreeze(newusers);
            let result = snoonotesInfoReducer(initialInfo, { type: SET_USERS_WITH_NOTES, payload: newusers });
            expect(result).has.property('modded_subs').to.have.lengthOf(1);
            expect(result.modded_subs[0]).to.eql(initialInfo.modded_subs[0]);
            expect(result).has.property('users_with_notes').to.have.lengthOf(1);
            expect(result.users_with_notes[0]).to.equal('user2');
        })
    })

    describe('GOT_NEW_NOTE', () => {
        it('Should add a new user', () => {
            deepFreeze(initialInfo);
            const newNotePayload = { appliesToUsername: "newuser1", note: {} };
            deepFreeze(newNotePayload)
            let result = snoonotesInfoReducer(initialInfo, { type: GOT_NEW_NOTE, payload: newNotePayload });
            expect(result).has.property('modded_subs').to.have.lengthOf(1);
            expect(result.modded_subs[0]).to.eql(initialInfo.modded_subs[0]);
            expect(result).has.property('users_with_notes').to.have.lengthOf(2);
            expect(result.users_with_notes).to.eql([initialInfo.users_with_notes[0], 'newuser1']);
        })
        it('Should not re-add old user', () => {
            deepFreeze(initialInfo);
            const newNotePayload = { appliesToUsername: initialInfo.users_with_notes[0], note: {} };
            deepFreeze(newNotePayload)
            let result = snoonotesInfoReducer(initialInfo, { type: GOT_NEW_NOTE, payload: newNotePayload });
            expect(result).has.property('modded_subs').to.have.lengthOf(1);
            expect(result.modded_subs[0]).to.eql(initialInfo.modded_subs[0]);
            expect(result).has.property('users_with_notes').to.have.lengthOf(1);
            expect(result.users_with_notes).to.eql([initialInfo.users_with_notes[0]]);
        })
    })

    describe('DELETE_NOTE', () => {
        it('Should delete user after last note', () => {
            deepFreeze(initialInfo);
            const payload = {
                type: DELETE_NOTE,
                payload: {
                    appliesToUsername: 'user1',
                    noteID: 1,
                    'outOfNotes': true
                }
            }
            deepFreeze(payload);
            let result = snoonotesInfoReducer(initialInfo, payload);
            expect(result).has.property('modded_subs').to.have.lengthOf(1);
            expect(result.modded_subs[0]).to.eql(initialInfo.modded_subs[0]);
            expect(result).has.property('users_with_notes').to.have.lengthOf(0);
        })
        it('Should not delete users with more notes',()=>{
            deepFreeze(initialInfo);
            const payload = {
                type: DELETE_NOTE,
                payload: {
                    appliesToUsername: 'user1',
                    noteID: 1,
                    'outOfNotes': false
                }
            }
            deepFreeze(payload);
            let result = snoonotesInfoReducer(initialInfo, payload);
            expect(result).has.property('modded_subs').to.have.lengthOf(1);
            expect(result.modded_subs[0]).to.eql(initialInfo.modded_subs[0]);
            expect(result).has.property('users_with_notes').to.have.lengthOf(1);
        })
    })
})
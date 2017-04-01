import {CALL_API, getJSON} from 'redux-api-middleware';
import {apiBaseUrl} from '../../config';

export const GET_NOTES_FOR_USERS = "GET_NOTES_FOR_USERS";
export const GOT_NEW_NOTE = "GOT_NEW_NOTE";

export const getNotesForUsers = (usernames) =>{
    return {
        [CALL_API]:{
            endpoint : apiBaseUrl + 'Note/GetNotes',
            method: 'POST',
            types: [{type:'REQUEST', payload:{request:"GetNotesForUsers",users:usernames}},
            {
                type: GET_NOTES_FOR_USERS,
                payload: (action,state,res)=>{
                   return getJSON(res);
                }
            },'FAILURE'],
            body: JSON.stringify(usernames)
        }
    }
}

export const gotNewNote = (note) =>{
    return{
        type: GOT_NEW_NOTE,
        payload:{
            [note.AppliesToUsername]:{
                NoteID: note.NoteID,
                NoteTypeID: note.NoteTypeID,
                SubName: note.SubName,
                Submitter: note.Submitter,
                Message: note.Message,
                Url: note.Url,
                Timestamp: note.Timestamp,
                ParentSubreddit: note.ParentSubreddit
            }
        }
    }
}
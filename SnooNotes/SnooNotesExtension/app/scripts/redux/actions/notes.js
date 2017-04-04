import {CALL_API, getJSON} from 'redux-api-middleware';
import {apiBaseUrl} from '../../config';
import axios from 'axios';

export const GET_NOTES_FOR_USERS = "GET_NOTES_FOR_USERS";
export const GOT_NEW_NOTE = "GOT_NEW_NOTE";
export const DELETE_NOTE = "DELETE_NOTE";

export const getNotesForUsers = (usernames) =>{
    return (dispatch)=>{
        axios.post('Note/GetNotes',usernames)
            .then(res=>{
                dispatch({type: GET_NOTES_FOR_USERS, payload:res.data
                });
            });
    }
}

export const gotNewNote = (note) =>{
    return{
        type: GOT_NEW_NOTE,
        payload:{
            appliesToUsername:note.AppliesToUsername,
            note: {
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

export const gotDeleteNote = (username,id,outOfNotes) =>{
    return {
        type: DELETE_NOTE,
        payload:{
            appliesToUsername: username,
            noteID:id,
            'outOfNotes': outOfNotes
        }
    }
}
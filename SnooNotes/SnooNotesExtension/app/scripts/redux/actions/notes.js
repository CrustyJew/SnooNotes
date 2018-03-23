import axios from 'axios';

export const GET_NOTES_FOR_USERS = "GET_NOTES_FOR_USERS";
export const ALIAS_GET_NOTES_FOR_USERS = "ALIAS" + GET_NOTES_FOR_USERS;
export const GOT_NEW_NOTE = "GOT_NEW_NOTE";
export const DELETE_NOTE = "DELETE_NOTE";
export const LOADING_NOTES_FOR_USERS = "LOADING_NOTES_FOR_USERS";

export const getNotesForUsers = (dispatch, usernames) => {
    let users = usernames.map((u) => { return { [u]: { loading: true } } });
    let loadingPayload = Object.assign({}, ...users);
    dispatch({ type: LOADING_NOTES_FOR_USERS, payload: loadingPayload });

    axios.post('Note/GetNotes', usernames)
        .then(res => {
            var obj = res.data;
            var keys = Object.keys(obj);
            var n = keys.length;
            var newobj={};
            while (n--) {
                var key = keys[n];
                newobj[key.toLowerCase()] = obj[key];
            }
            dispatch({
                type: GET_NOTES_FOR_USERS, payload: newobj
            });
        });

}

export const gotNewNote = (note) => {
    return {
        type: GOT_NEW_NOTE,
        payload: {
            appliesToUsername: note.AppliesToUsername.toLowerCase(),
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

export const gotDeleteNote = (username, id, outOfNotes) => {
    return {
        type: DELETE_NOTE,
        payload: {
            appliesToUsername: username.toLowerCase(),
            noteID: id,
            'outOfNotes': outOfNotes
        }
    }
}
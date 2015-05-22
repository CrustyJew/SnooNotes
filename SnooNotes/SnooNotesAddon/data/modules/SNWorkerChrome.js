function browserInit() {
    (function (snBrowser) {
        snBrowser.newNoteNewUser = function (req) {
            newNoteNewUser(req);
        }
        snBrowser.newNoteExistingUser = function (req) {
           newNoteExistingUser(req);
        }
        snBrowser.deleteNoteAndUser = function (req) {
            deleteNoteAndUser(req);
        }
        snBrowser.deleteNote = function (req) {
            deleteNote(req);
        }
        snBrowser.workerInitialized = function (req) {
            workerInitialized(req);
        }
        snBrowser.gotUsersWithNotes = function (req) {
           gotUsersWithNotes(req);
        }
        snBrowser.sendUserNotes = function (req) {
            sendUserNotes(req);
        }
        //listeners
        

    }(snBrowser = window.snUtil || {}));
}
function browserInit() {
    (function (snBrowser) {
        snBrowser.requstUserNotes = function(users){
            self.port.emit("requestUserNotes", users);
        }
        snBrowser.loggedIn = function () {
            self.port.emit("loggedIn");
        }
        //Listeners

        self.port.on("gotUsersWithNotes", function (users) {
            snUtil.setUsersWithNotes(users);
        });
        self.port.on("receiveUserNotes", function (notes) {
            processEntries(notes); //SNLoad.js
        });
        self.port.on("updateUsersWithNotes", function (req) {
            snUtil.updateUsersWithNotes(req);
        });
        self.port.on("reinitWorker", function () {
            snUtil.reinitWorker();
        });
        self.port.on("newNoteExistingUser", function (req) {
            newNoteExistingUser(req); //snoonotes.js
        });
        self.port.on("newNoteNewUser", function (req) {
            newNoteNewUser(req); //snoonotes.js
        });
        self.port.on("deleteNoteAndUser", function (req) {
            deleteNoteAndUser(req); //snoonotes.js
        });
        self.port.on("deleteNote", function (req) {
            deleteNote(req); //snoonotes.js
        });
        self.port.on("setNoteTypeCSS", function (css) {
            snUtil.NoteStyles.innerHTML = css;
        });
        self.port.on("setNoteTypeJSON", function (json) {
            snUtil.NoteTypes = json;
        });
    }(snBrowser = window.snUtil || {}));
   
}
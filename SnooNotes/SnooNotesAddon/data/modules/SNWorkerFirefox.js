function browserInit() {
    (function (snBrowser) {
        snBrowser.newNoteNewUser = function (req) {
            self.port.emit("newNoteNewUser", req);
        }
        snBrowser.newNoteExistingUser = function (req) {
            self.port.emit("newNoteExistingUser", req);
        }
        snBrowser.deleteNoteAndUser = function (req) {
            self.port.emit("deleteNoteAndUser",req);
        }
        snBrowser.deleteNote = function (req) {
            self.port.emit("deleteNote", req);
        }
        snBrowser.workerInitialized = function (req) {
            self.port.emit("workerInitialized", req);
        }
        snBrowser.gotUsersWithNotes = function (req) {
            self.port.emit("gotUsersWithNotes", req);
        }
        snBrowser.sendUserNotes = function (req) {
            self.port.emit("sendUserNotes", req);
        }
        //listeners
        self.port.on("initWorker", initWorker);
        self.port.on("requestUserNotes", requestUserNotes);
        self.port.on("closeSocket", closeSocket);
        self.port.on("openSocket", initSocket);
    }(snBrowser = window.snUtil || {}));
}
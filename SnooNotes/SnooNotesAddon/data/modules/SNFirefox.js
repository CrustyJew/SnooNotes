function browserInit() {
    (function (snBrowser) {
        snBrowser.requstUserNotes = function(users){
            self.port.emit("requestUserNotes", users);
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
    }(snBrowser = window.snUtil || {}));
   
}
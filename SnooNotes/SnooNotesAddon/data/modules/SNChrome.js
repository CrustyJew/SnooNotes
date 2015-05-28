function browserInit() {
    
    (function (snBrowser) {
        snBrowser.requstUserNotes = function(users){
            chrome.runtime.sendMessage({"method":"requestUserNotes","users": users});
        }
        snBrowser.loggedIn = function () {
            chrome.runtime.sendMessage({ "method": "loggedIn" });
        }

        //Listeners
        chrome.runtime.onMessage.addListener(function(request,sender,sendResponse){
            switch (request.method){
                case "gotUsersWithNotes":
                    snUtil.setUsersWithNotes(request.users);
                    break;
                case "receiveUserNotes" :
                    processEntries(request.notes); //SNLoad.js
                    break;
                case "updateUsersWithNotes" :
                    snUtil.updateUsersWithNotes(request.req);
                    break;
                case "reinitWorker" :
                    //snUtil.reinitWorker();
                    break;
                case "newNoteExistingUser":
                    newNoteExistingUser(request.req); //snoonotes.js
                    break;
                case "newNoteNewUser" :
                    newNoteNewUser(request.req); //snoonotes.js
                    break;
                case "deleteNoteAndUser" :
                    deleteNoteAndUser(request.req); //snoonotes.js
                    break;
                case "deleteNote" :
                    deleteNote(request.req); //snoonotes.js
                    break;
                case "setNoteTypeCSS":
                    snUtil.NoteStyles.innerHTML = request.css;
                default:
                    break;
            }
        });

        chrome.runtime.sendMessage({ "method": "getUsersWithNotes" }, function (users) {
            if (users) {
                snUtil.setUsersWithNotes(users);
            }
        });
        chrome.runtime.sendMessage({ "method": "getNoteTypeCSS" }, function (css) {
            snUtil.NoteStyles.innerHTML = css;
        });
        
    }(snBrowser = window.snUtil || {}));
   
}
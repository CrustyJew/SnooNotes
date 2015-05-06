function initSnooNotes() {
    (function (snUtil) {
        //snUtil.ApiBase = "https://snoonotesapi.azurewebsites.net/api/";
        snUtil.ApiBase = "https://localhost:44311/api/";
        //snUtil.LoginAddress = "https://snoonotesapi.azurewebsites.net/Auth/Login";
        snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
        
        self.port.on("gotUsersWithNotes", function (users) {
            if (!users) {
                return;
            }
            snUtil.UsersWithNotes = "," + users.join(",") + ","; //I hate stupid arrays and not being able to case-insensitive searches!
        });
        //received data from socket to add/remove a user
        self.port.on("updateUsersWithNotes", function (req) {
            var user = req.user;
            if (req.remove) {
                console.log("removed user");
                snUtil.UsersWithNotes = snUtil.UsersWithNotes.replace(","+user+",",","); //E-i-E-i-ooooooo
            }
            else if (req.add) {
                console.log("Added user");
                snUtil.UsersWithNotes = snUtil.UsersWithNotes + ","+user+",";
            }
        });
        snUtil.GetNotesForUsers = function (users) {
            self.port.emit("requestUserNotes", users);
        };
        self.port.on("receiveUserNotes", function (notes) {
            processEntries(notes); //SNLoad.js
        });

        //do this lateish so we get all the listeners hooked up first
        if (!snUtil.LoggedIn) checkLoggedIn();
        var sub = /reddit\.com\/r\/[a-z0-9]*\/?/i.exec(window.location);
        snUtil.Subreddit = !sub ? "" : sub[0].substring(13).replace(/\//g, '');
        
        window.snUtil = snUtil;
        return;
    }(snUtil = window.snUtil || {}));
}

function setModdedSubs(){
    $.ajax({
        url: snUtil.ApiBase + "Account/GetModeratedSubreddits",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.ModdedSubs = "," + d.join(",") + ",";
            var event = new CustomEvent("snUtilDone");
            window.dispatchEvent(event);
        },
        error: handleAjaxError
    });
    return;
}
function checkLoggedIn() {
    $.ajax({
        url: snUtil.ApiBase + "Account/IsLoggedIn",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.LoggedIn = true;
            self.port.emit("loggedIn");
            if (!snUtil.ModdedSubs) setModdedSubs();
        },
        error: handleAjaxError
    });
}
function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if(jqXHR.status === 401)
    {
        showLoginPopup();
    }
}

(function () {
    //window.addEventListener("snReadyToInit", function () {
    self.port.on("reinitWorker", function () {
        //initSnooNotes();
        var event = new CustomEvent("snUtilDone");
        window.dispatchEvent(event);
    });
    initSnooNotes();
    
    //});
})();
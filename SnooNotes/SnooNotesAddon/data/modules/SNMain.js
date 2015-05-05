function initSnooNotes() {
    (function (snUtil) {
        //snUtil.ApiBase = "https://snoonotesapi.azurewebsites.net/api/";
        snUtil.ApiBase = "https://localhost:44311/api/";
        //snUtil.LoginAddress = "https://snoonotesapi.azurewebsites.net/Auth/Login";
        snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
        if (!snUtil.LoggedIn) snUtil.LoggedIn = checkLoggedIn();
        self.port.on("gotUsersWithNotes", function (users) {
            snUtil.UsersWithNotes = users;
        });
        //received data from socket to add/remove a user
        self.port.on("updateUsersWithNotes", function (user) {
            var ind = snUtil.UsersWithNotes.indexOf(user);
            if (ind != -1 && user.remove) {
                console.log("removed user");
                snUtil.UsersWithNotes.splice(ind, 1);
            }
            else if (ind === -1 && user.add) {
                console.log("Added user");
                snUtil.UsersWithNotes.push(user);
            }
        });
        //if it's logged in, go ahead and set up things that depend on a login.
        if (snUtil.LoggedIn) {
            if (!snUtil.ModdedSubs) setModdedSubs();
        }
        var sub = /reddit\.com\/r\/[a-z0-9]*\/?/i.exec(window.location);
        snUtil.Subreddit = !sub ? "" : sub[0].substring(13).replace(/\//g, '');
        snUtil.GetNotesForUsers = function (users) {
            self.port.emit("requestUserNotes", users);
        };
        self.port.on("receiveUserNotes", function (notes) {
            processEntries(notes); //SNLoad.js
        });
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
            snUtil.ModdedSubs = d;
            
        },
        error: handleAjaxError
    });
    return;
}
function checkLoggedIn() {
    $.ajax({
        url: snUtil.ApiBase + "Account/IsLoggedIn",
        method: "GET",
        async: false,
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.LoggedIn = true;
            self.port.emit("loggedIn")
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
    
    initSnooNotes();
    var event = new CustomEvent("snUtilDone");
    window.dispatchEvent(event);
    //});
})();
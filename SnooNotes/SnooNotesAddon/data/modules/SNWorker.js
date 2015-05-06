﻿
function initSocket() {
    console.log("connecting socket");
    var snUpdate = $.connection.SnooNoteUpdates;
    snUpdate.client.receiveUpdate = function (note) { console.log(note.toString()); }
    $.connection.hub.start().done(function () { console.log('Connected socket'); });

}
(function (snUtil) {
    //snUtil.ApiBase = "https://snoonotesapi.azurewebsites.net/api/";
    snUtil.ApiBase = "https://localhost:44311/api/";
    //snUtil.LoginAddress = "https://snoonotesapi.azurewebsites.net/Auth/Login";
    snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
    window.snUtil = snUtil;
    self.port.on("initWorker", initWorker);
    self.port.on("requestUserNotes",requestUserNotes)
    return;
}(snUtil = window.snUtil || {}));

function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if (jqXHR.status === 401) {
        //do nothing on worker page, wait for user to login
        
    }
    console.log("ERROR: "+textStatus);
}
function initWorker() {
    console.log("initWorker");
    getUsersWithNotes();
    $.ajax({
        url: snUtil.ApiBase + "Note/InitializeNotes",
        method: "GET",
        async:false,
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            initNoteData(d);
            initSocket();
            console.log("pageWorker: initialized")
            self.port.emit("workerInitialized", {});
        },
        error: handleAjaxError
    });
}

function initNoteData(data) {
    console.log("Building Note Worker HTML");
    $('body').empty();
    for (var key in data) {
        var udata = data[key];
        var unoterows = "";

        for (var i = 0; i < udata.length; i++) {
            var note = udata[i];
            unoterows += '<tr id="' + note.NoteID + '" class="' + note.SubName.toLowerCase() + note.NoteTypeID + '">' +
                '<td><a href="//r//'+note.SubName+'">' + note.SubName + '</span>' +
                '<td><span>' + note.Submitter + '</span><a href="' + note.Url + '">' + note.Timestamp + '</a></td>' +
                '<td><p>' + note.Message + '</p></td></tr>';
        }
        var $user = $('' +
       '<div id="SnooNote-' + key.toLowerCase() + '" style="display:none;">' +
       '<table>' + unoterows + '</table>' +
       '</div>');
        $('body').append($user);
    }
}

function getUsersWithNotes() {
    console.log("Getting users with notes");
    $.ajax({
        url: snUtil.ApiBase + "Note/GetUsernamesWithNotes",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.UsersWithNotes = d;
            self.port.emit("gotUsersWithNotes", d);
        },
        error: handleAjaxError
    });
}

function requestUserNotes(req) {
    console.log("Fetching notes for " + req.users.length + " users");
    if (req.users.length <= 0) {
        return;
    }
    var usersIDs = req.users.join('|').toLowerCase();
    usersIDs = usersIDs.replace('|', ',#SnooNote-');
    usersIDs = "#SnooNote-" + usersIDs;
    var usersHTML = "";
    $(usersIDs).each(function (index, $ent) {
        usersHTML += $ent.outerHTML;
    });
   
    self.port.emit("sendingUserNotes", { "notes": usersHTML, "worker": req.worker });
}
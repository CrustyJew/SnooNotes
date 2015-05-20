
var socketOpen = false;
function initSocket() {
    console.log("connecting socket");
    socketOpen = true;
    var snUpdate = $.connection.SnooNoteUpdates;
    snUpdate.client.receiveUpdate = function (note) { console.log(note.toString()); }
    snUpdate.client.addNewNote = function (note) {
        
        var $user = $('#SnooNote-'+note.AppliesToUsername.toLowerCase());
        if ($user.length == 0) {
            console.log("Gots a new note for a brand new user");
            //brand spankin new user
            var notecont = generateNoteContainer(note.AppliesToUsername, generateNoteRow(note));
            $('body').append(notecont);
            self.port.emit("newNoteNewUser", { "user": note.AppliesToUsername.toLowerCase(), "note": notecont[0].outerHTML });
        }
        else {
            console.log("Gots a new note for an existing user");
            //user exists, shove the new note in there
            var noterow = generateNoteRow(note);
            $('table',$user).append(noterow);
            self.port.emit("newNoteExistingUser", { "user": note.AppliesToUsername.toLowerCase(), "note": noterow });
        }
    }

    snUpdate.client.deleteNote = function (user,noteID) {
        console.log("Removing a note!");
        var $note = $('tr#SN' + noteID);
        var $user = $note.closest('div');
        $note.remove();

        if ($('tr', $user).length == 0) {
            console.log("User dun run outta notes, so removing it too");
            $user.remove();
            self.port.emit("deleteNoteAndUser", { "user": user, "noteID": noteID });
        }
        else {
            self.port.emit("deleteNote", { "user": user, "noteID": noteID });
        }

    }
    $.connection.hub.disconnected(function () {
        if (socketOpen) {
            setTimeout(function () {
                $.connection.hub.start();
            }, 5000); // Restart connection after 5 seconds.
        }
    });
    $.connection.hub.start().done(function () { console.log('Connected socket'); });

}
(function (snUtil) {
    snUtil.ApiBase = "https://snoonotes.com/api/";
    //snUtil.ApiBase = "https://localhost:44311/api/";
    snUtil.LoginAddress = "https://snoonotes.com/Auth/Login";
    //snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
    window.snUtil = snUtil;
    self.port.on("initWorker", initWorker);
    self.port.on("requestUserNotes", requestUserNotes);
    self.port.on("closeSocket", closeSocket);
    self.port.on("openSocket", initSocket);
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
function closeSocket() {
    socketOpen = false;
    $.connection.hub.stop();
}
function initNoteData(data) {
    console.log("Building Note Worker HTML");
    $('body').empty();
    for (var key in data) {
        var udata = data[key];
        var unoterows = "";

        for (var i = 0; i < udata.length; i++) {
            var note = udata[i];
            unoterows += generateNoteRow(note);
        }
        $('body').append(generateNoteContainer(key, unoterows));
    }
}
function generateNoteContainer(user, notes) {
    var $usernote = $('' +
      '<div id="SnooNote-' + user.toLowerCase() + '" class="SNViewContainer" style="display:none;">' +
      '<div class="SNHeader"><a class="SNCloseNote">Close [x]</a></div>' +
      '<table>' + notes + '</table>' +
      '<div class="SNNewNote"><textarea placeholder="Add a new note for user..." class="SNNewMessage" /><button type="button" class="SNNewNoteSubmit" SNUser="'+user.toLowerCase()+'">Submit</button>  </div>' +
      '</div>');
    return $usernote;
}
function generateNoteRow(note) {
    return '<tr id="SN' + note.NoteID + '" class="' + note.SubName.toLowerCase() + note.NoteTypeID + '">' +
                '<td class="SNSubName"><a href="https://reddit.com/r/'+note.SubName+'">' + note.SubName + '</span>' +
                '<td class="SNSubmitter"><span>' + note.Submitter + '</span><br /><a href="' + note.Url + '">' + new Date(note.Timestamp).toLocaleString().replace(', ','<br />') + '</a></td>' +
                '<td class="SNMessage"><p>' + note.Message + '</p><a class="SNDeleteNote">[x]</a></td></tr>';
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
    usersIDs = usersIDs.replace(/\|/g, ',#SnooNote-');
    usersIDs = "#SnooNote-" + usersIDs;
    var usersHTML = "";
    $(usersIDs).each(function (index, $ent) {
        usersHTML += $ent.outerHTML;
    });
   
    self.port.emit("sendingUserNotes", { "notes": usersHTML, "worker": req.worker });
}
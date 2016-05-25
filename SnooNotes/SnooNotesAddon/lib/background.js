var waitingToClose;
var loggedIn = false;
var usersWithNotes = [];
var cssReady = false;

var socketOpen = false;
function initSocket() {
    console.log("connecting socket");
    socketOpen = true;
    var snUpdate = $.connection.SnooNoteUpdates;
    snUpdate.client.receiveUpdate = function (note) { console.log(note.toString()); }
    snUpdate.client.addNewNote = function (note) {
        var user = note.AppliesToUsername.toLowerCase();
        var i = usersWithNotes.indexOf(user);
        var newUser = false;
        if (i < 0) {
            newUser = true;
            usersWithNotes.push(user);
        }

        if (newUser) {
            console.log("Gots a new note for a brand new user");

            //brand spankin new user
            var notecont = generateNoteContainer(user, generateNoteRow(note));

            newNoteNewUser({ "user": user, "note": notecont });

        }
        else {
            console.log("Gots a new note for an existing user");
            //user exists, shove the new note in there
            var noterow = generateNoteRow(note);

            newNoteExistingUser({ "user": user, "note": noterow });

        }
    }

    snUpdate.client.deleteNote = function (user, noteID, lastNoteForSub) {
        console.log("Removing a note!");
        user = user.toLowerCase();
        if (lastNoteForSub) {
            console.log("User might have run outta notes, so getting a fresh list");
            checkUserHasNotes(user).then(function (d) {
                if (d) {
                    console.log("User still has notes in other subs");
                    deleteNote({ "user": user, "noteID": noteID })
                }
                else {
                    console.log("User is outta notes in all subs");
                    var i = usersWithNotes.indexOf(user);
                    if (i > -1) usersWithNotes.splice(i, 1);

                    deleteNoteAndUser({ "user": user, "noteID": noteID });
                }
            })
        }
        else {
            deleteNote({ "user": user, "noteID": noteID })
        }

    }
    snUpdate.client.reinitAll = function () {
        initWorker();
    }
    snUpdate.client.refreshNoteTypes = function () {
        getNoteTypes();
    }
    $.connection.hub.disconnected(function () {
        console.log('Socket Disconnected');
        if (socketOpen) {
            setTimeout(function () {
                $.connection.hub.start().then(function () { console.log('Connected socket'); }, function (e) { console.log(e.toString()) });
            }, 5000); // Restart connection after 5 seconds.
        }
    });
    $.connection.hub.start().then(function () { console.log('Connected socket'); }, function (e) { console.log(e.toString()) });

}
(function (snUtil) {
    //snUtil.ApiBase = "https://snoonotes.com/api/";
    //snUtil.LoginAddress = "https://snoonotes.com/Auth/Login";
    snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
    snUtil.ApiBase = "https://localhost:44311/api/";

    snUtil.NoteStyles = document.createElement('style');
    document.head.appendChild(snUtil.NoteStyles);

    return;
}(snUtil = this.snUtil || {}));

function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if (jqXHR.status === 401) {
        //do nothing on worker page, wait for user to login

    }
    console.log("ERROR: " + textStatus);
}
function initWorker() {
    console.log("initWorker");
    getUsersWithNotes();
    getNoteTypes();
    initSocket();
    console.log("pageWorker: initialized");
    workerInitialized({});
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
    var usernote = '' +
      '<div id="SnooNote-' + user.toLowerCase() + '" class="SNViewContainer" style="display:none;">' +
        '<div class="SNHeader"><a class="SNClose SNCloseNote">Close [x]</a></div>' +
        '<table>' + notes + '</table>' +
        '<div class="SNNewNoteContainer">' +
            '<div class="SNNewNote"><textarea placeholder="Add a new note for user..." class="SNNewMessage" /><button type="button" class="SNNewNoteSubmit" SNUser="' + user.toLowerCase() + '">Submit</button>  </div>' +
            '<div class="SNNoteType"></div>' +
            '<div class="SNNewError"></div>' +
        '</div>' +
      '</div>';
    return usernote;
}
function generateNoteRow(note) {
    return '<tr id="SN' + note.NoteID + '" class="SN' + note.SubName.toLowerCase() + note.NoteTypeID + '">' +
                '<td class="SNSubName"><a href="https://reddit.com/r/' + note.SubName + '">' + note.SubName + '</a>' +
                (note.ParentSubreddit ? '<br>via<br><a href="https://reddit.com/r/' + note.ParentSubreddit + '">' + note.ParentSubreddit + '</a>' : '') +
                '<td class="SNSubmitter"><span>' + note.Submitter + '</span><br /><a href="' + note.Url + '">' + new Date(note.Timestamp).toLocaleString().replace(', ', '<br />') + '</a></td>' +
                '<td class="SNMessage"><p>' + note.Message + '</p><a class="SNDeleteNote">[x]</a></td></tr>';
}

function getUsersWithNotes() {
    console.log("Getting users with notes");
    $.ajax({
        url: snUtil.ApiBase + "Note/GetUsernamesWithNotes",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            usersWithNotes = d.join("|").toLowerCase().split("|");
            gotUsersWithNotes(usersWithNotes);
        },
        error: handleAjaxError
    });
}

function checkUserHasNotes(user) {
    console.log("Checking that '" + user + "' has notes");
    return $.ajax({
        url: snUtil.ApiBase + "Note/" + user + "/HasNotes",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
}

function requestUserNotes(req) {
    console.log("Fetching notes for " + req.users.length + " users");
    if (req.users.length <= 0) {
        return;
    }
    $.ajax({
        url: snUtil.ApiBase + "Note/GetNotes",
        method: "POST",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        contentType: "application/json",
        data: JSON.stringify(req.users),
        success: function (data) {
            console.log("Building Notes HTML");
            var usersHTML = "";
            for (var key in data) {
                var udata = data[key];
                var unoterows = "";

                for (var i = 0; i < udata.length; i++) {
                    var note = udata[i];
                    unoterows += generateNoteRow(note);
                }
                usersHTML += generateNoteContainer(key, unoterows);
            }
            sendUserNotes({ "notes": usersHTML, "worker": req.worker });
        }
    });
}

function getNoteTypes() {
    console.log("Getting purdy things..");
    $.ajax({
        url: snUtil.ApiBase + "NoteType/Get",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            initNoteTypeData(d);
        },
        error: handleAjaxError
    });
}

function initNoteTypeData(data) {
    snUtil.NoteTypes = data;
    sendNoteTypeJSON(data);
    var cssString = '';
    for (var key in data) {
        var subCSSString = '/***start ' + key + '***/';
        var subData = data[key];
        for (var i = 0; i < subData.length; i++) {
            var noteType = subData[i];
            subCSSString += '#SNContainer .SN' + key + noteType.NoteTypeID + ' .SNMessage, #SNContainer .SNNoteType .SN' + key + noteType.NoteTypeID +
                '{' +
                'color: #' + noteType.ColorCode + ';';
            subCSSString += noteType.Bold ? 'font-weight: bold;' : '';
            subCSSString += noteType.Italic ? 'font-style: italic;' : '';
            subCSSString += '}';
        }
        subCSSString += '/***end ' + key + '***/';
        cssString += subCSSString
    }
    snUtil.NoteStyles.innerHTML = cssString;
    sendNoteTypeCSS(cssString);
}
function newNoteNewUser(req) {
    chrome.tabs.query({url:"*://*.reddit.com/*"}, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "newNoteNewUser", "req": req });
            chrome.tabs.sendMessage(tabs[i].id, { "method": "updateUsersWithNotes", "req": { "add": 1, "user": req.user } });
        }
    });
}
function newNoteExistingUser(req) {
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "newNoteExistingUser", "req": req });
        }
    });
}
function deleteNoteAndUser(req) {
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "deleteNoteAndUser", "req": req });
            chrome.tabs.sendMessage(tabs[i].id, { "method": "updateUsersWithNotes", "req": { "delete": 1, "user": req.user } });
        }
    });
}
function deleteNote(req) {
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "deleteNote", "req": req });
        }
    });
}
function workerInitialized(req) {
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "reinitWorker", "req": req });
        }
    });
}
function gotUsersWithNotes(users) {
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id,{ "method": "gotUsersWithNotes", "users": users });
        }
    });
}
function sendUserNotes(req) {
    chrome.tabs.sendMessage(req.worker, { "method": "receiveUserNotes", "notes": req.notes });
}
function sendNoteTypeCSS(css) {
    cssReady = true;
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "setNoteTypeCSS", "css": css });
        }
    });
}
function sendNoteTypeJSON(json) {
    cssReady = true;
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "setNoteTypeJSON", "json": json });
        }
    });
}
function getNoteTypeCSS(sendResponse, attempts) {
    attempts = attempts ? attempts + 1 : 1;
    if (attempts > 100) { return;} //this aint happening
    if (loggedIn) {
        if (cssReady) {
            sendResponse(snUtil.NoteStyles.innerHTML)
        }
        else {
            setTimeout(function () { getNoteTypeCSS(sendResponse, attempts) },250);
        }
    }
    else {
        setTimeout(function () { getNoteTypeCSS(sendResponse, attempts) }, 500);
    }
}
function getNoteTypeJSON(sendResponse, attempts) {
    attempts = attempts ? attempts + 1 : 1;
    if (attempts > 100) { return; } //this aint happening
    if (loggedIn) {
        if (cssReady) {
            sendResponse(snUtil.NoteTypes)
        }
        else {
            setTimeout(getNoteTypeJSON(sendResponse, attempts), 250);
        }
    }
    else {
        setTimeout(function () { getNoteTypeJSON(sendResponse, attempts) }, 500);
    }
}
chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
    switch (request.method) {
        case 'loggedIn':
            if (!loggedIn) {
                loggedIn = true;
                initWorker(); //socket opens here
            }
            break;
        case 'requestUserNotes':
            requestUserNotes({ "users": request.users, "worker": sender.tab.id });
            break;
        case 'closeSocket':
            closeSocket();
            break;
        case 'openSocket':
            initSocket();
            break;
        case 'getUsersWithNotes':
            sendResponse(loggedIn?usersWithNotes:undefined);
            break;
        case 'getNoteTypeCSS':
            getNoteTypeCSS(sendResponse, 0);
            break;
        case 'getNoteTypeJSON':
            getNoteTypeJSON(sendResponse, 0);
            break;
        case 'reinitAll':
            initWorker();
            break;
        default:
            break;
    }
});



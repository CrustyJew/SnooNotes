var waitingToClose;

var cssReady = false;

var settings = {subSettings:{}, moddedSubs:[], dirtbagSubs:[], isCabal:false, usersWithNotes:[], loggedIn:false, noteTypeCSS:''};
var errors = [];

var initialized = false;
var initializing = false;

var socketOpen = false;
function initSocket() {
    console.log("connecting socket");
    socketOpen = true;
    var snUpdate = $.connection.SnooNoteUpdates;
    snUpdate.client.receiveUpdate = function (note) { console.log(note.toString()); }
    snUpdate.client.addNewNote = function (note) {
        var user = note.AppliesToUsername.toLowerCase();
        var i = settings.usersWithNotes.indexOf(user);
        var newUser = false;
        if (i < 0) {
            newUser = true;
            settings.usersWithNotes.push(user);
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
                    var i = settings.usersWithNotes.indexOf(user);
                    if (i > -1) settings.usersWithNotes.splice(i, 1);

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
        getModdedSubs(true);
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
    //snUtil.RESTApiBase = "https://snoonotes.com/restapi/";
    snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
    snUtil.ApiBase = "https://localhost:44311/api/";
    snUtil.RESTApiBase = "https://localhost:44311/restapi/";
    snUtil.CabalSub = "spamcabal"; //lower case this bad boy

    initWorker();
    return;
}(snUtil = this.snUtil || {}));

function handleAjaxError(jqXHR, textStatus, errorThrown, message) {
    if (jqXHR.status === 401) {
        console.warn("401 received, reinitializing worker");
        initWorker();
    }
    else {
        //TODO send error through worker
        console.error("ERROR: " + message + " -- " + textStatus);
    }
}

function initWorker() {
    if (initializing) return; //run away if it's already initializing
    initializing = true;
    console.log("initWorker");
    $.ajax({
        url: snUtil.ApiBase + "Account/IsLoggedIn",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function () {
        settings.loggedIn = true;
        return $.when(getUsersWithNotes(), getModdedSubs(), getDirtbagSubs(), initSocket())
        .then(function () {
            initializing = false;
            console.log("pageWorker: initialized");
            initialized = true;
            workerInitialized({});
        });
    }).fail(function (e) {
        initializing = false;
        if (e.status === 401) {
            initialized = true;
            settings.loggedIn = false;
            workerInitialized({});
        }
        else {
            console.error(e.textStatus);
            setTimeout(function () { initWorker(); }, 2500);
        }
    })
    
    
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
      '<div id="SnooNote-' + user.toLowerCase() + '" class="SNViewContainer SNNoteArea" style="display:none;">' +
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
                (note.ParentSubreddit ? '<br>via<br><a href="https://reddit.com/r/' + note.ParentSubreddit + '">' + note.ParentSubreddit + '</a>' : settings.isCabal ? '<br><span class="SNCabalify"></span>' : '') +
                '<td class="SNSubmitter"><span>' + note.Submitter + '</span><br /><a href="' + note.Url + '">' + new Date(note.Timestamp).toLocaleString().replace(', ', '<br />') + '</a></td>' +
                '<td class="SNMessage"><p>' + note.Message + '</p>' + 
                (!note.ParentSubreddit || settings.moddedSubs.indexOf(note.ParentSubreddit.toLowerCase()) > -1 ? '<a class="SNDeleteNote">[x]</a></td></tr>' : '');
}

function getUsersWithNotes() {
    console.log("Getting users with notes");
    return $.ajax({
        url: snUtil.ApiBase + "Note/GetUsernamesWithNotes",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
    }).then(function (d, s, x) {
        settings.usersWithNotes = d.join("|").toLowerCase().split("|");
    }).fail(function (j,t,e) {
        handleAjaxError(j,t,e,"Getting usernames with notes failed");
    });
}

function getModdedSubs(broadcast) {
    console.log("Getting moderated subreddits and settings");
    return $.ajax({
        url: snUtil.RESTApiBase + "Subreddit",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },

    }).then(function (d) {
        settings.moddedSubs = d.map(function (sub) { return sub.SubName.toLowerCase() });
        settings.subSettings = {};
        settings.isCabal = false;
        var cssString = '';

        for (var i = 0; i < d.length; i++) {
            var name = d[i].SubName.toLowerCase();
            //this stores the NoteTypes as well so it's a bit redundant, but I'm leaving it in for now.
            settings.subSettings[name] = d[i].Settings;

            if (name == snUtil.CabalSub) settings.isCabal = true;

            var subCSSString = '/***start ' + name + '***/';
            for (var x = 0; x < d[i].Settings.NoteTypes.length; x++) {
                var noteType = d[i].Settings.NoteTypes[x];
                subCSSString += '#SNContainer .SN' + name + noteType.NoteTypeID + ' .SNMessage, #SNContainer .SNNoteType .SN' + name + noteType.NoteTypeID +
                    '{' +
                    'color: #' + noteType.ColorCode + ';';
                subCSSString += noteType.Bold ? 'font-weight: bold;' : '';
                subCSSString += noteType.Italic ? 'font-style: italic;' : '';
                subCSSString += '}';
            }
            subCSSString += '/***end ' + name + '***/';
            cssString += subCSSString
        }
        settings.noteTypeCSS = cssString;
        if (broadcast) {
            workerInitialized({});
        }
    }).fail(function (j, t, e) {
        handleAjaxError(j, t, e, "Getting moderated subreddits and settings failed");
    });
}

function getDirtbagSubs() {
    return $.ajax({
        url: snUtil.RESTApiBase + "Subreddit/admin",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        error: handleAjaxError
    }).then(function (d, s, x) {

        settings.dirtbagSubs = [];
        for (var i = 0; i < d.length; i++) {
            var botSettings = d[i].BotSettings;
            //check if there is a URL for DirtBag 
            if (botSettings && botSettings.DirtbagUrl) settings.dirtbagSubs.push(d[i].SubName.toLowerCase());
        }
    }).fail(function (j, t, e) {
        handleAjaxError(j, t, e, "Getting admin / dirtbag subreddits failed");
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
        data: JSON.stringify(req.users)
    }).then(function (data) {
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
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        handleAjaxError(jqXHR, textStatus, errorThrown, "Requesting usernotes failed");
    });
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
    console.log("Broadcast reinitWorker");
    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "reinitWorker", "req": req });
        }
    });
}
//function gotUsersWithNotes(users) {
//    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
//        for (var i = 0; i < tabs.length; i++) {
//            chrome.tabs.sendMessage(tabs[i].id,{ "method": "gotUsersWithNotes", "users": users });
//        }
//    });
//}
function sendUserNotes(req) {
    chrome.tabs.sendMessage(req.worker, { "method": "receiveUserNotes", "notes": req.notes });
}

//function sendNoteTypeJSON(json) {
//    cssReady = true;
//    chrome.tabs.query({ url: "*://*.reddit.com/*" }, function (tabs) {
//        for (var i = 0; i < tabs.length; i++) {
//            chrome.tabs.sendMessage(tabs[i].id, { "method": "setNoteTypeJSON", "json": json });
//        }
//    });
//}

chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
    switch (request.method) {
        case 'loggedIn':
            if (!settings.loggedIn) {
                settings.loggedIn = true;
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
        case 'reinitAll':
            initWorker();
            break;
        case 'getSettings':
            sendResponse(initialized ? settings : undefined);
        default:
            break;
    }
});



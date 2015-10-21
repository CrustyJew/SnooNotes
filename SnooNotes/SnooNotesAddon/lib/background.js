var waitingToClose;
var loggedIn = false;
var usersWithNotes = [];
var cssReady = false;
(function () {
    initBrowser();
});


function newNoteNewUser(req) {
    var x = usersWithNotes.indexOf(req.user);
    if (x == -1) {
        usersWithNotes.push(req.user);
    }
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
    var x = usersWithNotes.indexOf(req.user);
    if (x > -1) {
        usersWithNotes.splice(x, 1);
    }
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
    usersWithNotes = users;
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




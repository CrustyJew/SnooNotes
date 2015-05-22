var waitingToClose;
var loggedIn = false;
var usersWithNotes = [];

(function () {
    initBrowser();
});


function newNoteNewUser(req) {
    var x = usersWithNotes.indexOf(req.user);
    if (x == -1) {
        usersWithNotes.push(req.user);
    }
    chrome.tabs.query({ active: true }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "newNoteNewUser", "req": req });
            chrome.tabs.sendMessage(tabs[i].id, { "method": "updateUsersWithNotes", "req": { "add": 1, "user": req.user } });
        }
    });
}
function newNoteExistingUser(req) {
    chrome.tabs.query({ active: true }, function (tabs) {
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
    chrome.tabs.query({ active: true }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "deleteNoteAndUser", "req": req });
            chrome.tabs.sendMessage(tabs[i].id, { "method": "updateUsersWithNotes", "req": { "delete": 1, "user": req.user } });
        }
    });
}
function deleteNote(req) {
    chrome.tabs.query({ active: true }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "deleteNote", "req": req });
        }
    });
}
function workerInitialized(req) {
    chrome.tabs.query({ active: true }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "reinitWorker", "req": req });
        }
    });
}
function gotUsersWithNotes(users) {
    usersWithNotes = users;
    chrome.tabs.query({ active: true }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id,{ "method": "gotUsersWithNotes", "users": users });
        }
    });
}
function sendUserNotes(req) {
    chrome.tabs.query({ active: true, index: req.worker.tabid, windowid: req.worker.windowid }, function (tabs) {
        for (var i = 0; i < tabs.length; i++) {
            chrome.tabs.sendMessage(tabs[i].id, { "method": "receiveUserNotes", "notes": req.notes });
        }
    });
}

chrome.runtime.onMessage.addListener(function (request, sender, sendRespones) {
    switch (request.method) {
        case 'loggedIn':
            if (!loggedIn) {
                loggedIn = true;
                initWorker(); //socket opens here
            }
            break;
        case 'requestUserNotes':
            requestUserNotes({ "users": request.users, "worker": { "tabid": sender.tab.id, "windowid": sender.tab.windowid } });
            break;
        case 'closeSocket':
            closeSocket();
            break;
        case 'openSocket':
            initSocket();
            break;
        default:
            break;
    }
});




var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
var array = require('sdk/util/array');
var timers = require("sdk/timers");
var tabs = require("sdk/tabs");

var activeWorkers = [];
var waitingToClose;
var socketOpen = false;
var loggedIn = false;
var usersWithNotes = [];
pageMod.PageMod({
    include: "*.reddit.com",
    exclude: [/.*.reddit.com\/api\/v1\/authorize.*/, /.*.reddit.com\/login.*/],
    contentStyleFile: [data.url("styles/SnooLogin.css"),
        data.url("styles/SNContainer.css")],
    contentScriptFile: [data.url("libs/jquery-2.1.3.min.js"),
        data.url("modules/SNFirefox.js"),
        data.url("modules/SNLoad.js"),
        data.url("modules/SnooNotes.js"),
        data.url("modules/SnooLoginPopup.js"),
        data.url("modules/SNMain.js")],
    attachTo: ["existing", "frame", "top"],
    onAttach: function (worker) {
        console.log(worker.tab.url);
        if (waitingToClose) {
            timers.clearTimeout(waitingToClose);
            waitingToClose = null;
        }

        array.add(activeWorkers, worker);

        /*if (!socketOpen && loggedIn) {
            openSocket();
        }*/
        worker.on('detach', function (worker) {
            //var index = activeWorkers.indexOf(worker);
            //if (index != -1) {
            //    activeWorkers.splice(index, 1);
            //}
            array.remove(activeWorkers, this)
            checkStillModding();
        });
        worker.on('pageshow', function () { array.add(activeWorkers, this); });
        worker.on('pagehide', function () { array.remove(activeWorkers, this); });
        worker.port.on('loggedIn', function () {
            if (!loggedIn) {
                loggedIn = true;
                pageWorker.port.emit("initWorker"); //socket opens here
            }
        });
        if (loggedIn) { worker.port.emit("gotUsersWithNotes", usersWithNotes); }
        worker.port.on("requestUserNotes", function (users) {
            pageWorker.port.emit("requestUserNotes", { "users": users, "worker": activeWorkers.indexOf(worker) });
        });
        console.log(activeWorkers.length);
    }
});


var pageWorker = require("sdk/page-worker").Page({
    contentScriptFile: [data.url("libs/jquery-2.1.3.min.js"),
        data.url("libs/jquery.signalR-2.2.0.min.js"),
        data.url("libs/snUpdatesHub.js"),
        data.url("modules/SNWorkerFirefox.js"),
        data.url("modules/SNWorker.js")],
    contentURL: data.url("WorkerPage.html")
});

pageWorker.port.on("gotUsersWithNotes", function (users) {
    usersWithNotes = users;
    for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("gotUsersWithNotes", usersWithNotes);
    }
});
pageWorker.port.on("workerInitialized", function () {
    /*for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("reinitWorker");
    }*/
});
pageWorker.port.on("sendUserNotes", function (req) {
    activeWorkers[req.worker].port.emit("receiveUserNotes", req.notes);
});
pageWorker.port.on("newNoteExistingUser", function (req) {
    for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("newNoteExistingUser", req);
    }
});
pageWorker.port.on("newNoteNewUser", function (req) {
    var x = usersWithNotes.indexOf(req.user);
    if (x == -1)
    {
        usersWithNotes.push(req.user);
    }
    for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("newNoteNewUser", req);
        activeWorkers[i].port.emit("updateUsersWithNotes", { "add": 1, "user": req.user });
    }
});
pageWorker.port.on("deleteNoteAndUser", function (req) {
    var x = usersWithNotes.indexOf(req.user);
    if (x > -1) {
        usersWithNotes.splice(x,1);
    }
    for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("deleteNoteAndUser", req);
        activeWorkers[i].port.emit("updateUsersWithNotes", { "delete": 1, "user": req.user });
    }
});
pageWorker.port.on("deleteNote", function (req) {
    for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("deleteNote", req);
    }
});
function checkStillModding() {
    console.log("checking if Reddit is still open");
    if (activeWorkers.length <= 0) {
        waitingToClose = timers.setTimeout(closeSocket, 10000);
    }
}
function closeSocket() {
    if (activeWorkers.length <= 0) {
        console.log("closing socket");
        socketOpen = false;
        loggedIn = false;
        pageWorker.port.emit("closeSocket");
    }
}
function openSocket() {
    console.log("opening socket");
    socketOpen = true;
    pageWorker.port.emit("openSocket");
}


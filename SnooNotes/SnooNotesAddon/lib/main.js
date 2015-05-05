var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");

var timers = require("sdk/timers");
var tabs = require("sdk/tabs");

var activeWorkers = [];
var waitingToClose;
var socketOpen = false;
var loggedIn = false;
var usersWithNotes = [];
pageMod.PageMod({
    include: "*.reddit.com",
    exclude: [/.*.reddit.com\/api\/v1\/authorize.*/,/.*.reddit.com\/login.*/],
    contentStyleFile: [data.url("styles/SnooLogin.css")],
    contentScriptFile: [data.url("libs/jquery-2.1.3.min.js"),
        data.url("libs/jstorage.min.js"),
        
         data.url("modules/SNLoad.js"),
         data.url("modules/SnooLoginPopup.js"),
         data.url("modules/SNMain.js"),
            
         ],
    attachTo: ["existing","frame", "top"],
    onAttach: function (worker) {
        console.log(worker.tab.url);
        if (waitingToClose) {
            timers.clearTimeout(waitingToClose);
            waitingToClose = null;
        }
        activeWorkers.push(worker);
        if (!socketOpen && loggedIn) {
            openSocket();
        }
        worker.on('detach', function () {
            var index = activeWorkers.indexOf(worker);
            if (index != -1) {
                activeWorkers.splice(index, 1);
            }
            checkStillModding();
        });
        worker.port.on('loggedIn', function () {
            if (!loggedIn) {
                loggedIn = true;
                pageWorker.port.emit("initWorker");
            }
        });
        worker.port.emit("gotUsersWithNotes", usersWithNotes);
        worker.port.on("requestUserNotes", function (users) {
            pageWorker.port.emit("requestUserNotes", users, worker);
        });
        console.log(activeWorkers.length);
    }
});


var pageWorker = require("sdk/page-worker").Page({
    contentScriptFile: [data.url("libs/jquery-2.1.3.min.js"),
        data.url("libs/jquery.signalR-2.2.0.min.js"),
        data.url("libs/snUpdatesHub.js"),
        data.url("modules/SNWorker.js")],
    contentURL: data.url("WorkerPage.html")
});

pageWorker.port.on("gotUsersWithNotes", function (users) {
    usersWithNotes = users;
    for (var i = 0; i < activeWorkers.length; i++) {
        activeWorkers[i].port.emit("gotUsersWithNotes", usersWithNotes);
    }
});

pageWorker.port.on("sendingUserNotes", function (notes, worker) {
    worker.port.emit("receiveUserNotes", notes);
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
    }
}
function openSocket() {
    console.log("opening socket");
    socketOpen = true;
}


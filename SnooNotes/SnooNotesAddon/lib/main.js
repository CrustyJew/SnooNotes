var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
var pageWorkers = require("sdk/page-worker");
var timers = require("sdk/timers");
var tabs = require("sdk/tabs");

var workerCount = 0;
var waitingToClose;
var socketOpen = false;
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
        workerCount = workerCount + 1;
        if (!socketOpen) {
            openSocket();
        }
        worker.on('detach', function () {
            workerCount = workerCount - 1;
            checkStillModding();
        });
        console.log(workerCount);
    }
});

function checkStillModding() {
    console.log("checking if Reddit is still open");
    if (workerCount <= 0) {
        waitingToClose = timers.setTimeout(closeSocket, 10000);
    }
}
function closeSocket() {
    if (workerCount <= 0) {
        console.log("closing socket");
        socketOpen = false;
    }
}
function openSocket() {
    console.log("opening socket");
    socketOpen = true;
}


var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
pageMod.PageMod({
    include: "*.reddit.com",
    exclude: [/.*.reddit.com\/api\/v1\/authorize.*/,/.*.reddit.com\/login.*/],
    contentStyleFile: [data.url("styles/SnooLogin.css")],
    contentScriptFile: [data.url("libs/jquery-2.1.3.min.js"),
            data.url("modules/SNMain.js"),
            data.url("modules/SnooLoginPopup.js")],
    attachTo: ["existing","frame", "top"],
    onAttach: function (worker) {
        console.log(worker.tab.url);
        //var event = new CustomEvent("snReadyToInit");
        //window.dispatchEvent(event);
    }
});
var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
pageMod.PageMod({
    include: "*",
    contentScriptFile: [data.url("jquery-2.1.3.min.js"),
            data.url("SNMain.js"),
            data.url("SnooLoginPopup.js")],
    attachTo: ["existing","frame", "top"],
    onAttach: function (worker) {
        console.log(worker.tab.url);
        //var event = new CustomEvent("snReadyToInit");
        //window.dispatchEvent(event);
    }
});
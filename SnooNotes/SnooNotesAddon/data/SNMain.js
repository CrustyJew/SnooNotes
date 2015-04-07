function initSnooNotes() {
    (function (snUtil) {
        snUtil.ApiBase = "https://snoonotesapi.azurewebsites.net/api/";
        snUtil.LoginAddress = "https://snoonotesapi.azurewebsites.net/Auth/Login";
        if (snUtil.moddedSubs === undefined) setModdedSubs();
        return;
    }(snUtil = window.snUtil || {}));
}

function setModdedSubs(){
    $.ajax({
        url: snUtil.ApiBase + "Account/GetModeratedSubreddits",
        method: "GET",
        success: function (d, s, x) {
            snUtil.moddedSubs = d;
        },
        error: handleAjaxError
    });
    return;
}

function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if(jqXHR.statusText=="error" && jqXHR.status === 0)
    {
        showLoginPopup();
    }
}

(function () {
    //window.addEventListener("snReadyToInit", function () {
    initSnooNotes();
    var event = new CustomEvent("snUtilDone");
    window.dispatchEvent(event);
    //});
})();
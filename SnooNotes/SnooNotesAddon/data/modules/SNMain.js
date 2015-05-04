function initSnooNotes() {
    (function (snUtil) {
        //snUtil.ApiBase = "https://snoonotesapi.azurewebsites.net/api/";
        snUtil.ApiBase = "https://localhost:44311/api/";
        //snUtil.LoginAddress = "https://snoonotesapi.azurewebsites.net/Auth/Login";
        snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
        if (snUtil.ModdedSubs === undefined) setModdedSubs();
        var sub = /reddit\.com\/r\/[a-z0-9]*\/?/i.exec(window.location);
        snUtil.Subreddit = !sub ? "" : sub[0].substring(13).replace(/\//g, '');
        snUtil.getNotesForUsers = function (sub, users) {
            $.ajax({
                url: snUtil.ApiBase + "Note/GetNotes",
                method: "POST",
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                data: { 'subname': sub, 'Users': users },
                success: function (d, s, x) {
                    var x = d;
                },
                error: handleAjaxError
            });
        };
        return;
    }(snUtil = window.snUtil || {}));
}

function setModdedSubs(){
    $.ajax({
        url: snUtil.ApiBase + "Account/GetModeratedSubreddits",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.ModdedSubs = d;
        },
        error: handleAjaxError
    });
    return;
}

function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if(jqXHR.status === 401)
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
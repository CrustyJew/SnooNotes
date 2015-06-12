function initSnooNotes() {
    (function (snUtil) {
        

        //snUtil.ApiBase = "https://snoonotes.com/api/";
        //snUtil.LoginAddress = "https://snoonotes.com/Auth/Login";
        snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
        snUtil.ApiBase = "https://localhost:44311/api/";
        
        if ($('#SNContainer').length == 0) {
            $('body').append($('<div id="SNContainer"></div>'));
        }
        snUtil.NoteStyles = document.createElement('style');
        $('#SNContainer').append(snUtil.NoteStyles);
        snUtil.reinitWorker = function () {
            var event = new CustomEvent("snUtilDone");
            window.dispatchEvent(event);
        }
        snUtil.setUsersWithNotes = function(users){
            if (!users) {
                return;
            }
            snUtil.UsersWithNotes = "," + users.join(",") + ","; //I hate stupid arrays and not being able to case-insensitive searches!

        };
       
        //received data from socket to add/remove a user
        snUtil.updateUsersWithNotes = function (req) {
            var user = req.user;
            if (req.remove) {
                console.log("removed user");
                snUtil.UsersWithNotes = snUtil.UsersWithNotes.replace("," + user + ",", ","); //E-i-E-i-ooooooo
            }
            else if (req.add) {
                console.log("Added user");
                snUtil.UsersWithNotes = snUtil.UsersWithNotes + "," + user + ",";
            }
        };
        
        snUtil.getNotesForUsers = function (users) {
            snBrowser.requstUserNotes(users);
        };
        //have to have the snUtil functions ready >.<
        browserInit(); //init browser first to attach listeners etc
        //do this lateish so we get all the listeners hooked up first
        if (!snUtil.LoggedIn) checkLoggedIn();
        var sub = /reddit\.com\/r\/[a-z0-9\+]*\/?/i.exec(window.location);
        snUtil.Subreddit = !sub ? "" : sub[0].substring(13).replace(/\//g, '');
        snUtil.Subreddit = snUtil.Subreddit.indexOf('+') != -1  ? "" : snUtil.Subreddit; //if it contains a plus sign, it's a multi reddit, not a mod
        
        snUtil.Modmail = window.location.pathname.match(/\/message\/(?:moderator)\/?/);

        return;
    }(snUtil = window.snUtil || {}));
}

function setModdedSubs(){
    $.ajax({
        url: snUtil.ApiBase + "Account/GetModeratedSubreddits",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.ModdedSubs = "," + d.join(",") + ",";
            var event = new CustomEvent("snUtilDone");
            window.dispatchEvent(event);
        },
        error: handleAjaxError
    });
    return;
}
function checkLoggedIn() {
    $.ajax({
        url: snUtil.ApiBase + "Account/IsLoggedIn",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            snUtil.LoggedIn = true;
            snBrowser.loggedIn();
            if (!snUtil.ModdedSubs) setModdedSubs();
        },
        error: handleAjaxError
    });
}
function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if(jqXHR.status === 401)
    {
        window.dispatchEvent(new CustomEvent("snLoggedOut"));
        //showLoginPopup();
    }
}

(function () {
   
    jQuery.expr[":"].Contains = jQuery.expr.createPseudo(function (arg) {
        return function (elem) {
            return jQuery(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
        };
    });
    initSnooNotes();
    $('div.content').on('DOMNodeInserted', function (e) {
        //copied from mod toolbox in the hopes it will make it compatible with their stuff too.
        var $target = $(e.target), $parentNode = $(e.target.parentNode);
        if (!($target.hasClass("sitetable") && ($target.hasClass("listing") || $target.hasClass("linklisting") ||
            $target.hasClass("modactionlisting"))) && !$parentNode.hasClass('morecomments') && !$target.hasClass('flowwit')) return;

        console.log('snGotNewThings firing from: ' + $target.attr('class'));

        setTimeout(function () {
            var event = new CustomEvent("snGotNewThings");
            window.dispatchEvent(event);
        }, 1750);
    });
})();
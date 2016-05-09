
$('body').on('click', '.remove-button .main > a, .big-mod-buttons > span > .pretty-button:not(".positive")', function () {
    var $btn = $(this);
    var $thing = $btn.closest('.thing');
    var sub = $thing.attr('data-subreddit');
    var dbSub = new RegExp("," + sub + ",", "i").test(snUtil.DirtbagSubs);
    if (dbSub && $('.dbAutoModBan', $thing).length == 0) {
        var user = $thing.attr('data-author');
        var domain = $thing.attr('data-domain');
        var reason = $btn.attr('data-event-action');
        var thingid = $thing.attr('data-fullname');
        var url = "";
        if (domain && (domain.toLowerCase() == 'youtube.com' || domain.toLowerCase() == 'youtu.be')) {
            url = $thing.attr('data-url');
        }
        if ($btn.hasClass('pretty-button')) {
            $btn.after(getDBAutoModElement(sub, user, url, reason, thingid));
        } else {
            $btn.closest('li').after(getDBAutoModElement(sub, user, url, reason, thingid));
        }

    }
});

$('body').on('click', '.dbAutoModBan > a', function () {
    var $btn = $(this);
    var sub = $btn.attr('db-am-sub');
    var user = $btn.attr('db-am-user');
    var channel = $btn.attr('db-am-channel');
    var reason = $btn.attr('db-am-reason');
    var thing = $btn.attr('db-am-thingid');
    $btn.text("banning");
    if (channel) {
        $.ajax({
            url: snUtil.ApiBase + "Dirtbag/" + sub + "/BanList/Channels",
            method: "POST",
            data: {EntityString: channel, BanReason: reason, ThingID: thing } ,
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
        }).then(
        function (d) {
            $btn.replaceWith(function () {
                return $("<span>Banned!</span>");
            });
        }, function (e) {
            $btn.text("Error!");
        });
    }
    else if (user) {
        $.ajax({
            url: snUtil.ApiBase + "Dirtbag/" + sub + "/BanList/Users",
            method: "POST",
            data: { EntityString: user, BanReason: reason, ThingID: thing } ,
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
        }).then(
        function (d) {
            $btn.replaceWith(function () {
                return $("<span>Banned!</span>");
            });
        }, function (e) {
            $btn.text("Error!");
        });
    }
});


function getDBAutoModElement(sub, user, url, reason, thingID) {
    var banElem =
    "<span class='dbAutoModBan'>Automod Ban: " +
        "<a db-am-sub='" + sub + "' db-am-user='" + user + "' db-am-reason='" + reason + "' db-am-thingid='" + thingID + "'>User</a>" +
        (url ? "&nbsp;|&nbsp;<a db-am-sub='" + sub + "' db-am-channel='" + encodeURIComponent(url) + "' db-am-reason='" + reason + "' db-am-thingid='"+thingID+"'>Channel</a>" : "") +
    "</span>";

    return banElem;
}
$('body').on('click', '.big-mod-buttons > span > .pretty-button', function () {
    var $btn = $(this);
    var $thing = $btn.closest('.thing');
    var sub = $thing.attr('data-subreddit');
    var thingid = $thing.attr('data-fullname');
    var reason = $btn.attr('data-event-action');
    if (!reason) {
        reason = $btn.closest('form').find('.option.main a').attr('data-event-action')
    }
    actionClick(sub, thingid, reason);
});
$(function () {
    $('.remove-button .option .yes').each(function () {
        var $btn = $(this);
        var $thing = $btn.closest('.thing');
        var sub = $thing.attr('data-subreddit');
        var thingid = $thing.attr('data-fullname');
        var reason = $btn.attr('data-event-action');
        if (!reason) {
            reason = $btn.closest('form').find('.option.main a').attr('data-event-action')
        }
        $btn.attr('data-subreddit', sub);
        $btn.attr('data-fullname', thingid);
        $btn.attr('data-event-action', reason);
    });
    $('.remove-button .option').children().on('click', function () {
        if ($(this).hasClass('yes')) {
            var $btn = $(this);
            var sub = $btn.attr('data-subreddit');
            var thingid = $btn.attr('data-fullname');
            var reason = $btn.attr('data-event-action');
            actionClick(sub,thingid,reason);
        }
    });
})
function actionClick(sub, thingid, reason) {
    if (snUtil.settings.moddedSubs.indexOf(sub) > -1) {
        $.ajax({
            url: snUtil.ApiBase + 'ModAction/' + sub,
            method: 'POST',
            data: { ThingID: thingid, Action: reason },
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
    }
}
function receiveModAction(req) {
    var $thing = $('.id-' + req.thingID);
    if (req.action == "remove") {
        $thing.toggleClass('spam', true);
        $thing.toggleClass('removed', true);
        $thing.toggleClass('spammed', false);
        $thing.toggleClass('approved', false);
    }
    else if (req.action == 'spam') {
        $thing.toggleClass('spam', true);
        $thing.toggleClass('removed', false);
        $thing.toggleClass('spammed', true);
        $thing.toggleClass('approved', false);
    }
    else if (req.action == 'approve') {
        $thing.toggleClass('spam', false);
        $thing.toggleClass('removed', false);
        $thing.toggleClass('spammed', false);
        $thing.toggleClass('approved', true);
    }
    else {
        $thing.toggleClass('spam', false);
        $thing.toggleClass('removed', false);
        $thing.toggleClass('spammed', false);
        $thing.toggleClass('approved', false);
        $thing.toggleClass('SNModActionWarning', true);
    }
    var $entry = $('.id-' + req.thingID + '>.entry');
    $('.SNModAction',$entry).remove();
    var $bigButtons = $('ul.flat-list .big-mod-buttons',$entry);
    if ($bigButtons.length > 0) {
        $bigButtons.after('<span class="SNModAction">' + req.action + ' by ' + req.mod + '</span>');
    }
    else {
        $('li:last',$entry).after('<span class="SNModAction">' + req.action + ' by ' + req.mod + '</span>');
    }
}
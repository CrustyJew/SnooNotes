(function () {
    window.addEventListener("snUtilDone", function (e) {
        $('.sitetable').on('click', '.SNViewNotes', function (e) {
            showNotes(e);
        });
        $('#SNContainer').on('click', '.SNCloseNote', function (e) {
            closeNote(e);
        });
        $('#SNContainer').on('click', '.SNNewNoteSubmit', function (e) {
            var ot = e.originalEvent.originalTarget;
            submitNote(ot.attributes["SNUser"].value,ot.attributes["SNSub"].value,ot.attributes["SNLink"].value,$(ot).siblings('.SNNewMessage').val(),3);
        });
        e.target.removeEventListener(e.type, arguments.callee);
    });
    self.port.on("newNoteExistingUser", function (req) {
        var $user = $('#SnooNote-' + req.user + ' table');
        if ($user.length > 0) {
            var $note = $(req.note);
            if ($user.is(":visible")) {
                $user.append($note.hide());
                $note.fadeIn("slow");
            }
            else {
                $user.append($note);
            }
        }
    });
})();

function showNotes (e){
    var $sn = $('#SnooNote-' + e.originalEvent.originalTarget.attributes["SNUser"].value);
    $sn.css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
    var $submit = $('.SNNewNoteSubmit', $sn);
    var $ot = $(e.originalEvent.originalTarget);
    $submit.attr("SNSub", window.snUtil.Subreddit ? window.snUtil.Subreddit : $ot.siblings("a.subreddit:first").innerHTML);
    $submit.attr("SNLink", $('ul li.first a', $ot.closest('div.entry')).attr('href'));
}

function closeNote(e) {
    $(e.originalEvent.originalTarget).closest('.SNViewContainer').hide();
}

function submitNote(user, sub, link, message, type) {
    $.ajax({
        url: window.snUtil.ApiBase + "note",
        method: "POST",
        datatype: "application/json",
        data: { "NoteTypeID": type, "SubName": sub, "Message": message, "AppliesToUsername": user, "Url": link },
        success: function (d, status, jqXHR) {
            $('#SnooNote-' + user.toLowerCase() + ' .SNNewMessage').val('');
        }
    });
}
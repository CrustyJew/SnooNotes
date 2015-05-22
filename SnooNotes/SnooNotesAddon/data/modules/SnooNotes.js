(function () {
    window.addEventListener("snUtilDone", function (e) {
        $('#siteTable,.commentarea').on('click', '.SNViewNotes', function (e) {
            showNotes(e);
        });
        $('#SNContainer').on('click', '.SNCloseNote', function (e) {
            closeNote(e);
        });
        $('#SNContainer').on('click', '.SNCloseNewNote', function (e) {
            var $newnote = $(e.originalEvent.originalTarget).closest('.SNNew');
            $newnote.hide();
        });
        $('#SNContainer').on('click', '.SNNewNoteSubmit', function (e) {
            var ot = e.originalEvent.originalTarget;
            submitNote(ot.attributes["SNUser"].value, ot.attributes["SNSub"].value, ot.attributes["SNLink"].value, $(ot).siblings('.SNNewMessage').val(), 3);
        });
        $('#SNContainer').on('click', '.SNDeleteNote', function (e) {
            var ot = e.originalEvent.originalTarget;
            var id = $(ot).closest('tr').attr("id").replace("SN", "");
            $.ajax({
                url: window.snUtil.ApiBase + "note/Delete?id=" + id,
                method: "DELETE",
                //datatype: "application/json",
                //data:{"id":id}
            });
        });
        $('#siteTable,.commentarea').on('click', '.SNNoNotes', function (e) {
            var $ot = {};

            if (e.originalEvent) $ot = $(e.originalEvent.originalTarget);
            else $ot = $(e.target);

            var user = $ot.siblings('a.author:first')[0].innerHTML.toLowerCase();
            var $newNote = $('#SnooNote-' + user);
            if ($newNote.length == 0) { //add a new note container if it doesn't exist
                $newNote = $('<div id="SnooNote-' + user + '" class="SNNew" style="display:none;">' +
                    '<div class="SNHeader"><a class="SNCloseNewNote">Cancel [x]</a></div>' +
                    '<div class="SNNewNote">' +
                    '<textarea placeholder="Add a new note for user..." class="SNNewMessage" />' +
                    '<button type="button" class="SNNewNoteSubmit" ' +
                        'SNUser="' + user + '" ' +
                        'SNSub="' + window.snUtil.Subreddit + '" ' + //only needs snUtil.Subreddit because if it's a comment then it has to be on a subreddit page
                        'SNLink="' + $('ul li.first a', $ot.closest('div.entry')).attr('href') + '" ' +
                    '>Submit</button>  ' +
                    '</div></div>');

                $('#SNContainer').append($newNote);
            }

            $newNote.css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
        });
        e.target.removeEventListener(e.type, arguments.callee);
    });
})();

function newNoteExistingUser(req) {
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
}

function newNoteNewUser(req) {
    var $user = $('#SnooNote-' + req.user);
    var $entries = $("#siteTable .entry .author:Contains(" + req.user + "), .commentarea .entry .author:Contains(" + req.user + ")").closest("div.entry");
    if ($entries.length > 0) {
        $('.SNNoNotes', $entries).remove();
        $('.author', $entries).after($('<a SNUser="' + req.user + '" class="SNViewNotes">[view note]</a>'));
    }
    if ($user.length == 0) {
        //new note for a user not added by this page
        if ($entries.length > 0) {
            $('#SNContainer').append($(req.note));
        }
    }
    else {
        //hey! I just added that one!
        var $notecont = $(req.note);
        $user.removeClass("SNNew").addClass("SNViewContainer");
        var $submit = $('.SNNewNoteSubmit', $user);
        $('.SNNewNoteSubmit', $notecont).replaceWith($submit);
        $user.empty();
        $user.append($notecont.children().hide().fadeIn("fast"));
    }
}
function deleteNoteAndUser(req) {
    var $user = $('#SnooNote-' + req.user);
    var $entries = $("#siteTable .entry .author:Contains(" + req.user + "), .commentarea .entry .author:Contains(" + req.user + ")").closest("div.entry");
    if ($entries.length > 0) {
        $('.SNViewNotes', $entries).remove();
        $('.author', $entries).after($('<a SNUser="' + req.user + '" class="SNNoNotes">[add note]</a>'));
    }
    if ($user.length > 0) {
        if ($user.is(":visible")) {
            var link = $('.SNNewNoteSubmit', $user).attr('SNLink');
            link = /\/r\/.*/.exec(link)[0]; //trim out some of the prefix garbage that might cause issues if browsing with https etc.
            var $entry = $('#siteTable .entry a[href$="' + link + '"], .commentarea .entry a[href$="' + link + '"]').closest('div.entry');

            $user.remove();
            //$('.SNNoNotes', $entry).trigger('click'); this doesn't work quite right so axing it for now.
        }
        else {
            $user.remove();
        }
    }
}
function deleteNote(req) {

    $note = $('#SN' + req.noteID);
    if ($note.is(":visible")) {
        $note.hide("slow", function () { $note.remove(); });
    }
    else {
        $note.remove();
    }
}


function showNotes(e) {
    var $sn = $('#SnooNote-' + e.originalEvent.originalTarget.attributes["SNUser"].value);
    $sn.css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
    var $submit = $('.SNNewNoteSubmit', $sn);
    var $ot = $(e.originalEvent.originalTarget);
    $submit.attr("SNSub", window.snUtil.Subreddit ? window.snUtil.Subreddit : $ot.siblings("a.subreddit:first")[0].innerHTML);
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
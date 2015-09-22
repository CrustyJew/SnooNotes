(function () {
    window.addEventListener("snUtilDone", function (e) {
        $('#siteTable,.commentarea').on('click', '.SNViewNotes', function (e) {
            showNotes(e);
        });
        $('#SNContainer').on('click', '.SNCloseNote', function (e) {
            closeNote(e);
        });
        $('#SNContainer').on('click', '.SNCloseNewNote', function (e) {
            var $newnote = $(e.target).closest('.SNNew');
            $newnote.hide();
        });
        $('#SNContainer').on('click', '.SNNewNoteSubmit', function (e) {
            var ot = e.target;
            var $newNoteContainer = $(ot).closest('.SNNewNoteContainer');
            var $message = $(ot).siblings('.SNNewMessage');
            var notetype = $newNoteContainer.find('input:radio[name=SNType]:checked').val();
            var valid = true;
            var $err = $newNoteContainer.find('.SNNewError');
            var $ntContainer = $newNoteContainer.find('.SNNoteType');
            //clean up previous errors if there were any
            $message.removeClass("SNError");
            $ntContainer.removeClass("SNError");
            $err.empty();
            if (!$message.val()) {
                valid = false;
                $err.append($("<p>Looks like you forgot the note text there...</p>"));
                $message.addClass("SNError");
            }
            if (!notetype) {
                valid = false;
                $err.append($("<p>Shucks! You forgot the note type...</p>"));
                $ntContainer.addClass("SNError");
            }
            if (valid) {
                submitNote(ot.attributes["SNUser"].value, ot.attributes["SNSub"].value, ot.attributes["SNLink"].value, $message.val(), notetype, $newNoteContainer);
            }
        });
        $('#SNContainer').on('click', '.SNDeleteNote', function (e) {
            var ot = e.target;
            var id = $(ot).closest('tr').attr("id").replace("SN", "");
            $.ajax({
                url: window.snUtil.ApiBase + "note/Delete?id=" + id,
                method: "DELETE",
                //datatype: "application/json",
                //data:{"id":id}
            });
        });
        $('#siteTable,.commentarea').on('click', '.SNNoNotes', function (e) {
            var $ot = $(e.target);

            var user = $ot.siblings('a.author:first')[0].innerHTML.toLowerCase();
            var $newNote = $('#SnooNote-' + user);
            var sub = getSubName(e);
            if ($newNote.length == 0) { //add a new note container if it doesn't exist
                $newNote = $('' +
                    '<div id="SnooNote-' + user + '" class="SNNew" style="display:none;">' +
                        '<div class="SNHeader"><a class="SNCloseNewNote SNClose">Cancel [x]</a></div>' +
                        '<div class="SNNewNoteContainer">' +
                            '<div class="SNNewNote">' +
                                '<textarea placeholder="Add a new note for user..." class="SNNewMessage" />' +
                                '<button type="button" class="SNNewNoteSubmit" ' +
                                    'SNUser="' + user + '" ' +
                                    'SNSub="' + sub  + '" ' + 
                                    'SNLink="' + $('ul li.first a', $ot.closest('div.entry')).attr('href') + '" ' +
                                '>Submit</button>  ' +
                            '</div>' +
                            '<div class="SNNoteType"></div>' +
                            '<div class="SNNewError"></div>' +
                        '</div>' +
                    '</div>');
                var subNoteTypes = snUtil.NoteTypes[sub];
                var $SNNoteType = $('.SNNoteType',$newNote);
                for (var i = 0; i < subNoteTypes.length;i++){
                    var noteType = subNoteTypes[i];
                    $SNNoteType.append($('<label class="SNTypeRadio SN'+sub+noteType.NoteTypeID+'"><input type="radio" name="SNType" value="'+noteType.NoteTypeID+'">'+noteType.DisplayName+'</label>'));
                }
                $('#SNContainer').append($newNote);
            }

            $newNote.css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
            $newNote.draggable({ handle: "div.SNHeader" });
        });
        e.target.removeEventListener(e.type, arguments.callee);
    });
})();

function getSubName(e) {

    var sub = window.snUtil.Subreddit;
    if (!sub || snUtil.ModQueue) {
        var $ot = $(e.target);
        //not a comment or browsing a sub you mod
        if (window.snUtil.Modmail) {
            var $sub = $ot.closest('.thing.message-parent').find('span.correspondent.reddit a');
            if ($sub.length > 1) {
                //multiple results here means RES / Mod toolbox is present which messes things up
                $sub = $sub.filter('.subreddit-name');
            }
            sub = $sub[0].textContent.substring(3).replace(/\//g, '');
        }
        else if (snUtil.ModQueue || snUtil.UserPage) {
            var $sub = $ot.closest('.thing').find('a.subreddit');
            var subinner = $sub[0].textContent;
            if (subinner.match(/\/r\//i)) {
                sub = subinner.substring(3).replace(/\//g, '');
            }
            else {
                sub = subinner;
            }
        }
        else {
            sub = $ot.siblings("a.subreddit:first")[0].innerHTML.substring(3).replace(/\//g, '');
        }
    }
    return sub ? sub.toLowerCase() : "";
}


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
        var sub = $submit.attr("SNSub");
        var subNoteTypes = snUtil.NoteTypes[sub];
        var $SNNoteType = $('.SNNoteType', $notecont);
        for (var i = 0; i < subNoteTypes.length; i++) {
            var noteType = subNoteTypes[i];
            $SNNoteType.append($('<label class="SNTypeRadio SN' + sub + noteType.NoteTypeID + '"><input type="radio" name="SNType" value="' + noteType.NoteTypeID + '">' + noteType.DisplayName + '</label>'));
        }
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
            //displaying add new note again doesn't work quite right so axing it for now.
            //link = /\/r\/.*/.exec(link)[0]; //trim out some of the prefix garbage that might cause issues if browsing with https etc.
            //var $entry = $('#siteTable .entry a[href$="' + link + '"], .commentarea .entry a[href$="' + link + '"]').closest('div.entry');

            $user.remove();
            //$('.SNNoNotes', $entry).trigger('click'); 
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
    var $sn = $('#SnooNote-' + e.target.attributes["SNUser"].value);
    
    var $submit = $('.SNNewNoteSubmit', $sn);
    var $ot = $(e.target);
    var sub = getSubName(e);

    var subNoteTypes = snUtil.NoteTypes[sub];
    var $SNNoteType = $('.SNNoteType', $sn);
    $SNNoteType.empty();
    for (var i = 0; i < subNoteTypes.length; i++) {
        var noteType = subNoteTypes[i];
        $SNNoteType.append($('<label class="SNTypeRadio SN' + sub + noteType.NoteTypeID + '"><input type="radio" name="SNType" value="' + noteType.NoteTypeID + '">' + noteType.DisplayName + '</label>'));
    }

    $submit.attr("SNSub", sub );
    $submit.attr("SNLink", $('ul li.first a', $ot.closest('div.entry')).attr('href'));
    $sn.css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
    $sn.draggable({ handle: "div.SNHeader" });
}

function closeNote(e) {
    $(e.target).closest('.SNViewContainer').hide();
}

function submitNote(user, sub, link, message, type, $noteCont) {
    $('.SNNewNoteSubmit, .SNNewMessage', $noteCont).attr('disabled', 'disabled');
    $noteCont.find('.SNNewError').empty();
    $.ajax({
        url: window.snUtil.ApiBase + "note",
        method: "POST",
        datatype: "application/json",
        data: { "NoteTypeID": type, "SubName": sub, "Message": message, "AppliesToUsername": user, "Url": link },
        success: function (d, status, jqXHR) {
            $('#SnooNote-' + user.toLowerCase() + ' .SNNewMessage').val('');
            $('#SnooNote-' +user.toLowerCase() + ' .SNNewNoteSubmit, #SnooNote-' + user.toLowerCase() + ' .SNNewMessage').removeAttr('disabled');
        },
        error: function () {
            $('#SnooNote-' +user.toLowerCase() + ' .SNNewNoteSubmit, #SnooNote-' +user.toLowerCase() + ' .SNNewMessage').removeAttr('disabled');
            $('#SnooNote-' +user.toLowerCase() + ' .SNNewError').append($("<p>Something goofed. You can try resubmitting the note, but I'm not promising anything...</p>"));
        }
    });
}
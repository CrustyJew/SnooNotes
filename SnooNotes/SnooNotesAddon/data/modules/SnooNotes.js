(function () {
    window.addEventListener("snUtilDone", function () {
        $('.sitetable').on('click', '.SNViewNotes', function (e) {
            showNotes(e);
        });
        $('#SNContainer').on('click', '.SNCloseNote', function (e) {
            closeNote(e);
        });
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
    $('#SnooNote-' + e.originalEvent.originalTarget.attributes["SNUser"].value).css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
}

function closeNote(e) {
    $(e.originalEvent.originalTarget).closest('.SNViewContainer').hide();
}


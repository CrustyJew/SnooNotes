(function () {
    window.addEventListener("snUtilDone", function () {
        $('.sitetable').on('click', '.SNViewNotes', function (e) {
            showNotes(e);
        });
        $('#SNContainer').on('click', '.SNCloseNote', function (e) {
            closeNote(e);
        });
    });
})();

function showNotes (e){
    $('#SnooNote-' + e.originalEvent.originalTarget.attributes["SNUser"].value).css({ 'top': e.pageY, 'left': e.pageX }).fadeIn('slow');
}

function closeNote(e) {
    $(e.originalEvent.originalTarget).closest('.SNViewContainer').hide();
}


function showLoginPopup() {
    var $container = $('' +
        '<div class="SnooNotesLoginContainer" style="display:none;">' +
        '<button onclick="$(\'.SnooNotesLoginContainer\').hide();" class="SnooNotesCancelLogin" />' +
        '<div class="SnooNotesDoneLogin" style="display:none;"><h1>All logged in?</h1><button id="SnooNotesConfirmLoggedIn">Click here!</button></div>' +
        '<iframe id="SnooNotesLoginFrame" frameborder="0" scrolling="no" src="' + snUtil.LoginAddress + '"></iframe></div>');
    $('body').append($container);
    $('#SnooNotesConfirmLoggedIn').on('click', function () { $('.SnooNotesLoginContainer').hide(); initSnooNotes(); });

    window["addEventListener"]("message",function(){ $('#SnooNotesLoginFrame').hide(); $('.SnooNotesDoneLogin').show();  });
    $container.show();
}
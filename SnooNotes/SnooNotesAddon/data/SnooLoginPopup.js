function showLoginPopup() {
    var $container = $('' +
        '<div class="SnooNotesLoginContainer">' +
        '<iframe id="SnooNotesLoginFrame" frameborder="0" scrolling="no" src="' + snUtil.LoginAddress + '"></iframe></div>');
    $('body').append($container);
}
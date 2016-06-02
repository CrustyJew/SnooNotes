function processSnooNotes(){
    getEntriesToProcess();
}
var retrynum = 0; //this is bad and probably should get changed to a passed param.
function getEntriesToProcess() {

    var $SNEntries = {};
    $SNEntries = $('.sitetable .thing .entry:not(.SNDone), .commentarea .thing .entry:not(.SNDone)');
    
    var SNUsers = [];
    if (!snUtil.UsersWithNotes) {
        if (retrynum > 75) return;//fuck on outa here if something is wrong with user;
        else {
            retrynum += 1;
            console.log("Users was undefined " + retrynum + " times!");
            setTimeout(getEntriesToProcess, retrynum * 5 + 50 );
            return;
        }
    }
    retrynum = 0;
    $('div.thing:not(.SNDone,.SNFetching)').each(function () {
        var $thing = $(this);
        var $authElem = $thing.children('.entry').find('.author:not(.moderator)');
        var sub = $thing.attr('data-subreddit');
        var author = $thing.attr('data-author');
        if (!sub) {
            $thing.addClass('SNDone');
            return;
        }
        if ($authElem.parent().hasClass('recipient')) return;

        if (snUtil.settings.moddedSubs.indexOf(sub.toLowerCase()) > -1) {
            if (snUtil.settings.usersWithNotes.indexOf(author.toLowerCase()) > -1) {
                if ($('#SnooNote-' + author.toLowerCase()).length == 0) {
                    //we don't have the note loaded yet
                    if (SNUsers.indexOf(author.toLowerCase()) == -1) {
                        //the user isn't in the list to request yet
                        SNUsers.push(author.toLowerCase());
                    }
                    $thing.addClass('SNFetching');
                }
                else {
                    //already have the notes for the user
                    $thing.addClass('SNDone');
                }
                $authElem.after('<a SNUser="' + author.toLowerCase() + '" class="SNViewNotes">[view&nbsp;note]</a>');
            }
            else {
                //user doesn't have notes
                $thing.addClass('SNDone');
                $authElem.after('<a SNUser="' + author.toLowerCase() + '" class="SNNoNotes">[add&nbsp;note]</a>')
            }
        }
    })
    if (snUtil.UserPage) {      
        var $user =  $('body.profile-page .side .titlebox h1')
        var uname = $user[0].textContent.toLowerCase();
        if (new RegExp("," + uname + ",", "i").test(snUtil.UsersWithNotes)) {
            if ($('#SnooNote-' + uname).length == 0 && SNUsers.indexOf(uname) == -1) {
                SNUsers.push(uname);
            }
            if ($user.children('a').length <= 0) {
                $user[0].textContent = $user[0].textContent + '  ';
                $user.append($('<a SNUser="' + uname + '" class="SNViewNotes SNNoLink SNOpenRight">[view&nbsp;note]</a>'));
            }
        }
        else {
            if ($user.children('a').length <= 0) {
                $user[0].textContent = $user[0].textContent + '  ';
                $user.append($('<a SNUser="' + uname + '" class="SNNoNotes SNNoLink SNOpenRight">[add&nbsp;note]</a>'));
            }
        }
    }
    if (SNUsers.length > 0) {
        snUtil.getNotesForUsers(SNUsers);
    }

}
function getUsersToProcess(){
    window.$SNUsers = {};
    $SNUsers = $('')

}
function processEntries(notes) {
   
    $('#SNContainer').append($(notes));
    if (notes) {
        $('.thing.SNFetching').removeClass("SNFetching").addClass("SNDone"); //TODO check this to make sure it won't lose notes / users randomly.s
    }
}

(function () {
    window.addEventListener("snUtilDone", function () {
        processSnooNotes();
    });
    window.addEventListener("snGotNewThings", function () {
        processSnooNotes();
    });
})();
function processSnooNotes(){
    getEntriesToProcess();
}

function getEntriesToProcess(){
    var $SNEntries = {};
    $SNEntries = $('.sitetable .thing .entry:not(.SNDone)');
    

    var SNUsers = [];
    if(snUtil.Subreddit){
        if($.inArray(snUtil.Subreddit , snUtil.ModdedSubs)){
            $('.author', $SNEntries).each(function (index, $ent) {
                if ($('#SnooNote-' + $ent.innerHTML).length == 0 && $.inArray(snUtil.UsersWithNotes, $ent.innerHTML)) {
                    if ($('#SnooNote-' + $ent.innerHTML).length == 0) {
                        SNUsers.push($ent.innerHTML);
                    }
                    $('<a onclick="$(\'#SnooNote-' + $ent.innerHTML + '\').show()">view note</a>').insertAfter($ent);
                }
                else {
                    //TODO add icon for new note
                    $ent.closest('div').addClass('SNDone');
                }
            });
        }
        else { //not browsing a /r/ you moderate
            $SNEntries.each(function (index, $ent) {
                if ($.inArray($('.tagline a.subreddit',$ent).innerHTML,snUtil.ModdedSubs)) {
                    var auth = ('.author',$ent);
                    if ($.inArray(snUtil.UsersWithNotes, auth.innerHTML)) {
                        if ($('#SnooNote-' + auth.innerHTML).length == 0) {
                            SNUsers.push(auth.innerHTML);
                        }
                        $('<a onclick="$(\'#SnooNote-'+auth.innerHTML+'\').show()">view note</a>').insertAfter($ent);
                    }
                    else {
                        //TODO add icon for new note
                        $ent.addClass('SNDone');
                    }
                }
            });
            
        }
    }
    if (SNUsers.length > 0) {
        snUtil.GetNotesForUsers(SNUsers);
    }

}
function getUsersToProcess(){
    window.$SNUsers = {};
    $SNUsers = $('')

}
function processEntries(notes){
    $('body').append($(notes));
}

(function () {
    window.addEventListener("snUtilDone", function () {
        processSnooNotes();
    });
})();
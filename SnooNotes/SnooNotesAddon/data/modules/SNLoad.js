function processSnooNotes(){
    getEntriesToProcess();
}

function getEntriesToProcess(){
    var $SNEntries = {};
    $SNEntries = $('.sitetable .thing .entry:not(.SNDone)');
    

    var SNUsers = [];
    if (!snUtil.UsersWithNotes || snUtil.UsersWithNotes == ",,") return; //fuck on outa here if no users with notes;
    if(snUtil.Subreddit){
        if (new RegExp("," + snUtil.Subreddit + ",", "i").test(snUtil.ModdedSubs)) {
            console.log("Viewing sub that you mod");
            $('.author', $SNEntries).each(function (index, ent) {
                var $container = $(ent).closest('div');
                //console.log("," + ent.innerHTML + "," + " ------ " + snUtil.UsersWithNotes);
                if (new RegExp("," + ent.innerHTML + ",","i").test(snUtil.UsersWithNotes)) {
                    if ($container.hasClass('SNFetching') && SNUsers.indexOf(ent.innerHTML) == -1) { //don't add doubles
                        SNUsers.push(ent.innerHTML);
                    }
                    else {
                        if ($('#SnooNote-' + ent.innerHTML.toLowerCase()).length == 0) {
                            if (SNUsers.indexOf(ent.innerHTML) == -1) {
                                SNUsers.push(ent.innerHTML);
                            }
                            $container.addClass('SNFetching');
                        }
                        else {
                            $container.addClass('SNDone');
                        }
                        $('<a onclick="$(\'#SnooNote-' + ent.innerHTML.toLowerCase() + '\').show()">view note</a>').insertAfter(ent);
                    }
                    
                }
                else {
                    //TODO add icon for new note
                    $container.addClass('SNDone');
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
                        $('<a onclick="$(\'#SnooNote-'+auth.innerHTML.toLowerCase()+'\').show()">view note</a>').insertAfter($ent);
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
function processEntries(notes) {
   
    $('#SNContainer').append($(notes));
    if (notes) {
        $('.sitetable .thing .entry.SNFetching').removeClass("SNFetching").addClass("SNDone"); //TODO check this to make sure it won't lose notes / users randomly.s
    }
}

(function () {
    window.addEventListener("snUtilDone", function () {
        processSnooNotes();
    });
})();
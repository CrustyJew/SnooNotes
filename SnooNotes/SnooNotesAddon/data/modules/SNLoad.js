function processSnooNotes(){
    getEntriesToProcess();
}

function getEntriesToProcess(){
    var $SNEntries = {};
    $SNEntries = $('.sitetable .thing .entry:not(.SNDone)');
    $SNEntries.addClass('SNDone');

    var SNUsers = [];
    if(snUtil.Subreddit){
        if($.inArray(snUtil.Subreddit , snUtil.ModdedSubs)){
            $('.author',$SNEntries).each(function(index, $ent){
                SNUsers.push($ent.innerHTML);
            });
        }
    }


}
function getUsersToProcess(){
    window.$SNUsers = {};
    $SNUsers = $('')

}
function processEntries(){

}

(function () {
    window.addEventListener("snUtilDone", function () {
        processSnooNotes();
    });
})();
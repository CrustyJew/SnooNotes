function snBanNotes(){
    var self = {};
    self.init = function () {
        if (window.location.pathname.match(/\/about\/banned/i)) {
            document.getElementById('banned').addEventListener("submit", function (e) {
                self.removeDOMListener();
                var dur = document.getElementById('duration').value;
                var sub = snUtil.Subreddit;
                var type = -1;
                if (dur) {
                    //temp ban
                    type = snUtil.SubSettings[sub.toLowerCase()].TempBanID;
                }
                else {
                    type = snUtil.SubSettings[sub.toLowerCase()].PermBanID;
                }

                if (!type || type < 0) {
                    //doesn't have a note type for this ban, run like the wind!

                }
                else {
                    //has a note type, bind up the events yo.
                    var link = "https://reddit.com/u/" + user;
                    var user = document.getElementById('name').value;
                    var message = 'Note: ' + document.getElementById('note').value + ' - ' 
                        +(dur ? dur + ' days' : 'Permanent') + '\n\n'
                        + 'Message: ' + document.getElementById('ban_message').value;
                    
                    $('.banned-table').one('DOMSubtreeModified', function (ae, jq, ad) {
                        //if it wasn't an error
                        //alert('slkdfjsldkjfsd');
                        //if (!(/error/i).test(JSON.parse(jq.responseText).jquery[10][3][0])) {
                        self.addNote(user, sub, link, message, type);
                        //}
                    });
                    window.setTimeout(self.removeDOMListener,5000);
                }
            });
        }
    }
    self.addNote = function(user,sub,link,message,type){
        submitNote(user, sub, link, message, type);
    }
    self.removeDOMListener = function () {
        $('.banned-table').off('DOMSubtreeModified')
    }
    self.init();
}


(function () {
    window.addEventListener("snUtilDone", function (e) {
        snBanNotes();
        e.target.removeEventListener(e.type, arguments.callee);
    });
})();



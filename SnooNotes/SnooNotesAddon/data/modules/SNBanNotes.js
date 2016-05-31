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
                    type = snUtil.settings.subSettings[sub.toLowerCase()].TempBanID;
                }
                else {
                    type = snUtil.settings.subSettings[sub.toLowerCase()].PermBanID;
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
        $('body').on('click', '.mod-popup .save', function () {
            var $popup = $(this).closest('div.mod-popup');
            var $meta = $popup.children('div.meta');
            var action = $('select.mod-action', $popup).val();
            var sub = $('label.subreddit', $meta).text();

            if (action == "ban" && snUtil.settings.moddedSubs.indexOf(sub) > -1) {
                //mod this sub w/ snoonotes
                var dur = $('input.ban-duration', $popup).val();
                var type = -1;
                if (dur) {
                    //temp ban
                    type = snUtil.settings.subSettings[sub.toLowerCase()].TempBanID;
                }
                else {
                    type = snUtil.settings.subSettings[sub.toLowerCase()].PermBanID;
                }

                if (!type || type < 0) {
                    //doesn't have a note type for this ban, run like the wind!

                }
                else {
                    var user = $('label.user', $meta).text();
                    var thing = $('label.thing_id', $meta).text();
                    var link = $('.thing[data-fullname="' + thing + '"] .entry:first ul li.first a').attr('href');
                    var message = 'Note: ' + $('.ban-note', $popup).val() + ' - '
                            + (dur ? dur + ' days' : 'Permanent') + '\n\n'
                            + 'Message: ' + $('.ban-message', $popup).val();

                    self.addNote(user, sub, link, message, type);
                }
            }
            
        });
    }
    self.addNote = function(user,sub,link,message,type){
        submitNote(user, sub, link, message, type);
    }
    self.removeDOMListener = function () {
        $('.banned-table').off('DOMSubtreeModified');
    }
    self.init();
}


(function () {
    window.addEventListener("snUtilDone", function (e) {
        snBanNotes();
        e.target.removeEventListener(e.type, arguments.callee);
    });
})();



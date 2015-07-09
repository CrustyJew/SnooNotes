function renderOptionsButton(msg) {
    var $optBtn = $('#SNOptionsBtn');
    if ($optBtn.length < 1) {
        //option button hasn't been created yet
        //commented until I get icons up in hur
        //$optBtn = $('<input type="button" id="SNOptionsBtn" class="SNOptionsBtn" />');
        $optBtn = $('<a href="#" id="SNOptionsBtn" class="SNOptionsBtn"></a>')
        $('#modmail').after([$('<span class="separator">|</span>'), $optBtn]);
        bindOptionClick();
    }
    if (!msg) {
        $optBtn.attr("class","SNOptionsBtn");
    }
    else if (msg == "LoggedOut") {
        $optBtn.attr("class", "SNOptionsBtn SNLoggedOut");
        $optBtn.text("[SN Login!]");
    }
    else if (msg == "LoggedIn") {
        $optBtn.attr("class", "SNOptionsBtn SNLoggedIn");
        $optBtn.text("[SN Options]");
    }
}


    window.addEventListener("snUtilDone", function (e) {
        renderOptionsButton("init");
    });
    window.addEventListener("snLoggedOut", function (e) {
        renderOptionsButton("LoggedOut");
    });
    window.addEventListener("snLoggedIn", function (e) {
        if ($('#SNModal').is(":visible")) {
            renderOptionsContainer();
        }
        renderOptionsButton("LoggedIn");
    })

    function bindOptionClick() {
        $('#SNOptionsBtn').click(function (e) {
            renderOptionsContainer();
        });
    }

    function renderOptionsContainer() {
        var modal = "";
        if (!snUtil.LoggedIn) {
            modal = '<div class="SnooNotesLoginContainer">' +
            '<div class="SnooNotesDoneLogin" style="display:none;"><h1>All logged in?</h1><button id="SnooNotesConfirmLoggedIn">Click here!</button></div>' +
            '<iframe id="SnooNotesLoginFrame" frameborder="0" scrolling="no" src="' + snUtil.LoginAddress + '"></iframe></div>';
            window.addEventListener("message", LoggingInEvent, false);
        }
        else {
            modal = '<div class="SNOptionsMain"><h1>Options will be here.... Eventually.. I swear..</h1></div>';
        }
        snUtil.ShowModal(modal);
    }
    function LoggingInEvent(msg){
        if(msg.data.LoggingIn){
            $('#SnooNotesLoginFrame').hide(); 
            $('.SnooNotesDoneLogin').show();
            $('#SnooNotesConfirmLoggedIn').on('click', function () { $('.SnooNotesLoginContainer').hide(); checkLoggedIn(); });
        }
    }
/*
function loadModToolboxNotesHTML() {
    var html = '' +
        '<div style="width:100%;min-height:600px">' +
            '<div style="width:49%;float:left;height:100%;">' +
                '<textarea id="SNModToolboxNotes" style="width:100%;height:600px;resize:none;"></textarea>' +
            '</div>' +
        '</div>';
    return html;
}
//$('body').html(loadModToolboxNotesHTML())


var data = {};
$.ajax('/r/videos/wiki/usernotes.json', {
    dataType: "json",
    success: function (json) {
        var wikiData = json.data.content_md;
        data = JSON.parse(wikiData);
    }
});


//data

var notes = {};

$.each(data.users, function (name, user) {
    notes[name] = {
        "name": name,
        "notes": user.ns.map(function (note) {
            var n = {};
            n.note = $('<div />').html(note.n).text();
            n.time = new Date(data.ver >= 5 && note.t.toString().length <= 10 ? note.t * 1000 : note.t).toISOString();
            n.mod = data.constants.users[note.m];
            n.link = unsquashLink("videos", note.l);
            n.type = data.constants.warnings[note.w];
            return n;
        })
    }
});

function unsquashLink(subreddit, permalink) {
    if (!permalink)
        return '';

    var linkParams = permalink.split(/,/g);
    var link = "/r/" + subreddit + "/";

    if (linkParams[0] == "l") {
        link += "comments/" + linkParams[1] + "/";
        if (linkParams.length > 2)
            link += "-/" + linkParams[2] + "/";
    }
    else if (linkParams[0] == "m") {
        link += "message/messages/" + linkParams[1];
    }
    else {
        return "";
    }

    return link;
}

var insert = "";
$.each(notes, function (username, user) {
    $.each(user.notes, function (i, note) {
        insert += "('" + note.type + "',2,'" + note.mod + "','" + note.note.replace(/'/g, "''") + "','" + username + "','https://reddit.com" + note.link + "','" + note.time + "'),";
    })
})

insert;*/
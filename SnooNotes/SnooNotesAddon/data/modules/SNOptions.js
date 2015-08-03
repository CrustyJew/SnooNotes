function renderOptionsButton(msg) {
    var $optBtn = $('#SNOptionsBtn');
    if ($optBtn.length < 1) {
        //option button hasn't been created yet
        //commented until I get icons up in hur
        //$optBtn = $('<input type="button" id="SNOptionsBtn" class="SNOptionsBtn" />');
        $optBtn = $('<a id="SNOptionsBtn" class="SNOptionsBtn"></a>')
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
        modal = '<div id="SNOptionsContainer">'+
                    '<div id="SNOptionsSidebar">' +
                        '<div id="SNOptionsSubOpts" class="SNOptionsCategory active">Subreddits</div>' +
                        '<div id="SNOptionsPlaceholder1" class="SNOptionsCategory">Placeholder 1</div>' +
                        '<div id="SNOptionsPlaceholder2" class="SNOptionsCategory">Placeholder 2</div>' +
                    '</div>' +
                    '<div id="SNOptionsPanel"><div id="SNOptionsContents">' +
                            snSubredditOptions() + 
                    '</div></div>' +
                '</div>';
    }
    snUtil.ShowModal(modal,snBindOptionEvents);
}
function LoggingInEvent(msg){
    if(msg.data.LoggingIn){
        $('#SnooNotesLoginFrame').hide(); 
        $('.SnooNotesDoneLogin').show();
        $('#SnooNotesConfirmLoggedIn').on('click', function () { $('.SnooNotesLoginContainer').hide(); checkLoggedIn(); });
    }
}

function snSubredditOptions() {
    var subOpts = "";
    subOpts = '<div style="display:inline-block;width:100%;"><h1 style="float:left;">Has something gone rogue? <br />Change subreddits you moderate?<br />Activate a new sub?</h1><button type="button" id="SNRestart" class="SNBtnWarn" style="margin-top:20px;margin-left:15px;">Refresh SnooNotes</button>' +
                '<br style="clear:both;"/>' +
                '<div style="margin:0px auto;width:300px;margin-bottom:15px;"><select id="SNActivateSub"><option value="-1">---Activate a new Subreddit---</option></select><button type="button" id="SNBtnActivateSub" class="SNBtnSubmit">Activate</button></div>' +
                '<div id="SNSubredditsContainer"></div>' +
                '<div id="SNSubRedditSettings" style="display:none;">' +
                    snSubOptDescriptions() + 
                    '<div class="SNOptsHeader"><h1 class="SNSubOptsHeader"></h1><button type="button" class="SNBtnCancel" id="SNBtnSubOptsCancel">Cancel</button><br style="clear:both;"></div>' +
                    '<form id="SNSubOptsForm">' +
                        '<div class="SNContainer"></div>' +
                        '<button type="button" class="SNBtnSubmit" id="SNBtnSubOptsSave">Save</button></div>'+
                    '</form>';
    snGetSubSettings();
    snGetInactiveSubs();
    return subOpts;
}
function snSubOptDescriptions() {
    var descs = '' +
        '<div id="SNAccessMaskDesc">Choose who can view and add notes below. Anyone with full permissions can always view and add notes as well as edit this page</div>';
    return descs;
}
function snBindOptionEvents() {
    $('#SNSubredditsContainer').on('click', '.SNBtnSettings', function (e) {
        
        var sub = e.target.attributes["snsub"].value;
        $('#SNSubredditsContainer').hide();
        $('#SNSubRedditSettings').show();
        $('.SNSubOptsHeader').text('/r/' + sub);
        $('.SNSubreddit:not([snsub=' + sub + '])').hide();
        var $subopts = $('.SNSubreddit[snsub=' + sub + ']');
        $subopts.show();
        $('#SNAccessMaskDesc').prependTo($('.SNAccessMask', $subopts));
    });
    $('#SNBtnSubOptsCancel').on('click', function () {
        $('#SNSubRedditSettings').hide();
        $('#SNSubOptsForm')[0].reset();
        $('#SNSubredditsContainer').show();
    });
    $('#SNBtnSubOptsSave').on('click', function () {
        $('#SNModal').block({ message: '<h1>Charging AMEX card for changes made...</h1>' });
        $('.SNSubreddit:visible').each(function (i, o) {
            var data = {};
            data.SubName = o.attributes['snsub'].value;
            data.Settings = {};
            data.Settings.AccessMask = 64;
            $('.SNAccessMaskOptions input:checked', o).each(function (ii, chkb) {
                data.Settings.AccessMask += parseInt(chkb.value);
            });
            $.ajax({
                url: snUtil.RESTApiBase + "Subreddit/" + data.SubName,
                method: "PUT",
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                data: data,
                datatype: "Application/JSON",
                success: function () {
                    $.unblockUI();
                    $('#SNModal').block({
                        message: '<div class="growlUI growlUISuccess"><h1>Success!</h1><h2>Settings have been altered.<br />Pray I do not alter them further....</h2></div>',
                        fadeIn: 500,
                        fadeOut: 700,
                        timeout: 2000,
                        centerY: !0,
                        centerX: !0,
                        showOverlay: !1,
                        css: $.blockUI.defaults.growlCSS
                    });
                    $('.SNSubreddit:visible .SNAccessMaskOptions input:checked').attr('checked', 'checked');
                    $('#SNSubRedditSettings').hide();
                    $('#SNSubredditsContainer').show();
                }
            });
        });
    });
    $('#SNRestart').on('click', function () {
        $('#SNModal').block({ message: '<h1>Attempting to shoo gremlins...</h1>' });
        $.ajax({
            url: snUtil.ApiBase + "account/UpdateModeratedSubreddits",
            method: "GET",
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (d, s, x) {
                snUtil.reinitAll();
                $.unblockUI();
                $('#SNModal').block({
                    message: '<div class="growlUI growlUISuccess"><h1>Success!</h1><h2>Gremlins have flame throwered.</h2></div>',
                    fadeIn: 500,fadeOut: 700,timeout: 2000,centerY: !0,centerX: !0,showOverlay: !1,css: $.blockUI.defaults.growlCSS
                });
            },
            error: function () {
                $('#SNModal').block({
                    message: '<div class="growlUI growlUIError"><h1>Error!</h1><h2>Gremlins have won, blame the admins.</h2></div>',
                    fadeIn: 500,fadeOut: 700,timeout: 2000,centerY: !0,centerX: !0,showOverlay: !1,css: $.blockUI.defaults.growlCSS
                });
            }
        });
    });
    $('#SNBtnActivateSub').on('click', function () {
        var sub = $('#SNActivateSub').val();
        if (sub == "-1") {
            $('#SNActivateSub').css("border:2px solid red;");
        } else {
            $('#SNModal').block({ message: '<h1>Waking up the Balrog, err, fluffy bunny.. yeah..</h1>' });
            $.ajax({
                url: snUtil.RESTApiBase + "subreddit",
                method: 'POST',
                data: { SubName: sub },
                success: function () {
                    $('#SNModal').unblock();
                    $('#SNModal').block({
                        message: '<div class="growlUI growlUISuccess"><h1>Success!</h1><h2>Summoning ritual has been completed.</h2></div>',
                        fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1, css: $.blockUI.defaults.growlCSS
                    });
                    snUtil.reinitAll();
                },
                error: function () {
                    $('#SNModal').block({
                        message: '<div class="growlUI growlUIError"><h1>Error!</h1><h2>Rolled a natural 0. If this persists, contact /u/snoonotes.</h2></div>',
                        fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1, css: $.blockUI.defaults.growlCSS
                    });
                }
            });
        }
    });
}
function snGetSubSettings() {

    $('#SNSubredditsContainer').block({message:"<h1>Fetching things for master</h1>",fadeIn:0});
    $.ajax({
        url: snUtil.RESTApiBase + "Subreddit/",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            var activeSubs = d;
            var subOpts = "";
            var subOptsPanel = "";
            for (var i = 0; i < activeSubs.length; i++) {
                var sub = activeSubs[i];
                if (sub) {
                    subOpts += '<div class="SNSubredditBtn"><button type="button" class="SNBtnSettings" snsub="' + sub.SubName + '">/r/' + sub.SubName + '</button></div>';
                    subOptsPanel += '<div class="SNSubreddit" snsub="'+sub.SubName+'" style="display:none;">'+
                                        '<div class="SNAccessMask"><div class="SNAccessMaskOptions">' +
                                            '<label><input type="checkbox" value="1" '+(sub.Settings.AccessMask & snUtil.Permissions.Access ? 'checked="checked"' : '') +'>access</label>' +
                                            '<label><input type="checkbox" value="2" ' + (sub.Settings.AccessMask & snUtil.Permissions.Config ? 'checked="checked"' : '') + '>config</label>' +
                                            '<label><input type="checkbox" value="4" ' + (sub.Settings.AccessMask & snUtil.Permissions.Flair ? 'checked="checked"' : '') + '>flair</label>' +
                                            '<label><input type="checkbox" value="8" ' + (sub.Settings.AccessMask & snUtil.Permissions.Mail ? 'checked="checked"' : '') + '>mail</label>' +
                                            '<label><input type="checkbox" value="16" ' + (sub.Settings.AccessMask & snUtil.Permissions.Posts ? 'checked="checked"' : '') + '>posts</label>' +
                                            '<label><input type="checkbox" value="32" ' + (sub.Settings.AccessMask & snUtil.Permissions.Wiki ? 'checked="checked"' : '') + '>wiki</label>' +
                                        '</div></div>' +
                                    '</div>';
                }
            }
            $(function () {
                $('#SNSubredditsContainer').remove('.SNSubredditBtn').append($(subOpts));
                $('#SNSubRedditSettings .SNContainer').remove('.SNSubreddit').append($(subOptsPanel));
                $('#SNSubredditsContainer').unblock();
            });
        },
        error: function () {
            $('#SNSubredditsContainer').unblock();
            $('#SNSubredditsContainer').append($('<h1 style="color:red;">Something went horribly wrong, try and reload the options window to fix it</h1>'));
        }
    });
}
function snGetInactiveSubs() {
    $.ajax({
        url: snUtil.ApiBase + "Account/GetInactiveModeratedSubreddits/",
        method: "GET",
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (d, s, x) {
            var $opts = $('#SNActivateSub');
            if (d.length == 0) {
                $opts.closest('div').hide();
            }
            for (var i = 0; i < d.length; i++) {
                $opts.append($('<option value="' + d[i] + '">' + d[i] + '</option>'));
            }
        }
    });
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
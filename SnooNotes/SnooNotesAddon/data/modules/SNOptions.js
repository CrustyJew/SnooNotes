var snSubAccessDirty = false;
var snSubBanNoteTypeDirty = false;

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
        $optBtn.attr("class", "SNOptionsBtn");
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

    snSubAccessDirty = false;
    snSubBanNoteTypeDirty = false;

    if (!snUtil.settings.loggedIn) {
        modal = '<div class="SnooNotesLoginContainer">' +
        '<div class="SnooNotesDoneLogin" style="display:none;"><h1>All logged in?</h1><button id="SnooNotesConfirmLoggedIn">Click here!</button></div>' +
        '<iframe id="SnooNotesLoginFrame" frameborder="0" scrolling="no" src="' + snUtil.LoginAddress + '"></iframe></div>';
        window.addEventListener("message", LoggingInEvent, false);
        window.addEventListener("snLoggedInSuccess", function (e) {
            LoggingInEvent({ data: { LoggingIn: "true" } });
        });
    }
    else {
        modal = '<div id="SNOptionsContainer">' +
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
    snUtil.ShowModal(modal, snBindOptionEvents);
}
function LoggingInEvent(msg) {
    if (msg.data.LoggingIn) {
        $('#SnooNotesLoginFrame').hide();
        $('.SnooNotesDoneLogin').show();
        $('#SnooNotesConfirmLoggedIn').on('click', function () { $('.SnooNotesLoginContainer').hide(); snBrowser.reinitAll(); });
    }
    else if (msg.data.LoggedInSuccess) {
        $('.SnooNotesLoginContainer').hide(); snBrowser.reinitAll();
    }
}

function snSubredditOptions() {
    var subOpts = "";
    subOpts = '<div style="display:inline-block;width:100%;"><h1 style="float:left;">Has something gone rogue? <br />Change subreddits you moderate?<br />Activate a new sub?</h1><button type="button" id="SNRestart" class="SNBtnWarn" style="margin-top:20px;margin-left:15px;">Refresh SnooNotes</button>' +
                '<br style="clear:both;"/>' +
                '<div style="margin:0px auto;width:310px;margin-bottom:15px;"><select id="SNActivateSub"><option value="-1">---Activate a new Subreddit---</option></select><button type="button" id="SNBtnActivateSub" class="SNBtnSubmit">Activate</button></div>' +
                '<div id="SNSubredditsContainer"></div>' +
                '<div id="SNSubRedditSettings" style="display:none;">' +
                    snSubOptDescriptions() +
                    '<div class="SNOptsHeader"><h1 class="SNSubOptsHeader"></h1><button type="button" class="SNBtnCancel" id="SNBtnSubOptsCancel">Cancel</button><br style="clear:both;"></div>' +
                    '<form id="SNSubOptsForm">' +
                        '<div class="SNContainer"></div>' +
                        '<button type="button" class="SNBtnSubmit" id="SNBtnSubOptsSave">Save</button></div>' +
                    '</form>';
    snGetSubSettings();
    snGetInactiveSubs();
    return subOpts;
}
function snSubOptDescriptions() {
    var descs = '' +
        '<div id="SNAccessMaskDesc">Choose who can view and add notes below. Anyone with full permissions can always view and add notes as well as edit this page</div>'+
        '<div id="SNNoteTypesDesc">Change just about everything about the Note Types belonging to this subreddit below. If no checkbox is chosen for Perm Ban or Temp Ban, then automatic ban notes will not be generated for that type of ban.<br><br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Temp&nbsp;|&nbsp;Perm<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Ban&nbsp;&nbsp;|&nbsp;&nbsp;Ban</div>';
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
        $('#SNNoteTypesDesc').prependTo($('.SNNoteTypes', $subopts));
    });
    $('#SNBtnSubOptsCancel').on('click', function () {
        snSubAccessDirty = false;
        snSubBanNoteTypeDirty = false;
        $('#SNSubRedditSettings').hide();
        $('#SNSubOptsForm')[0].reset();
        $('#SNSubredditsContainer').show();
        
        resetNoteTypes();
    });
    $('#SNBtnSubOptsSave').on('click', function () {
        $('#SNModal').block({ message: '<h1>Charging AMEX card for changes made...</h1>' });
        var o = $('.SNSubreddit:visible')[0];
        var subname = o.attributes['snsub'].value;
        var subdata = {};
        subdata.SubName = subname;
        subdata.Settings = {};
        subdata.Settings.AccessMask = 64;

        var dSub = {};
        if (snSubAccessDirty || snSubBanNoteTypeDirty) {
            $('.SNAccessMaskOptions input:checked', o).each(function (ii, chkb) {
                subdata.Settings.AccessMask += parseInt(chkb.value);
            });
            subdata.Settings.TempBanID = $('input.SNChkGrp[name="ChkTempBan"]:checked', o).val();
            subdata.Settings.PermBanID = $('input.SNChkGrp[name="ChkPermBan"]:checked', o).val();

            dSub = $.ajax({
                url: snUtil.RESTApiBase + "Subreddit/" + subdata.SubName,
                method: "PUT",
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                data: subdata,
                datatype: "Application/JSON",
                success: function () {

                    $('.SNSubreddit:visible .SNAccessMaskOptions input:checked').attr('checked', 'checked');

                }
            });
        }

        var ntAddData = [];
        var ntDelData = [];
        var ntUpdData = [];
        var dispOrderCounter = 0;
        $('.SNNoteTypes li', o).each(function (i, ntli) {
            var $ntli = $(ntli);
            var nt = {};
            nt.SubName = subname;
            nt.NoteTypeID = $ntli.attr('SNNoteTypeID');
            //notetype add or update
            if ($ntli.is(':visible')) {

                nt.DisplayOrder = dispOrderCounter;
                dispOrderCounter++;
                if ($ntli.is('[SNChanged="true"]') || nt.DisplayOrder != $ntli.attr("SNNoteTypeDisplayOrder")) {
                    nt.DisplayName = $ntli.children('.SNNoteTypeDisp').val();
                    nt.ColorCode = $ntli.children('.SNNoteTypeColor').val();
                    nt.Bold = $('.SNntBold:checked', $ntli).length > 0 ? true : false;
                    nt.Italic = $('.SNntItalic:checked', $ntli).length > 0 ? true : false;
                    //new notetype
                    if (nt.NoteTypeID == "-1") {

                        ntAddData.push(nt);
                    }
                        //update notetype
                    else {
                        ntUpdData.push(nt);
                    }
                }
            }
                //NoteType delete
            else {
                ntDelData.push(nt);
            }
        });
        var dNtAdd = !(ntAddData.length) ? {} : $.ajax({
            url: snUtil.RESTApiBase + "NoteType",
            method: "POST",
            data: JSON.stringify(ntAddData),
            dataType: "json",
            contentType: "application/json",
            success: function (d, ts, x) {
                $('.SNNoteTypes li:visible[SNNoteTypeID="-1"]').remove();
                for (var i = 0; i < d.length; i++) {
                    $('.SNSubreddit[snsub="' + d[i].SubName + '"').children('ol').append(genNoteTypeLI(d[i]));
                }
            }
        });
        var dNtUpd = !(ntUpdData.length) ? {} : $.ajax({
            url: snUtil.RESTApiBase + "NoteType",
            method: "PUT",
            data: JSON.stringify(ntUpdData),
            dataType: "json",
            contentType: "application/json",
            //traditional: true,
            success: function (d, ts, x) {
                for (var i = 0; i < d.length; i++) {
                    $('.SNSubreddit .SNNoteTypes li[SNNoteTypeID="' + d[i].NoteTypeID + '"]').replaceWith(genNoteTypeLI(d[i]));
                }
            }
        });
        var dNtDel = !(ntDelData.length) ? {} : $.ajax({
            url: snUtil.RESTApiBase + "NoteType",
            method: "DELETE",
            data: JSON.stringify(ntDelData),
            dataType: "json",
            contentType: "application/json",
            //traditional:true,
            success: function (d, ts, x) {
                for (var i = 0; i < d.length; i++) {
                    $('.SNSubreddit .SNNoteTypes li[SNNoteTypeID="' + d[i] + '"]').remove();
                }
            }
        });
        $.when(dSub, dNtAdd, dNtUpd, dNtDel).then(
            //success
            function () {
                snSubAccessDirty = false;
                snSubBanNoteTypeDirty = false;
                resetNoteTypes();
                $.unblockUI();
                $('#SNModal').block({
                    message: '<div class="growlUI growlUISuccess"><h1>Success!</h1><h2>Settings have been altered.<br />Pray I do not alter them further....</h2></div>',
                    fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1,
                    css: $.blockUI.defaults.growlCSS
                });
                $('#SNSubRedditSettings').hide();
                $('#SNSubredditsContainer').show();
            },
        //failure
           function (dSub, dNtAdd, dNtUpd, dNtDel) {
               $.unblockUI();
               $('#SNModal').block({
                   message: '<div class="growlUI growlUIError"><h1>Error!</h1><h2>Possible partial success, recommend re-opening options window</h2></div>',
                   fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1,
                   css: $.blockUI.defaults.growlCSS
               });
           }
        ); //end when/then

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
                    fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1, css: $.blockUI.defaults.growlCSS
                });
            },
            error: function () {
                $('#SNModal').block({
                    message: '<div class="growlUI growlUIError"><h1>Error!</h1><h2>Gremlins have won, blame the admins.</h2></div>',
                    fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1, css: $.blockUI.defaults.growlCSS
                });
            }
        });
    });
    $('#SNBtnActivateSub').on('click', function () {
        var sub = $('#SNActivateSub').val();
        if (sub == "-1") {
            $('#SNActivateSub').attr("style", "border:2px solid red;");
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
                    $('#SNModal').unblock();
                    $('#SNModal').block({
                        message: '<div class="growlUI growlUIError"><h1>Error!</h1><h2>Rolled a natural 0. If this persists, contact /u/snoonotes.</h2></div>',
                        fadeIn: 500, fadeOut: 700, timeout: 2000, centerY: !0, centerX: !0, showOverlay: !1, css: $.blockUI.defaults.growlCSS
                    });
                }
            });
        }
    });

    //update preview for notetype
    $('#SNSubRedditSettings').on("keyup change", ".SNSubreddit .SNNoteTypes li", function (e) {
        ntUpdatePreview(this);
        $(this).attr('SNChanged', 'true');
    });
    $('#SNSubRedditSettings').on("change", ".SNSubreddit .SNAccessMaskOptions", function (e) {
        snSubAccessDirty = true;
    });
    //remove note type (will preserve in DB)
    $('#SNSubRedditSettings').on("click", ".SNSubreddit .SNNoteTypes .SNRemove", function (e) {
        var $ntli = $(this).closest('li');
        if ($ntli.attr('SNNoteTypeID') == "-1") {
            $ntli.remove();
        }
        else {
            $ntli.hide();
        }
    });
    //Add new notetype LI
    $('#SNSubRedditSettings').on("click", ".SNSubreddit .SNNoteTypes .SNAdd", function (e) {
        var $ntlo = $(this).parent().siblings('ol'); //have to get parent because it's wrapped in a div
        var $newLI = $(genNoteTypeLI({ NoteTypeID: -1, DisplayOrder: -1, Bold: false, Italic: false, ColorCode: "000", DisplayName: "New" }));
        $newLI.attr('SNChanged', 'true');
        $ntlo.append($newLI);
        ntUpdatePreview($newLI);
    });

    $('#SNSubRedditSettings').on("click", "input.SNChkGrp", function (e) {
        var $chk = $(this);
        snSubBanNoteTypeDirty = true;
        if (this.checked) {
            var grp = this.attributes["name"].value;
            $('input:checkbox.SNChkGrp[name="' + grp + '"]', $chk.closest('.SNSubreddit')).prop("checked", false);
            this.checked = true;
        }
        else {
            this.checked = false;
}
    });
}
function snGetSubSettings() {

    $('#SNSubredditsContainer').block({ message: "<h1>Fetching things for master</h1>", fadeIn: 0 });
    $.ajax({
        url: snUtil.RESTApiBase + "Subreddit/admin",
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
                    subOptsPanel += '<div class="SNSubreddit" snsub="' + sub.SubName + '" style="display:none;">' +
                                        '<div class="SNAccessMask">' +
                                            '<div class="SNAccessMaskOptions">' +
                                                '<label><input type="checkbox" value="1" ' + (sub.Settings.AccessMask & snUtil.Permissions.Access ? 'checked="checked"' : '') + '>access</label>' +
                                                '<label><input type="checkbox" value="2" ' + (sub.Settings.AccessMask & snUtil.Permissions.Config ? 'checked="checked"' : '') + '>config</label>' +
                                                '<label><input type="checkbox" value="4" ' + (sub.Settings.AccessMask & snUtil.Permissions.Flair ? 'checked="checked"' : '') + '>flair</label>' +
                                                '<label><input type="checkbox" value="8" ' + (sub.Settings.AccessMask & snUtil.Permissions.Mail ? 'checked="checked"' : '') + '>mail</label>' +
                                                '<label><input type="checkbox" value="16" ' + (sub.Settings.AccessMask & snUtil.Permissions.Posts ? 'checked="checked"' : '') + '>posts</label>' +
                                                '<label><input type="checkbox" value="32" ' + (sub.Settings.AccessMask & snUtil.Permissions.Wiki ? 'checked="checked"' : '') + '>wiki</label>' +
                                            '</div>' +
                                            '<div style="text-align:center;margin-top:5px;">WARNING: May take up to 15 minutes for permissions to expire and refresh!</span></div>' +
                                        '</div>' +
                                        '<div class="SNNoteTypes"><div class="SNNoteTypeOptions">' +
                                            '<ol>';
                    for (var n = 0; n < sub.Settings.NoteTypes.length; n++) {
                        var nt = sub.Settings.NoteTypes[n];
                        if (nt) {
                            subOptsPanel += genNoteTypeLI(nt,sub.Settings.TempBanID,sub.Settings.PermBanID);
                        }
                    }
                    subOptsPanel += '</ol>' +
                                    '<div style="text-align:center;" ><a class="SNAdd">+</a></div>' +
                                '</div></div>' +
        '</div>';
                }
            }
            $(function () {
                $('#SNSubredditsContainer').remove('.SNSubredditBtn').append($(subOpts));
                $('#SNSubRedditSettings .SNContainer').remove('.SNSubreddit').append($(subOptsPanel));
                resetNoteTypes();

                $('.SNNoteTypeOptions ol').sortable({ axis: "y", containment: "parent", tolerance: 'pointer' });

                $('#SNSubredditsContainer').unblock();
            });
        },
        error: function () {
            $('#SNSubredditsContainer').unblock();
            $('#SNSubredditsContainer').append($('<h1 style="color:red;">Something went horribly wrong, try and reload the options window to fix it</h1>'));
        }
    });
}
function genNoteTypeLI(nt,tempBanID,permBanID) {
    var ntLI = '<li SNNoteTypeID="' + nt.NoteTypeID + '" SNNoteTypeDisplayOrder="' + nt.DisplayOrder + '">' +
                                                    '<a class="SNSort"></a>' +
                                                    '<input type="checkbox" class="SNChkGrp" value="' + nt.NoteTypeID + '" name="ChkTempBan" ' + (nt.NoteTypeID == tempBanID ? 'checked="checked"' : '') + '>' +
                                                    '<input type="checkbox" class="SNChkGrp" value="' + nt.NoteTypeID + '" name="ChkPermBan" ' + (nt.NoteTypeID == permBanID ? 'checked="checked"' : '') + '>' +
                                                    '<input class="SNNoteTypeDisp" type="text" maxlength="20" value="' + nt.DisplayName + '">' +
                                                    '&nbsp;Color:&nbsp;<input class="SNNoteTypeColor" type="text" maxlength="6" value="' + nt.ColorCode + '">' +
                                                    '<label><input type="checkbox" class="SNntBold" value="bold" ' + (nt.Bold ? 'checked="checked"' : '') + '>Bold</label>' +
                                                    '<label><input type="checkbox" class="SNntItalic" value="italic" ' + (nt.Italic ? 'checked="checked"' : '') + '>Italic</label>' +
                                                    '&nbsp;<span class="SNPreview"></span>' +
                                                    '<a class="SNRemove">x</a>' +
                                                '</li>';
    return ntLI;
}
function resetNoteTypes() {
    $('.SNSubreddit .SNNoteTypes ol').each(function (i, nt) {
        //put this here since it's only called on init and reset, and the display needs updated always then
        $('li', nt).each(function (i, ntli) {
            ntUpdatePreview(ntli);
            var $ntli = $(ntli);
            if ($ntli.attr('SNNoteTypeID') == "-1") {
                $ntli.remove();
            } else {
                $ntli.show();
                $ntli.attr('SNChanged', '');
            }
        });
        $('li', nt).sort(ntSort).appendTo($(nt));
    });

}
function ntSort(a, b) {
    return ($(b).attr("SNNoteTYpeDisplayOrder")) < ($(a).attr("SNNoteTYpeDisplayOrder")) ? 1 : -1;
}
function ntUpdatePreview(ntLI) {
    var $li = $(ntLI);
    var newCSS = "";
    newCSS += "color:#" + $('.SNNoteTypeColor', $li).val() + ";" +
        ($('.SNntBold:checked', $li).length > 0 ? "font-weight:bold;" : "") +
        ($('.SNntItalic:checked', $li).length > 0 ? "font-style:italic;" : "");
    $('span.SNPreview', $li).text($('.SNNoteTypeDisp', $li).val()).attr("style", newCSS);
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
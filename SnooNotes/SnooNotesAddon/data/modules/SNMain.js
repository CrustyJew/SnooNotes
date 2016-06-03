function initSnooNotes() {
    (function (snUtil) {
        

        snUtil.ApiBase = "https://snoonotes.com/api/";
        snUtil.LoginAddress = "https://snoonotes.com/Auth/Login";
        snUtil.RESTApiBase = "https://snoonotes.com/restapi/";
        //snUtil.LoginAddress = "https://localhost:44311/Auth/Login";
        //snUtil.ApiBase = "https://localhost:44311/api/";
        //snUtil.RESTApiBase = "https://localhost:44311/restapi/";
        snUtil.CabalSub = "spamcabal"; //lower case this bad boy

        snUtil.Permissions = {};
        snUtil.Permissions.None = 0x00;
        snUtil.Permissions.Access = 0x01;
        snUtil.Permissions.Config = 0x02;
        snUtil.Permissions.Flair = 0x04;
        snUtil.Permissions.Mail = 0x08;
        snUtil.Permissions.Posts = 0x10;
        snUtil.Permissions.Wiki = 0x20;
        snUtil.Permissions.All = 0x40 | snUtil.Permissions.Access | snUtil.Permissions.Config | snUtil.Permissions.Flair | snUtil.Permissions.Mail | snUtil.Permissions.Posts | snUtil.Permissions.Wiki;
        

        if ($('#SNContainer').length == 0) {
            $('body').append($('<div id="SNContainer"></div>'));
        }
        if ($('#SNModalContainer').length < 1) {
            $('body').append($('<div id="SNModalContainer"></div>'));
        }
        snUtil.NoteStyles = document.createElement('style');
        $('#SNContainer').append(snUtil.NoteStyles);
        snUtil.reinitWorker = function () {
            getSettings();
        }
        snUtil.setUsersWithNotes = function(users){
            if (!users) {
                return;
            }
            snUtil.settings.usersWithNotes = users;

        };
       
        //received data from socket to add/remove a user
        snUtil.updateUsersWithNotes = function (req) {
            var user = req.user;
            if (req.remove) {
                console.log("removed user");
                var i = snUtil.settings.usersWithNotes.indexOf(req.user);
                if (i > -1) {
                    snUtil.settings.usersWithNotes.splice(i, 1);
                }
            }
            else if (req.add) {
                console.log("Added user");
                //snUtil.UsersWithNotes = snUtil.UsersWithNotes + "," + user + ",";
                snUtil.settings.usersWithNotes.push(req.user);
            }
        };
        
        snUtil.getNotesForUsers = function (users) {
            snBrowser.requstUserNotes(users);
        };
        snUtil.reinitAll = function () {
            $('.SNNoNotes,.SNViewNotes').remove();
            $('.SNDone').removeClass('SNDone');
            $('#SNContainer').empty();
            snBrowser.reinitAll();
            getSettings();
        };
        //have to have the snUtil functions ready >.<
        browserInit(); //init browser first to attach listeners etc
        //do this lateish so we get all the listeners hooked up first
        //if (!snUtil.settings.loggedIn) checkLoggedIn();

        getSettings();

        var sub = /reddit\.com\/r\/[a-z0-9\+]*\/?/i.exec(window.location);
        snUtil.Subreddit = !sub ? "" : sub[0].substring(13).replace(/\//g, '');
        snUtil.Subreddit = snUtil.Subreddit.indexOf('+') != -1  ? "" : snUtil.Subreddit; //if it contains a plus sign, it's a multi reddit, not a mod
        
        

        $('#SNModalContainer').on('click', '.SNCloseModal,#SNModalBackground', function (e) {
            snUtil.CloseModal();
        });

        snUtil.ShowModal = function (modalHTML, bindEvents) {
            $('body.res .side .titlebox div[data-res-css]:first').style('z-index', 'auto', 'important').children().style('z-index', 'auto', 'important');
            $('#tb-bottombar').style('z-index', 'auto', 'important');
            $('body').style('overflow', 'hidden', 'important');
            var $modalContainer = $('#SNModalContainer');
            $modalContainer.empty();
            $modalContainer.append($('<div id="SNModalBackground" style="display:none"></div>').show("fast"));
            $modalContainer.append($('<div id="SNModal"><div class="SNHeader"><a class="SNCloseModal SNClose">[x]</a></div>' + modalHTML + '</div>').show("fast"));
            if (bindEvents) {
                bindEvents();
            }
        }
        snUtil.CloseModal = function () {
            $('body.res .side .titlebox div[data-res-css]:first').style('z-index', '2147483646', 'important').children().style('z-index', '2147483646', 'important');
            $('#tb-bottombar').attr('style', '');
            $('body').attr('style', '');
            $('#SNModalContainer').empty();
        }

        snUtil.Modmail = window.location.pathname.match(/\/message\/(?:moderator)\/?/i);
        snUtil.ModQueue = window.location.pathname.match(/\/r\/mod\/about\/modqueue/i);
        snUtil.UserPage = window.location.pathname.match(/\/user\//i);
        return;
    }(snUtil = this.snUtil || {}));
}

function getSettings() {
    chrome.runtime.sendMessage({ "method": "getSettings" }, function (settings) {
        snUtil.settings = settings;
        if (settings) {
            if (settings.loggedIn) {
                window.dispatchEvent(new CustomEvent("snLoggedIn"));

                $('#SNSubDropdown').remove();
                var $select = $('<select id="SNSubDropdown" class="SNNewNoteSub"><option value="-1">--Select a Sub--</option></select>');
                for (var i = 0; i < settings.moddedSubs.length; i++) {
                    if (settings.moddedSubs[i] != snUtil.CabalSub) {
                        $select.append($('<option value="' + settings.moddedSubs[i] + '">' + settings.moddedSubs[i] + '</option>'));
                    }
                    else {
                       
                    }
                }
                $('#SNContainer').append($select);
                if (settings.isCabal) {
                    var cabalTypes = settings.subSettings[snUtil.CabalSub].NoteTypes;
                    var cabalPanel = '<div id="SNCabalTypes" style="display:none;"><ul class="SNNoteType">';
                    for (var x = 0; x < cabalTypes.length; x++) {
                        cabalPanel += '<li class="SN' + snUtil.CabalSub + cabalTypes[x].NoteTypeID + '" sn-cabal-type="' + cabalTypes[x].NoteTypeID + '">' + cabalTypes[x].DisplayName + '</li>';
                    }
                    cabalPanel += '</ul></div>';
                    $('#SNContainer').append($(cabalPanel));
                }
                snUtil.NoteStyles.innerHTML = settings.noteTypeCSS;
                var event = new CustomEvent("snUtilDone");
                window.dispatchEvent(event);
            }
            else {
                window.dispatchEvent(new CustomEvent("snLoggedOut"));
            }
            
        }
    });
}

function handleAjaxError(jqXHR, textStatus, errorThrown) {
    if(jqXHR.status === 401)
    {
        window.dispatchEvent(new CustomEvent("snLoggedOut"));
        //showLoginPopup();
    }
}

(function () {
   
    jQuery.expr[":"].Contains = jQuery.expr.createPseudo(function (arg) {
        return function (elem) {
            return jQuery(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
        };
    });
    initSnooNotes();
    $('div.content').on('DOMNodeInserted', function (e) {
        //copied from mod toolbox in the hopes it will make it compatible with their stuff too.
        var $target = $(e.target), $parentNode = $(e.target.parentNode);
        if (!($target.hasClass("sitetable") && ($target.hasClass("listing") || $target.hasClass("linklisting") ||
            $target.hasClass("modactionlisting"))) && !$parentNode.hasClass('morecomments') && !$target.hasClass('flowwit')) return;

        console.log('snGotNewThings firing from: ' + $target.attr('class'));

        setTimeout(function () {
            var event = new CustomEvent("snGotNewThings");
            window.dispatchEvent(event);
        }, 1750);
    });
})();

(function ($) {
    //BlockUI defaults
    $.blockUI.defaults.growlCSS = {
        "width": "350px",
        "top": "10px",
        "left": "",
        "right": "10px",
        "border": "none",
        "padding": "5px",
        "opacity": 1,
        "cursor": "default",
        "color": "#fff",
        "backgroundColor": "#000",
        "-webkit-border-radius": "10px",
        "-moz-border-radius": "10px",
        "border-radius": "10px",
        "z-index":"999999"
    };
    $.blockUI.defaults.css= { 
        padding:        0, 
        margin:         0, 
        width:          '30%', 
        top:            '40%', 
        left:           '35%', 
        textAlign:      'center', 
        color:          '#000', 
        border:         '1px solid #darkgrey', 
        backgroundColor:'#fff', 
        cursor:         'wait' 
    }



    if ($.fn.style) {
        return;
    }

    // Escape regex chars with \
    var escape = function (text) {
        return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
    };

    // For those who need them (< IE 9), add support for CSS functions
    var isStyleFuncSupported = !!CSSStyleDeclaration.prototype.getPropertyValue;
    if (!isStyleFuncSupported) {
        CSSStyleDeclaration.prototype.getPropertyValue = function (a) {
            return this.getAttribute(a);
        };
        CSSStyleDeclaration.prototype.setProperty = function (styleName, value, priority) {
            this.setAttribute(styleName, value);
            var priority = typeof priority != 'undefined' ? priority : '';
            if (priority != '') {
                // Add priority manually
                var rule = new RegExp(escape(styleName) + '\\s*:\\s*' + escape(value) +
                    '(\\s*;)?', 'gmi');
                this.cssText =
                    this.cssText.replace(rule, styleName + ': ' + value + ' !' + priority + ';');
            }
        };
        CSSStyleDeclaration.prototype.removeProperty = function (a) {
            return this.removeAttribute(a);
        };
        CSSStyleDeclaration.prototype.getPropertyPriority = function (styleName) {
            var rule = new RegExp(escape(styleName) + '\\s*:\\s*[^\\s]*\\s*!important(\\s*;)?',
                'gmi');
            return rule.test(this.cssText) ? 'important' : '';
        }
    }

    // The style function
    $.fn.style = function (styleName, value, priority) {
        // DOM node
        var node = this.get(0);
        // Ensure we have a DOM node
        if (typeof node == 'undefined') {
            return this;
        }
        // CSSStyleDeclaration
        var style = this.get(0).style;
        // Getter/Setter
        if (typeof styleName != 'undefined') {
            if (typeof value != 'undefined') {
                // Set style property
                priority = typeof priority != 'undefined' ? priority : '';
                style.setProperty(styleName, value, priority);
                return this;
            } else {
                // Get style property
                return style.getPropertyValue(styleName);
            }
        } else {
            // Get CSSStyleDeclaration
            return style;
        }
    };
})(jQuery);
function toggleScopes() {
    var config = document.querySelector('#chkScopeConfig');
    var wiki = document.querySelector('#chkScopeWiki');

    if (config.checked || wiki.checked) {
        document.getElementById('AdditionalScopes').classList.remove('hidden');
    }
    else {
        document.getElementById('AdditionalScopes').classList.add('hidden');
    }

    if (config.checked) {
        wiki.setAttribute('disabled', 'true');
        document.querySelectorAll('.addScopeConfig').forEach(function (li) { li.classList.remove('hidden'); });
    }
    else if (wiki.checked) {
        wiki.removeAttribute('disabled');
        document.querySelectorAll('.addScopeConfig:not(.addScopeWiki)').forEach(function (li) { li.classList.add('hidden'); });
        document.querySelectorAll('.addScopeWiki').forEach(function (li) { li.classList.remove('hidden');});
    }
    else {
        wiki.removeAttribute('disabled');
    }
}
(function () {
    document.querySelectorAll('.optScope').forEach(function (e, i) { e.addEventListener('click', toggleScopes)});
    toggleScopes();
})();
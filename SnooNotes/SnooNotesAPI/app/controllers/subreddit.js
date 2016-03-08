angular
       .module('SnooNotes')
       .controller('SubredditCtrl', SubredditCtrl);

function SubredditCtrl($scope, $stateParams, SubFactory, AuthFactory) {
    $scope.import = {};
    $scope.importing = false;
    $scope.imported = false;
    $scope.currentUser = AuthFactory.currentUser;
    var subSettings = SubFactory.getByName($stateParams.subName);
    if (!subSettings) {
        //TODO ERROR
        $scope.sub = { SubName: "ERROR" };
    } else {
        $scope.sub = subSettings;
    }
    for (var i = 0; i < $scope.sub.Settings.NoteTypes.length; i++) {
        var nt = $scope.sub.Settings.NoteTypes[i];
        switch (nt.DisplayName.toLowerCase()) {
            case "none":
                $scope.import.snNone = nt;
                break;
            case "good contributor":
                $scope.import.snGoodUser = nt;
                break;
            case "spam watch":
                $scope.import.snSpamWatch = nt;
                break;
            case "spam warning":
                $scope.import.snSpamWarn = nt;
                break;
            case "abuse warning":
                $scope.import.snAbuseWarn = nt;
                break;
            case "ban":
                $scope.import.snBan = nt;
                break;
            case "perma ban":
                $scope.import.snPermBan = nt;
                break;
            case "bot ban":
                $scope.import.snBotBan = nt;
                break;

        }
    }

    $scope.importNotes = function () {
        var noteMapping = {};
        noteMapping.subName = $scope.sub.SubName;
        for (var propertyName in $scope.import) {
            noteMapping[propertyName] = $scope.import[propertyName].NoteTypeID;
        }
        $scope.importing = true;
        SubFactory.importTBNotes(noteMapping).then(function () { $scope.imported = true; $scope.importing = false; }, function () { $scope.error = true; $scope.importing = false; });
    }
    //$scope.selChange = function () {
    //    this.setAttribute('style', this.options[this.selectedIndex].attributes['style'].value);
    //}
}
angular
       .module('SnooNotes')
       .controller('SubredditCtrl', SubredditCtrl);

function SubredditCtrl($scope, $stateParams, SubFactory) {
    $scope.import = {};
    var subSettings = SubFactory.getByName($stateParams.subName);
    if (!subSettings) {
        //TODO ERROR
        $scope.sub = { subName: "ERROR" };
    } else {
        $scope.sub = subSettings;
    }
    for (var i = 0; i < $scope.sub.Settings.NoteTypes.length; i++) {
        var nt = $scope.sub.Settings.NoteTypes[i];
        switch (nt.DisplayName.toLowerCase()) {
            case "none":
                $scope.import.tbNone = nt.NoteTypeID;
                break;
            case "good contributor":
                $scope.import.tbGoodContributor = nt.NoteTypeID;
                break;
            case "spam watch":
                $scope.import.tbSpamWatch = nt.NoteTypeID;
                break;
            case "spam warning":
                $scope.import.tbSpamWarning = nt.NoteTypeID;
                break;
            case "abuse warning":
                $scope.import.tbAbuseWarning = nt.NoteTypeID;
                break;
            case "ban":
                $scope.import.tbBan = nt.NoteTypeID;
                break;
            case "perma ban":
                $scope.import.tbPermaBan = nt.NoteTypeID;
                break;
            case "bot ban":
                $scope.import.tbBotBan = nt.NoteTypeID;
                break;

        }
    }
    $scope.selChange = function () {
        this.setAttribute('style', this.options[this.selectedIndex].attributes['style'].value);
    }
}
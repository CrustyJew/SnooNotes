angular
       .module('SnooNotes')
       .controller('SubredditCtrl', SubredditCtrl);

function SubredditCtrl($scope,$stateParams, SubFactory){
    var subSettings = SubFactory.getByName($stateParams.subName);
    if (!subSettings) {
        //TODO ERROR
        $scope.sub = { subName: "ERROR" };
    } else {
        $scope.sub = subSettings;
    }
}
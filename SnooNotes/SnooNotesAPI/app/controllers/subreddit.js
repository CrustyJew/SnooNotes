module.exports = function ($scope,$state, $stateParams) {
    $scope.subName = $stateParams.subName;
    //if ($state.current.name == "subreddit") $state.go("subreddit.settings", {}, { location: false });
}
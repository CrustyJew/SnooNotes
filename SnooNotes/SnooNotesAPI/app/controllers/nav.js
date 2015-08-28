angular
       .module('SnooNotes')
       .controller('NavCtrl', NavCtrl);

function NavCtrl($scope, AuthFactory, $modal, SubFactory,$location,$stateParams,$state) {
    $scope.currentUser = AuthFactory.currentUser;
    $scope.curSub = $stateParams.subName;

    $scope.$on("$stateChangeSuccess", function () {
        $scope.curSub = $stateParams.subName || "Subreddits";
    });
    //if ($location.path().substring(1, 10).toLowerCase() === 'subreddit' && $location.path().substring(11)) {
    //    $scope.curSub = $location.path().substring(11);
    //}
    SubFactory.initialized.then(function(){
        $scope.adminSubs = SubFactory.adminSubNames();
    });

    $scope.logout = function () {
        AuthFactory.logout();
    }
    $scope.login = function () {
        var loginModal = $modal.open({
            templateUrl: "loginModal.html",
            controller: 'AuthCtrl'
        });
    }
}
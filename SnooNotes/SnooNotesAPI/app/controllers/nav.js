module.exports = /*@ngInject*/ function ($scope, AuthFactory, $uibModal, SubFactory, $location, $stateParams, $state) {
    $scope.currentUser = AuthFactory.currentUser;
    $scope.curSub = $stateParams.subName;

    $scope.adminSubs = SubFactory.adminSubNames;
    $scope.$on("$stateChangeSuccess", function () {
        $scope.curSub = $stateParams.subName || "Subreddits";
    });

    $scope.logout = function () {
        AuthFactory.logout();
    }
    $scope.login = function () {
        var loginModal = $uibModal.open({
            templateUrl: "loginModal.html",
            controller: 'AuthCtrl'
        });
    }
}
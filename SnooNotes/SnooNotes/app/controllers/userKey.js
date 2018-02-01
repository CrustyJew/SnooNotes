module.exports = /*@ngInject*/ function ($scope, $stateParams, $http, AuthFactory) {
    $scope.loggedIn = AuthFactory.currentUser.isAuth;
    $scope.generating = false;
    $scope.error = false;
    $scope.done = false;

    $scope.genKey = function () {
        $scope.generating = true;
        $scope.error = false;
        $http.post('api/Account/ResetAuthCode').then(function () {
            $scope.generating = false;
            $scope.error = false;
            $scope.done = true;
        }, function () {
            $scope.error = true;
            $scope.done = false;
            $scope.generating = false;
        });
    }
}
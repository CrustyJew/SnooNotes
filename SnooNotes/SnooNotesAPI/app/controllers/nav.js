angular
       .module('SnooNotes')
       .controller('NavCtrl', NavCtrl);

function NavCtrl($scope, AuthFactory, $modal) {
    $scope.currentUser = AuthFactory.currentUser;
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
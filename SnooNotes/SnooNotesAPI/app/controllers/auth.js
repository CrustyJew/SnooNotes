angular
       .module('SnooNotes')
       .controller('AuthCtrl', AuthCtrl);

function AuthCtrl($scope, AuthFactory) {
    $scope.currentUser = AuthFactory.currentUser;
}
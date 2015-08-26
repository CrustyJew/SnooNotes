    angular
        .module('SnooNotes')
        .controller('HomeCtrl', HomeCtrl);

    function HomeCtrl($scope, AuthFactory) {
        AuthFactory.isLoggedIn().then(function(d){
            $scope.loggedIn = d;
            console.log($scope.loggedIn);
        });
    }

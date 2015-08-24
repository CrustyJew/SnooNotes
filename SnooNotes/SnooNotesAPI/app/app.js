
angular.module('SnooNotes', [
    'ui.router'
]).config(function ($stateProvider,$urlRouterProvider) {
    //'use strict';
    $urlRouterProvider
        .otherwise('/');
    $stateProvider
        .state('home', {
            url: '/',
            //template:"<h1>aksdlfj</h1>"
            templateUrl: "/Views/home.html",
            controller: 'HomeCtrl',
            data: {
                requireLogin: false
            }
        });

    
})
.run(function($rootScope){
    $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
        var requireLogin = toState.data.requireLogin;

        if (requireLogin && typeof $rootScope.currentUser === 'undefined') {
            event.preventDefault();
        }
    });
});

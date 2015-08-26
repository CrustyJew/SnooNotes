
angular.module('SnooNotes', [
    'ui.router',
    'ngCookies',
    'LocalStorageModule'
]).config(function ($stateProvider,$urlRouterProvider, $httpProvider) {
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
    $httpProvider.defaults.withCredentials = true;
    
})
.run(function ($rootScope,AuthFactory) {
    AuthFactory.getCurrentUser();
    $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
        var requireLogin = toState.data.requireLogin;

        if (requireLogin && typeof $rootScope.currentUser === 'undefined') {
            event.preventDefault();
        }
    });
});

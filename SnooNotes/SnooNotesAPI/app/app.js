
angular.module('SnooNotes', [
    'ui.router',
    'ngCookies',
    'LocalStorageModule',
    'ui.bootstrap'
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
        })
        .state('subreddit',{
            url: '/subreddit/:subName',
            templateUrl: "/Views/subreddit.html",
            controller: 'SubredditCtrl',
            data: {
                requireLogin: true
            },
            resolve: {
                SubFactoryInit: function (SubFactory) {
                    return SubFactory.initialized;
                }
            }
        })
    ;
    $httpProvider.defaults.withCredentials = true;
    
})
.run(function ($rootScope,AuthFactory,SubFactory) {
    AuthFactory.getCurrentUser();
    SubFactory.getSubsWithAdmin();
    $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
        var requireLogin = toState.data.requireLogin;

        if (requireLogin && !AuthFactory.currentUser.isAuth) {
            event.preventDefault();
        }
    });
});

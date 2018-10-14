
require('datatables.net')(window,$);
require("angular");
require("angular-local-storage");
require("angular-cookies");
require("angular-ui-router");
require("angular-ui-bootstrap");
require("angular-datatables");
var app = angular.module('SnooNotes', [
    'ui.router',
    'ngCookies',
    'LocalStorageModule',
    'ui.bootstrap',
    'datatables'
]);

require("./controllers");
require("./factories");
require("./directives");
    app.config(function ($stateProvider, $urlRouterProvider, $httpProvider) {
    //'use strict';
    $urlRouterProvider
        .otherwise('/');
    $stateProvider
        .state('home', {
            url: '/',
            //template:"<h1>aksdlfj</h1>"
            templateUrl: '/Views/home.html',
            controller: 'HomeCtrl',
            data: {
                requireLogin: false
            }
        })
        .state('subreddit', {
            url: '/subreddit/:subName',
            templateUrl: '/Views/subreddit.html',
            controller: 'SubredditCtrl',
            redirectTo: 'subreddit.settings',
            //abstract:true,
            data: {
                requireLogin: true
            },
            resolve: {
                SubFactoryInit: function (SubFactory) {
                    return SubFactory.initialized;
                }
            }

        })
        .state('subreddit.settings', {
            url: '/settings',
            templateUrl: '/Views/subredditSettings.html',
            controller: 'SubredditSettingsCtrl',
            data: {
            }
        })
        .state('subreddit.banned', {
            url: '/banned',
            templateUrl: '/Views/bannedEntities.html',
            controller: 'BannedEntitiesCtrl'
        })
        //.state('subreddit.default', {
        //    url: '',
        //    templateUrl: '/Views/subredditSettings.html',
        //    controller: 'SubredditSettingsCtrl'
        //})
        .state('userguide', {
            url: '/userguide',
            templateUrl: '/Views/userguide.html',
            data: {
                requireLogin: false
            }
        })
        .state('userkey', {
            url: '/userkey',
            templateUrl: '/Views/userkey.html',
            controller: 'UserKeyCtrl',
            data: {
                requireLogin: false
            }
        })
    ;
        $httpProvider.defaults.withCredentials = true;
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';

})
.run(function ($rootScope, AuthFactory, SubFactory, $uibModal, $state) {
    AuthFactory.getCurrentUser();
    SubFactory.getSubsWithAdmin();
    $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
        var requireLogin = toState.data.requireLogin;

        if (requireLogin && !AuthFactory.currentUser.isAuth) {
            event.preventDefault();

            $rootScope.redirectScope = toState;
            $rootScope.redirectParams = toParams;

            $uibModal.open({
                templateUrl: "loginModal.html",
                controller: 'AuthCtrl'
            });
            
        }
    });

    $rootScope.$on('$stateChangeSuccess', function (evt, to, params) {
        if (to.redirectTo) {
            evt.preventDefault();
            $state.go(to.redirectTo, params, { location: false });
        }
    });
});


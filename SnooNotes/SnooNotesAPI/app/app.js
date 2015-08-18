
angular.module('SnooNotes', [
    'ngRoute'
]).config(function ($routeProvider) {
    'use strict';
    $routeProvider
        .when('/', {
            templateUrl: 'Views/home.html',
            controller: 'HomeCtrl',
            controllerAs: 'home'
        })
        .otherwise({
            redirectTo: '/'
        });
});

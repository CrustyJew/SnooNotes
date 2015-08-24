angular.module('SnooNotes')
    .factory('AuthFactory', function AuthFactory($q, $http, $location) {
        'use strict';
        var exports = {};

        exports.isLoggedIn = function () {
            return $http.get('api/Account/IsLoggedIn')
                .then(
                    function () {
                        return true;
                    },
                    function () {
                        return false;
                    });
        };

        return exports;
    });
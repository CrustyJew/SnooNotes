module.exports = /*@ngInject*/ function AuthFactory($q, $http, $window, localStorageService, $cookies) {
    'use strict';
    var exports = {};

    exports.isLoggedIn = function () {
        var deferred = $q.defer();
        $http.get('api/Account/IsLoggedIn')
            .then(
                function () {
                    //return true;
                    deferred.resolve(true);
                },
                function () {
                    //return false;
                    deferred.resolve(false);
                });
        return deferred.promise;
    };

    exports.getCurrentUser = function () {
        var deferred = $q.defer();
        if (exports.currentUser.isAuth) {
            deferred.resolve(exports.currentUser);
        }
        else {
            var curUser = localStorageService.get('currentUser');
            if (curUser && curUser.isAuth) {
                exports.currentUser = curUser;
                deferred.resolve(exports.currentUser);
            }
            else {
                $http.get('api/Account/GetCurrentUser')
                    .then(
                        function (u) {

                            exports.currentUser.userName = u.data.UserName;
                            exports.currentUser.hasConfig = u.data.HasConfig;
                            exports.currentUser.hasWiki = u.data.HasWiki;
                            exports.currentUser.isAuth = true;
                            localStorageService.set('currentUser', exports.currentUser);
                            deferred.resolve(exports.currentUser);
                        },
                        function () {
                            exports.logout(true);
                            deferred.reject('Not logged in');
                        });
            }
        }
        return deferred.promise;
    }
    exports.logout = function (suppressRedirect) {
        
            localStorageService.remove('currentUser');
            exports.currentUser.userName = "";
            exports.currentUser.hasConfig = false;
            exports.currentUser.hasWiki = false;
            exports.currentUser.isAuth = false;
            if (!suppressRedirect) {
                $window.location.href = "/Signout";
            }

        //$cookies.remove('bog');
    }
    exports.currentUser = { isAuth: false, userName: "" };
    return exports;
}
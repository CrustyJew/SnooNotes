module.exports = /*@ngInject*/ function AuthFactory($q, $http, $window, $cookies) {
    'use strict';
    var exports = {};

    exports.isLoggedIn = function () {
        var deferred = $q.defer();
        $http.get('site/Account/IsLoggedIn')
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
            var curUser = $window.sessionStorage.getItem('currentUser');
            if (curUser) {
                curUser = angular.fromJson(curUser);
            }
            if (curUser && curUser.isAuth) {
                exports.currentUser = curUser;
                deferred.resolve(exports.currentUser);
            }
            else {
                $http.get('site/Account/GetCurrentUser')
                    .then(
                        function (u) {

                            exports.currentUser.userName = u.data.UserName;
                            exports.currentUser.hasConfig = u.data.HasConfig;
                            exports.currentUser.hasWiki = u.data.HasWiki;
                            exports.currentUser.isAuth = true;
                            $window.sessionStorage.setItem('currentUser', JSON.stringify(exports.currentUser));
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
        
        $window.sessionStorage.removeItem('currentUser');
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
module.exports = function ($q, $http, $rootScope) {
    var exports = {};

    exports.testConnection = function (settings, sub) {
        var deferred = $q.defer();
        $http.post("/api/Dirtbag/" + sub + "/TestConnection", JSON.stringify(settings))
            .then(
                function (d) {
                    deferred.resolve(d.data);
                }
            , function (e) {
                deferred.reject(e.status + ": " + e.data.Message);
            });
        return deferred.promise;
    }

    exports.saveSettings = function (settings, sub) {
        var deferred = $q.defer();
        $http.put("/api/Dirtbag/" + sub, JSON.stringify(settings))
            .then(
                function (d) {
                    deferred.resolve(d.data);
                },
                function (e) {
                    deferred.reject(e);
                });
        return deferred.promise;
    }

    exports.getBanList = function (sub) {
        var deferred = $q.defer();
        $http.get("/api/Dirtbag/" + sub + "/BanList")
            .then(
                function (d) {
                    deferred.resolve(d.data);
                }, function (e) {
                    deferred.reject(e);
                });
        return deferred.promise;
    }

    exports.removeBan = function (sub, id) {
        var deferred = $q.defer();
        $http.delete("/api/Dirtbag/" + sub + "/BanList/" + id)
            .then(
                function (d) {
                    deferred.resolve(d.data);
                },
                function (e) {
                    deferred.reject(e);
                }
            );
        return deferred.promise;

    }

    exports.updateBanReason = function (sub, id, reason) {
        var deferred = $q.defer();
        $http.put("/api/Dirtbag/" + sub + "/BanList/" + id, '"' + reason + '"')
            .then(
                function (d) {
                    deferred.resolve(true);
                }, function (e) {
                    deferred.reject(e);
                }
            );
        return deferred.promise;
    }
    return exports;
};
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

    return exports;
};
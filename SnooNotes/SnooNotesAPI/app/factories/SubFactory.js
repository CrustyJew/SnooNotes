angular.module('SnooNotes')
    .factory('SubFactory', function ($q, $http, $rootScope) {
        var exports = {};
        var _initialized = $q.defer();
        var _adminSubs = [];
        var _adminSubNames = [];
        exports.getSubsWithAdmin = function () {
            var deferred = $q.defer();

            $http.get('restapi/Subreddit?id=admin').then(
                function (d) {
                    _adminSubs = d.data;
                    _adminSubNames = _adminSubs.map(function (sub) {
                        return sub.SubName;
                    });
                    exports.adminSubNames.subNames = _adminSubNames;
                    exports.adminSubDetails.subs = _adminSubs;
                    deferred.resolve(_adminSubs);
                    _initialized.resolve(true);
                },
                function (e) {
                    deferred.reject(e);
                    //_initialized.reject(e);
                });

            return deferred.promise;
        }

        exports.getByName = function (name) {
            for (var i = 0; i < _adminSubs.length; i++) {
                if (_adminSubs[i].SubName.toLowerCase() == name.toLowerCase()) {
                    return _adminSubs[i];
                }
            }
            return {};
        }

        exports.adminSubNames = { subNames: _adminSubNames };

        exports.adminSubDetails = { subs: _adminSubs };

        exports.initialized = _initialized.promise;

        exports.importTBNotes = function (noteMapping) {

            var deferred = $q.defer();

            $http.post('restapi/ToolBoxNotes', JSON.stringify(noteMapping)).
                then(function (response) {
                    deferred.resolve(response.data);
                }, function (response) {
                    deferred.reject(response);
                });

            return deferred.promise;
        }
        return exports;
    });
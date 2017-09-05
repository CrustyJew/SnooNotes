module.exports = /*@ngInject*/ function ($scope, $stateParams, SubFactory, AuthFactory, DirtbagFactory) {
    $scope.import = {};
    $scope.importing = false;
    $scope.imported = false;
    $scope.loading = true;
    $scope.exporting = false;
    $scope.importingStatus = "Initializing..";

    $scope.tbNoteTypes = [];

    SubFactory.getTBWarningKeys($stateParams.subName).then(function (keys) {
        $scope.tbNoteTypes = keys;
        $scope.loading = false;
        $scope.import = {};
        for (var i = 0; i < $scope.sub.Settings.NoteTypes.length; i++) {
            var nt = $scope.sub.Settings.NoteTypes[i];
            var ntn = nt.DisplayName.toLowerCase().split(' ').join('');
            if ($scope.tbNoteTypes.indexOf(ntn) > -1) {
                $scope.import[ntn] = nt;
            }
        }
    }, function (err) {
        $scope.tbNoteTypes = [];
        $scope.loading = false;
        $scope.import = {};
    });

    $scope.currentUser = AuthFactory.currentUser;
    var subSettings = SubFactory.getByName($stateParams.subName);
    if (!subSettings) {
        //TODO ERROR
        $scope.sub = { SubName: "ERROR" };
    } else {
        $scope.sub = subSettings;
        $scope.sub.oldBotSettings = angular.copy(subSettings.BotSettings);
    }


    $scope.importNotes = function () {
        var noteMapping = {};
        //noteMapping.subName = $scope.sub.SubName;
        for (var propertyName in $scope.import) {
            noteMapping[propertyName] = $scope.import[propertyName].NoteTypeID;
        }
        $scope.importing = true;
        SubFactory.importTBNotes(noteMapping, $scope.sub.SubName).then(
            function () {
                $scope.checkTBImport();
                //$scope.imported = true; $scope.importing = false;

        }, function () { $scope.error = true; $scope.importing = false; });
    }
    $scope.checkTBImport = function () {
        SubFactory.checkImportTBNotes($scope.sub.SubName).then(
            function (status) {
                $scope.importing = !status.done;
                $scope.importingStatus = status.msg;
                if (!status.done) {
                    window.setTimeout($scope.checkTBImport, 2000);
                }
            }, function (err) {
                $scope.importingStatus = err;
            });
    }

    $scope.checkTBImport();
    $scope.exportNotes = function () {
        $scope.exporting = true;
        SubFactory.exportNotes($scope.sub.SubName).
            then(function (data) {
                $scope.exporting = false;
                data = JSON.stringify(data, undefined, 2);
                var blob = new Blob([data], { type: 'text/json' });
                var date = new Date();
                var filename = 'SnooNotesExport_' + $scope.sub.SubName + '_'+  date.getFullYear() + '-' + date.getMonth() + '-' + date.getDate() + '.json';
                // FOR IE:

                if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                    window.navigator.msSaveOrOpenBlob(blob, filename);
                }
                else {
                    var e = document.createEvent('MouseEvents'),
                        a = document.createElement('a');

                    a.download = filename;
                    a.href = window.URL.createObjectURL(blob);
                    a.dataset.downloadurl = ['text/json', a.download, a.href].join(':');
                    e.initEvent('click', true, false, window,
                        0, 0, 0, 0, 0, false, false, false, false, 0, null);
                    a.dispatchEvent(e);
                }
            }, function (error) { $scope.exporting = false });
    }

    $scope.updateSub = function () {
        $scope.updating = true;
        $scope.dirtbagMessage = "";
        DirtbagFactory.saveSettings($scope.sub.BotSettings, $scope.sub.SubName).
            then(function (d) {
                $scope.updating = false;
                $scope.sub.BotSettings = angular.copy(d);
                $scope.sub.oldBotSettings = angular.copy(d);
                $scope.frmBotIntegration.$setPristine();
                $scope.dirtbagMessage = "Saved settings!";
                $scope.successMessage = true;
            }, function (e) {
                $scope.updating = false;
                $scope.dirtbagMessage = e;
                $scope.successMessage = false;
            });

    }

    $scope.reset = function () {
        $scope.sub.BotSettings = angular.copy($scope.sub.oldBotSettings);
        $scope.frmBotIntegration.$setPristine();
    }

    $scope.testDirtbag = function () {
        DirtbagFactory.testConnection($scope.sub.BotSettings, $scope.sub.SubName)
            .then(function (success) {
                $scope.successMessage = true; $scope.dirtbagMessage = "Test Succeeded!"
            }, function (e) {
                $scope.successMessage = false; $scope.dirtbagMessage = e;
            })
    }

    $scope.urlChanged= function(){
        return $scope.sub.BotSettings.DirtbagUrl != $scope.sub.oldBotSettings.DirtbagUrl;
    }
    
    //$scope.selChange = function () {
    //    this.setAttribute('style', this.options[this.selectedIndex].attributes['style'].value);
    //}
}
module.exports = function ($scope, AuthFactory, $uibModalInstance, $cookies,SubFactory, $state, $rootScope) {
    $scope.currentUser = AuthFactory.currentUser;
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.prefs = $cookies.getObject('snPrefs') || {wiki:false,read:false};
    $scope.openLoginWindow = function () {
        if ($scope.prefs.wiki == undefined) $scope.prefs.wiki = false;
        if ($scope.prefs.read == undefined) $scope.prefs.read = false;
        var d = new Date();
        d.setTime(d.getTime() + (10000 * 24 * 60 * 60 * 1000));
        $cookies.putObject('snPrefs', $scope.prefs,{expires: d.toGMTString()} );
        var oauthwin = window.open('/Auth/DoLogin?' + $.param($scope.prefs), 'SnooLogin', 'height=850px,width=850px');
        oauthwin.focus();
        setTimeout(function () { CheckLogin(oauthwin) }, 1500);
    }

    function CheckLogin(win) {
        if (win == null || win.closed) {
            $uibModalInstance.close();
            AuthFactory.getCurrentUser()
                .then(function(){
                    SubFactory.getSubsWithAdmin()
                    .then(function () {
                        var scope = $rootScope.redirectScope;
                        var params = $rootScope.redirectParams;
                        $rootScope.redirectScope = undefined;
                        $rootScope.redirectParams = undefined;
                        $state.go(scope, params , { reload: true });
                    });
                });
        }
        else{
            setTimeout(function () { CheckLogin(win) }, 1500);
        }
    }
}
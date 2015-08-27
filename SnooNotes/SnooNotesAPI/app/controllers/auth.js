angular
       .module('SnooNotes')
       .controller('AuthCtrl', AuthCtrl);

function AuthCtrl($scope, AuthFactory, $modalInstance) {
    $scope.currentUser = AuthFactory.currentUser;
    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    }
    $scope.openLoginWindow = function () {
        var oauthwin = window.open('/Auth/DoLogin', 'SnooLogin', 'height=850px,width=850px');
        oauthwin.focus();
        setTimeout(function () { CheckLogin(oauthwin) }, 1500);
    }

    function CheckLogin(win) {
        if (win == null || win.closed) {
            $modalInstance.close();
            AuthFactory.getCurrentUser();
        }
        else{
            setTimeout(function () { CheckLogin(win) }, 1500);
        }
    }
}
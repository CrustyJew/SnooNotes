module.exports = function (DirtbagFactory, $scope, $filter, $compile, DTOptionsBuilder, DTColumnBuilder) {

    $scope.entities = {};
    $scope.dtInstance = {};

    $scope.dtOptions = DTOptionsBuilder.fromFnPromise(getBanList)
        .withPaginationType('full_numbers')
        .withOption('createdRow', createdRow);
    $scope.dtColumns = [
        DTColumnBuilder.newColumn('ID').notVisible(),
        DTColumnBuilder.newColumn('EntityString').withTitle('Entity (Author or channelid)'),
        DTColumnBuilder.newColumn('Type').withTitle('Type'),
        DTColumnBuilder.newColumn('BannedBy').withTitle('Banned By'),
        DTColumnBuilder.newColumn(null).withTitle('Ban Reason').renderWith(editReasonHtml),
        DTColumnBuilder.newColumn('BanDate').withTitle('Ban Date').renderWith(formatDate),
        DTColumnBuilder.newColumn('ThingID').withTitle('Thing ID'),
        DTColumnBuilder.newColumn(null).withTitle('Actions').notSortable().renderWith(actionsHtml)
    ];
    
    $scope.delete = function (entity) {
        var x = $scope.entities[entity];
        x.deleting = true;
        //alert(x.ID + ": " + x.EntityString);
        DirtbagFactory.removeBan($scope.subName, x.ID)
        .finally(function () {
            $scope.dtInstance.reloadData();
        })
    }

    $scope.cancelEdit = function (entity) {
        var x = $scope.entities[entity];
        x.editing = false;
        x.newReason = x.BanReason;
    }

    $scope.saveEdit = function (entity) {
        $scope.errMessages = "";
        var ent = $scope.entities[entity];
        DirtbagFactory.updateBanReason($scope.subName, ent.ID, ent.newReason)
            .then(function (d) {
                $scope.dtInstance.reloadData();
            }, function (e) {
                $scope.errMessages = e;
            });
    }

    function createdRow(row, data, dataIndex) {
        $compile(angular.element(row).contents())($scope);
    }

    function actionsHtml(data, type, full, meta) {
        $scope.entities[data.ID] = data;
        data.newReason = data.BanReason;
        return '<button class="btn btn-warning" ng-click="entities[' + data.ID + '].editing=true;" ng-disabled="entities[' + data.ID + '].deleting" ng-hide="entities[' + data.ID + '].editing"><span class="glyphicon glyphicon-edit"></span></button>' +
            '<button class="btn btn-success" ng-click="saveEdit(' + data.ID + ')" ng-show="entities[' + data.ID + '].editing"><span class="glyphicon glyphicon-ok-sign"></span></button>' +
            '<button class="btn" ng-click="cancelEdit(' + data.ID + ')" ng-show="entities[' + data.ID + '].editing"><span class="glyphicon glyphicon-remove-sign"></span></button>' +
            '<button class="btn btn-danger" ng-disabled="entities['+data.ID+'].deleting" ng-click="delete(' + data.ID + ')"><span class="glyphicon glyphicon-trash"></span></button>';
    }

    function editReasonHtml(data, type, full, meta) {
        return '<span ng-hide="entities[' + data.ID + '].editing">'+data.BanReason+'</span>'+
            '<textarea ng-show="entities[' + data.ID + '].editing" ng-model="entities[' + data.ID + '].newReason"></textarea>'
    }
    function getBanList() {
        return DirtbagFactory.getBanList($scope.subName);
    }
    function formatDate(data, type, full, meta) {
        var d = new Date(data);
        return '<span title="' + d.toUTCString() + '">' + $filter('date')(d, 'short') + '</span>';
    }
}
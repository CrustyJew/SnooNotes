module.exports = function (DirtbagFactory, $scope, $compile, DTOptionsBuilder, DTColumnBuilder) {

    var entities = {};
    $scope.dtInstance = {};

    $scope.dtOptions = DTOptionsBuilder.fromFnPromise(getBanList)
        .withPaginationType('full_numbers')
        .withOption('createdRow', createdRow);
    $scope.dtColumns = [
        DTColumnBuilder.newColumn('ID').notVisible(),
        DTColumnBuilder.newColumn('EntityString').withTitle('Entity (Author or channelid)'),
        DTColumnBuilder.newColumn('BannedBy').withTitle('Banned By'),
        DTColumnBuilder.newColumn('BanReason').withTitle('Ban Reason'),
        DTColumnBuilder.newColumn('BanDate').withTitle('Ban Date'),
        DTColumnBuilder.newColumn('ThingID').withTitle('Thing ID'),
        DTColumnBuilder.newColumn(null).withTitle('Actions').notSortable().renderWith(actionsHtml)
    ];
    
    $scope.delete = function (entity) {
        var x = entities[entity];
        //alert(x.ID + ": " + x.EntityString);
        DirtbagFactory.removeBan($scope.subName, x.ID)
        .finally(function () {
            $scope.dtInstance.reloadData();
        })
    }
    function createdRow(row, data, dataIndex) {
        $compile(angular.element(row).contents())($scope);
    }

    function actionsHtml(data, type, full, meta) {
        entities[data.ID] = data;
        return '<button class="btn btn-danger" ng-click="delete(' + data.ID + ')"><span class="glyphicon glyphicon-trash"></span></button>';
    }

    function getBanList() {
        return DirtbagFactory.getBanList($scope.subName);
    }
}
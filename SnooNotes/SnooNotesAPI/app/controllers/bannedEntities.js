module.exports = function (DirtbagFactory, $scope,DTOptionsBuilder, DTColumnBuilder) {
    $scope.dtOptions = DTOptionsBuilder.fromFnPromise(DirtbagFactory.getBanList()).withPaginationType('full_numbers');
    $scope.dtColumns = [
        DTColumnBuilder.newColumn('entityString').withTitle('Entity (Author or channelid)'),
        DTColumnBuilder.newColumn('bannedBy').withTitle('Banned By'),
        DTColumnBuilder.newColumn('banReason').withTitle('Ban Reason'),
        DTColumnBuilder.newColumn('banDate').withTitle('Ban Date'),
        DTColumnBuilder.newColumn('thingID').withTitle('Thing ID')
    ];
}
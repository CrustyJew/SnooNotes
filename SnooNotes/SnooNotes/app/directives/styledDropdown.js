module.exports = /*@ngInject*/ function ($parse) {
    return {
        require: 'select',
        restrict: 'A',
        link: function (scope, elem, attrs, ngSelect) {
            var optionsSourceStr = attrs.ngOptions.split(' ').pop(),
                getOptionsStyle = $parse(attrs.optionsStyle);

            scope.$watch(optionsSourceStr, function (items) {
                angular.forEach(items, function (item, index) {
                    var css = getOptionsStyle(item),
                        option = elem.find('option[value="' + item.$$hashKey + '"]');
                    option.css(css);
                });
            });

            scope.$watch(attrs.ngModel, function (item) {
                if (!item) {
                    elem.attr('style', elem.find('option[value=""]').attr('style'));
                } else {
                    elem.css(getOptionsStyle(item));
                }
            });
        }
    }
}

angular.module('SnooNotes', [
    'ui.router',
    'ngCookies',
    'LocalStorageModule',
    'ui.bootstrap'
]).config(function ($stateProvider, $urlRouterProvider, $httpProvider) {
    //'use strict';
    $urlRouterProvider
        .otherwise('/');
    $stateProvider
        .state('home', {
            url: '/',
            //template:"<h1>aksdlfj</h1>"
            templateUrl: "/Views/home.html",
            controller: 'HomeCtrl',
            data: {
                requireLogin: false
            }
        })
        .state('subreddit', {
            url: '/subreddit/:subName',
            templateUrl: "/Views/subreddit.html",
            controller: 'SubredditCtrl',
            data: {
                requireLogin: true
            },
            resolve: {
                SubFactoryInit: function (SubFactory) {
                    return SubFactory.initialized;
                }
            }
        })
    ;
    $httpProvider.defaults.withCredentials = true;

})
.run(function ($rootScope, AuthFactory, SubFactory) {
    AuthFactory.getCurrentUser();
    SubFactory.getSubsWithAdmin();
    $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
        var requireLogin = toState.data.requireLogin;

        if (requireLogin && !AuthFactory.currentUser.isAuth) {
            event.preventDefault();
        }
    });
});
angular
       .module('SnooNotes').directive('useSelectedStyle', function () {
           'use strict';
           function link(scope, element, attrs) {
               console.log(attrs.ngModel);
               //scope.select = select;
               scope.scope = scope;
               if (scope.$last) {
                   element.setAttribute('style', 'color:purple;');
               }
               scope.$on("event:repeat-done", function () {
                   console.log(attrs.options);
                   var selected = element.children(':selected');
                   if (selected && selected.length > 0) {
                       var style = selected[0].attributes['style'];
                       if (style) {
                           element.attr('style', style.value);
                       }
                   }
                   //if (element) {
                   //    element.setAttribute('style', element.options[element.selectedIndex].attributes['style'].value);
                   //}
               });
               element.bind("change", function () {
                   if (this.options) {
                       this.setAttribute('style', this.options[this.selectedIndex].attributes['style'].value);
                   }
               });

           }

           return {
               require: 'ngModel',
               restrict: 'A',
               scope: { model: '=ngModel' },
               link: link
           };
       })
    .directive('ngRepeat', function () {
        'use strict';
        return {
            restrict: 'A',
            link: function ($scope, $elem, $attrs) {
                if ($scope.$last) {
                    $scope.$parent.$broadcast('event:repeat-done', $elem);
                }
            }
        };
    })
.directive('styledDropdown', function ($parse) {
    return {
        require: 'select',
        link: function (scope, elem, attrs, ngSelect) {
            var optionsSourceStr = attrs.ngOptions.split(' ').pop(),
                getOptionsStyle = $parse(attrs.optionsClass);

            scope.$watch(optionsSourceStr, function (items) {
                angular.forEach(items, function (item, index) {
                    var css = getOptionsStyle(item),
                        option = elem.find('option[value=' + index + ']');
                    //angular.element(option).attr('style', css);
                    angular.forEach(css, function (add, className) {
                        if (add) {
                            angular.element(option).addClass(className);
                        }
                    });
                });
            });
        }
    }
});

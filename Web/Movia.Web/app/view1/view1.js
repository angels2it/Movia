'use strict';

angular.module('myApp.view1', ['ngRoute'])

    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/view1', {
            templateUrl: '/app/view1/view1.html',
            controller: 'View1Ctrl'
        });
    }])

    .controller('View1Ctrl', ['$scope', '$firebaseArray', 'NgMap', '$interval', '$timeout', function ($scope, $firebaseArray, NgMap, $interval, $timeout) {
        $scope.lat = 55.6585187, $scope.lng = 12.4897214;
        // See https://firebase.google.com/docs/web/setup#project_setup for how to
        // auto-generate this config
        var config = {
            databaseURL: "https://movia-99235.firebaseio.com/"
        };

        firebase.initializeApp(config);

        NgMap.getMap().then(function (map) {
        });


        var ref = firebase.database().ref().child("Users");
        // create a synchronized array
        // click on `index.html` above to see it used in the DOM!
        $scope.users = [];
        $interval(function () {
            console.log('reload users list');
            $timeout(function() {
                ref = firebase.database().ref().child("Users");
            });
        }, 1 * 60 * 1000);
        ref.on("child_added", function (child) {
            processChild(child.val());
        });

        function processChild(user) {
            if (user.Position == null)
                return;
            var isShowOnMap = moment().isBefore(moment(user.Position.UpdatedAt).add(3, 'm'));
            var localUser = _.find($scope.users,
                function (u) {
                    return u.Id === user.Id;
                });
            if (localUser == null) {
                if (isShowOnMap)
                    $timeout(function() {
                        $scope.users.push(user);
                    });
                return;
            }
            if (isShowOnMap)
                $timeout(function() {
                    localUser.Position = user.Position;
                });
            else
                $timeout(function() {
                    _.remove($scope.users,
                        function(u) {
                            return u.Id === user.Id;
                        });
                });
        }
        ref.on("child_changed", function (childSnapshot) {
            processChild(childSnapshot.val());
        });
    }]);
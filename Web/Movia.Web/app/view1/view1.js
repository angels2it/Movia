'use strict';

angular.module('myApp.view1', ['ngRoute'])

    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/view1', {
            templateUrl: '/app/view1/view1.html',
            controller: 'View1Ctrl'
        });
    }])

    .controller('View1Ctrl', ['$scope', '$firebaseArray', 'NgMap', function ($scope, $firebaseArray, NgMap) {
        $scope.lat = 40.74, $scope.lng = -74.18;
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
        var firebaseUsers = $firebaseArray(ref);
        var isCenterMap = false;
        var centerWatch = firebaseUsers.$watch(function (value) {
            if (!isCenterMap && $scope.users.length > 0) {
                isCenterMap = true;
                var center = $scope.users[0].Position;
                $scope.lat = center.Latitude;
                $scope.lng = center.Longitude;
                centerWatch();
            }
        });
        ref.on("child_added", function (child) {
            processChild(child.val());
        });

        function processChild(user) {
            var localUser = _.find($scope.users,
                function (u) {
                    return u.Id === user.Id;
                });
            if (localUser == null) {
                $scope.users.push(user);
                return;
            }
            localUser.Position = user.Position;
        }
        ref.on("child_changed", function (childSnapshot) {
            processChild(childSnapshot.val());
        });
    }]);
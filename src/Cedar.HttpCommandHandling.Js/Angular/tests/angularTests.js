(function(){
    'use strict';

    describe('The command Api tests', function() {

        var $httpBackend, $http, $q, $rootScope, _commandApiProvider;

        // common setup
        beforeEach(function() {

            angular.module('testmodule', ['cedar.js'])
                .config(function(commandApiProvider) {
                    _commandApiProvider = commandApiProvider;
                });

            module('testmodule');

            inject(function(_$httpBackend_, _$http_, _$q_, _$rootScope_) {
                $httpBackend = _$httpBackend_;
                $http = _$http_;
                $q = _$q_;
                $rootScope = _$rootScope_;
            });

            _commandApiProvider.configure({ namespace: 'namespace' });
        });

        describe('When configured with a route prefix', function() {

            beforeEach(function() {

                $httpBackend.expectPUT(/\/prefix\/commands\/*/).respond(200);

                _commandApiProvider.configure({ routePrefix: '/prefix/', namespace: 'namespace' });
                _commandApiProvider.$get($http, $q, $rootScope).execute({ commandName: 'examplecommand' });

                $httpBackend.flush();
            });

            it('should use the prefix in the url', function() {
                $httpBackend.verifyNoOutstandingExpectation();
                $httpBackend.verifyNoOutstandingRequest();
            });
        });

        describe('When successfully executed', function() {
            var successHandler = jasmine.createSpy('success');
            
            beforeEach(function() {
                var command = { commandId: '1234', commandName: 'commandA', domainVersion: '4' };
                
                var expectedData = { "commandId": "1234", "commandName": "commandA", "domainVersion": "4" };
                var expectedHeaders = {
                    "content-type": 'application/vnd.namespace.commandA+json',
                    "Accept": "application/problem+json"
                };

                $httpBackend.expect('PUT', 'commands/1234', expectedData,
                    expectedHeaders).respond(200);
                _commandApiProvider.$get($http, $q, $rootScope).execute(command).then(successHandler);
                $httpBackend.flush();
            });

            it('should satisfy all PUT related expectations', function() {
                
                $httpBackend.verifyNoOutstandingExpectation();
                $httpBackend.verifyNoOutstandingRequest();
            });

            it('should return a resolved promise', function() {
                
                expect(successHandler).toHaveBeenCalled();
            });
        });

        describe('When unsuccessfully executed', function() {

            it('should return a rejected promise', function() {
                var command = { commandId: '1234', commandName: 'commandA', domainVersion: '4' };
                var errorHandler = jasmine.createSpy('error');

                $httpBackend.expectPUT(/commands\/1234/).respond(500);
                _commandApiProvider.$get($http, $q, $rootScope).execute(command).then(function() {}, errorHandler);
                $httpBackend.flush();
                expect(errorHandler).toHaveBeenCalled();
            });
        });
    });
})();


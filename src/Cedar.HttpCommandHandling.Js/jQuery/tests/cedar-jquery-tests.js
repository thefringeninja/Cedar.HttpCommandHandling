(function(){
    'use strict';

    describe('The command Api tests', function() {

        var commandApi;

        // describe('When configured with a route prefix', function() {

        //     beforeEach(function() {

        //         $httpBackend.expectPUT(/\/prefix\/commands\/*/).respond(200);

        //         _commandApiProvider.configure({ routePrefix: '/prefix/', namespace: 'namespace' });
        //         _commandApiProvider.$get($http, $q, $rootScope).execute({ commandName: 'examplecommand' });

        //         $httpBackend.flush();
        //     });

        //     it('should use the prefix in the url', function() {
        //         $httpBackend.verifyNoOutstandingExpectation();
        //         $httpBackend.verifyNoOutstandingRequest();
        //     });
        // });
        
        beforeEach(function() {
            commandApi = new cedarJs.CommandApi({
                namespace: 'namespace'
            });
        });

        describe('When successfully executed', function() {
            
            var successHandler = jasmine.createSpy('success');
            var success = false;
            
            beforeEach(function() {
                
                $.mockjax({
                  url: "commands/someid",
                  type: 'PUT',
                  status: 200,
                  onAfterSuccess: function(data){ 
                    success = true; }
                });

                commandApi.execute({
                    commandId: 'someid',
                    commandName: 'somename',
                    value: 'somevalue'
                }).then(successHandler);
            });

            it('should create a valid put request', function() {
                var mockedRequest = $.mockjax.mockedAjaxCalls()[0];
                expect(mockedRequest.url).toBe('commands/someid');
                expect(mockedRequest.contentType).toBe('application/vnd.somename+json');
                expect(mockedRequest.data.indexOf('"commandId":"someid"')).toBeGreaterThan(0);
                expect(mockedRequest.data.indexOf('"value":"somevalue"')).toBeGreaterThan(0);
                expect(mockedRequest.data.indexOf('"commandName":"somename"')).toBeGreaterThan(0);
            });

            it('should return a resolved promise', function(done) {
                
                waitsFor(function() { return success; }, 1000);
                 
                runs(function() {
                    expect(successHandler).toHaveBeenCalled();
                });
            });
        });

        // describe('When unsuccessfully executed', function() {

        //     it('should return a rejected promise', function() {
        //         var command = { commandId: '1234', commandName: 'commandA', domainVersion: '4' };
        //         var errorHandler = jasmine.createSpy('error');

        //         $httpBackend.expectPUT(/commands\/1234/).respond(500);
        //         _commandApiProvider.$get($http, $q, $rootScope).execute(command).then(function() {}, errorHandler);
        //         $httpBackend.flush();
        //         expect(errorHandler).toHaveBeenCalled();
        //     });
        // });
    });
})();


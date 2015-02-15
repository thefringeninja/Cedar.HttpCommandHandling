(function(){
    'use strict';

    $.mockjaxSettings.logging = false;

    describe('The command Api tests', function() {

        var commandApi;

        describe('When not newing up the command api', function() {

            beforeEach(function() {

                commandApi = cedarJs.CommandApi();
            });

            it('should still return an instance', function() {
                expect((commandApi instanceof cedarJs.CommandApi)).toBe(true);
            });
        });

        describe('When successfully executed', function() {
            
            var successHandler = jasmine.createSpy('success');
            var done = false;
            
            beforeEach(function() {
                    
                commandApi = new cedarJs.CommandApi({
                    routePrefix: 'test'
                });

                $.mockjax({
                  url: "test/commands/someid",
                  type: 'PUT',
                  status: 200,
                  onAfterSuccess: function(data){ 
                    done = true; }
                });

                commandApi.execute({
                    commandId: 'someid',
                    version: 'v1',
                    commandName: 'somename',
                    value: 'somevalue'
                }).then(successHandler);
            });

            it('should create a valid put request', function() {
                var mockedRequest = $.mockjax.mockedAjaxCalls()[0];
                expect(mockedRequest.url).toBe('test/commands/someid');
                expect(mockedRequest.contentType).toBe('application/vnd.somename-v1+json');
                expect(mockedRequest.data.indexOf('"commandId":"someid"')).toBeGreaterThan(0);
                expect(mockedRequest.data.indexOf('"value":"somevalue"')).toBeGreaterThan(0);
                expect(mockedRequest.data.indexOf('"commandName":"somename"')).toBeGreaterThan(0);
            });

            it('should call the success handler', function() {
                
                waitsFor(function() { return done; }, 1000);
                 
                runs(function() {
                    expect(successHandler).toHaveBeenCalled();
                });
            });
        });

        describe('When unsuccessfully executed', function() {

            var errorHandler = jasmine.createSpy('error');
            var done = false;
            
            beforeEach(function() {
                
                commandApi = new cedarJs.CommandApi();

                $.mockjax({
                  url: "commands/someid",
                  type: 'PUT',
                  status: 500,
                  onAfterError: function(data){ 
                    done = true; }
                });

                commandApi.execute({
                    commandId: 'someid',
                    commandName: 'somename'
                }).then(null, errorHandler);
            });

            it('should call the error handler', function() {
                
                waitsFor(function() { return done; }, 1000);
                 
                runs(function() {
                    expect(errorHandler).toHaveBeenCalled();
                });
            });
        });
    });
})();
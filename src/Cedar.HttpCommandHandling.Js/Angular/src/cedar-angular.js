'use strict';

var cedarJs = angular.module("cedarjs", [])
    .provider('commandApi', function() {
        
        var defaultOptions = {
            routePrefix: ''
        };
        var _http, _q, _rootScope, _options;

        return {
            configure: function(options) {
                
                _options = options;
            },
            $get: function($http, $q, $rootScope) {
                _q = $q;
                _http = $http;
                _rootScope = $rootScope;
                _options = _options || defaultOptions;

                function execute(command, deferred) {

                    var deferred = _q.defer();

                    sendCommand(command, deferred);
                    
                    return deferred.promise;
                }

                return {
                    execute: execute
                };
            }
        };

        function sendCommand(command, deferred) {
            _rootScope.$broadcast('commandSending', command);
            
             var prefix = _options.routePrefix || '';
             var version = '';
            if(command.version){
                version = '-' + command.version;
            }

            _http.put(
                prefix + 'commands/' + command.commandId,
                command, {
                    headers: {
                        'content-type': 'application/vnd.' + command.commandName + version + '+json',
                        'Accept': 'application/problem+json'
                    }
                })
            .success(function(data) {
                
                deferred.resolve(data);

                _rootScope.$broadcast('commandSent', command);
            })
            .error(function(data) {
                deferred.reject(data);
            });
        }
    });

// Use amd loading if available
if (typeof exports !== 'undefined') {
  if (typeof module !== 'undefined' && module.exports) {
    exports = module.exports = cedarJs;
  }
  exports.cedarJs = cedarJs;
}
(function(cedarJs, jQuery, undefined){

    var _options;
    
    var CommandApi = function(options){
        
        if(!(this instanceof CommandApi)){
            return new CommandApi(options);
        }

        if(!options.namespace) throw new Error("No namespace defined in the options");
        
        _options = options;


        this.execute = function(command){
            
            var prefix = _options.routePrefix || '';

            var deferred = new jQuery.Deferred();

            jQuery.ajax({
                url: prefix + 'commands/' + command.commandId,
                type: 'PUT',
                contentType: 'application/vnd.' + options.namespace + '.' + command.commandName + '+json',
                accepts: 'application/problem+json',
                data: JSON.stringify(command),
                error: function(data){
                    deferred.reject(data);
                },
                success: function(data){
                    deferred.resolve(data);
                }
            });

            return deferred.promise();
        }
    };

    cedarJs.CommandApi = CommandApi;

    // Use amd loading if available
    if (typeof exports !== 'undefined') {
      if (typeof module !== 'undefined' && module.exports) {
        exports = module.exports = cedarJs;
      }
      exports.cedarJs = cedarJs;
    }

})(window.cedarJs = window.cedarJs || {}, jQuery);
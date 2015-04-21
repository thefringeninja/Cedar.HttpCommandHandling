namespace Cedar.HttpCommandHandling
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Cedar.CommandHandling;

    public static class CommandMessageExtensions
    {
        public const string UserKey = "Cedar.HttpCommandHandling#User";

        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            object value;
            if(dictionary.TryGetValue(key, out value))
            {
                return (T) value;
            }
            return default(T);
        }

        public static ClaimsPrincipal GetUser<T>(this CommandMessage<T> commandMessage)
        {
            return commandMessage.Metadata.Get<ClaimsPrincipal>(UserKey);
        }

        public static CommandMessage<T> SetUser<T>(this CommandMessage<T> commandMessage, ClaimsPrincipal user)
        {
            commandMessage.Metadata[UserKey] = user;
            return commandMessage;
        }
    }
}
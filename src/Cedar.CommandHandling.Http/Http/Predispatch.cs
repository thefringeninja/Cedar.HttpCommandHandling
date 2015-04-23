namespace Cedar.CommandHandling.Http
{
    using System.Collections.Generic;

    public delegate void Predispatch(
        IDictionary<string, object> metadata,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders);
}
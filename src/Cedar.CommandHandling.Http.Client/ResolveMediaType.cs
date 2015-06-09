namespace Cedar.CommandHandling.Http
{
    using System;

    public delegate string ResolveMediaType(Type commandType, string serializationType);
}
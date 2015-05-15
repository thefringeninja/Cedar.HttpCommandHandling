namespace Cedar.CommandHandling.Http
{
    using System;

    public delegate object DeserializeCommand(
        string commandString,
        Type commandType);
}
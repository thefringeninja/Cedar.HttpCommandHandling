namespace Cedar.CommandHandling.Http
{
    using System;
    using System.IO;

    public delegate object DeserializeCommand(
        TextReader commandReader,
        Type commandType);
}
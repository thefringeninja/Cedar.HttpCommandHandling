namespace Cedar.CommandHandling.Http
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents an operation to custom deserialize a command.
    /// </summary>
    /// <param name="commandReader">The command reader.</param>
    /// <param name="commandType">Type of the command.</param>
    /// <returns></returns>
    public delegate object DeserializeCommand(
        TextReader commandReader,
        Type commandType);
}
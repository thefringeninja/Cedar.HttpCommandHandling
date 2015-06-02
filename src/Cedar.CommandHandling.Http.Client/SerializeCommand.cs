namespace Cedar.CommandHandling.Http
{
    using System.IO;

    /// <summary>
    /// Delegate that represents command serialization operations.
    /// </summary>
    /// <param name="commandWriter">The command writer the serializer is to write to.</param>
    /// <param name="command">The command to serialize.</param>
    public delegate void SerializeCommand(
        TextWriter commandWriter,
        object command);
}
namespace Cedar.CommandHandling
{
    public delegate void HandlerSync<TMessage>(TMessage message)
        where TMessage : class;
}
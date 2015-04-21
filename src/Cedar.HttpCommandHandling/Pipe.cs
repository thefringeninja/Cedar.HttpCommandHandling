namespace Cedar.HttpCommandHandling
{
    using Cedar.CommandHandling;

    public delegate Handler<TMessage> Pipe<TMessage>(Handler<TMessage> next) 
        where TMessage : class;
}
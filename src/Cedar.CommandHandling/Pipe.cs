namespace Cedar.CommandHandling
{
    public delegate Handler<TMessage> Pipe<TMessage>(Handler<TMessage> next) 
        where TMessage : class;
}
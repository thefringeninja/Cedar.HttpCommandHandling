namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;

    internal class CommandHandlerRegistration
    {
        private static readonly IEqualityComparer<CommandHandlerRegistration> MessageTypeComparerInstance =
            new MessageTypeEqualityComparer();

        private readonly object _handlerInstance;
        private readonly Type _commandType;
        private readonly Type _registrationType;

        internal CommandHandlerRegistration(Type commandType, Type registrationType, object handlerInstance)
        {
            _commandType = commandType;
            _registrationType = registrationType;
            _handlerInstance = handlerInstance;
        }

        internal static IEqualityComparer<CommandHandlerRegistration> MessageTypeComparer
        {
            get { return MessageTypeComparerInstance; }
        }

        public Type RegistrationType
        {
            get { return _registrationType; }
        }

        public Type CommandType
        {
            get { return _commandType; }
        }

        public object HandlerInstance
        {
            get { return _handlerInstance; }
        }

        private sealed class MessageTypeEqualityComparer : IEqualityComparer<CommandHandlerRegistration>
        {
            public bool Equals(CommandHandlerRegistration x, CommandHandlerRegistration y)
            {
                if(ReferenceEquals(x, y))
                {
                    return true;
                }
                if(ReferenceEquals(x, null))
                {
                    return false;
                }
                if(ReferenceEquals(y, null))
                {
                    return false;
                }
                if(x.GetType() != y.GetType())
                {
                    return false;
                }
                return x._commandType == y._commandType;
            }

            public int GetHashCode(CommandHandlerRegistration obj)
            {
                return obj._commandType.GetHashCode();
            }
        }
    }
}
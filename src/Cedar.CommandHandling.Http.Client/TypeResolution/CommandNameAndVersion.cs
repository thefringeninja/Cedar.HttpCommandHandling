namespace Cedar.CommandHandling.Http.TypeResolution
{
    using System.Collections.Generic;

    public class CommandNameAndVersion
    {
        public readonly string CommandName;
        public readonly int? Version;

        public CommandNameAndVersion(string commandName, int? version = null)
        {
            CommandName = commandName;
            Version = version;
        }

        private sealed class CommandNameAndVersionEqualityComparer : IEqualityComparer<CommandNameAndVersion>
        {
            public bool Equals(CommandNameAndVersion x, CommandNameAndVersion y)
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
                return string.Equals(x.CommandName, y.CommandName) && x.Version == y.Version;
            }

            public int GetHashCode(CommandNameAndVersion obj)
            {
                unchecked
                {
                    return (obj.CommandName.GetHashCode()*397) ^ obj.Version.GetHashCode();
                }
            }
        }

        private static readonly IEqualityComparer<CommandNameAndVersion> s_commandNameAndVersionComparerInstance =
            new CommandNameAndVersionEqualityComparer();

        public static IEqualityComparer<CommandNameAndVersion> CommandNameAndVersionComparer
        {
            get { return s_commandNameAndVersionComparerInstance; }
        }

        public override string ToString()
        {
            return string.Format("CommandName = {0}; Version = {1}", CommandName, Version);
        }
    }
}
namespace Cedar.CommandHandling.Http
{
    using System;
    using System.Reflection;

    internal static class TypeExtensions
    {
        internal static Assembly GetAssembly(this Type type)
        {
#if CEDAR_TYPEINFO
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }
    }
}

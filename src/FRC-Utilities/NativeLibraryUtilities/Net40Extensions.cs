using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FRC.NativeLibraryUtilties
{
    #if !NETSTANDARD
    internal static class Net35Extensions
    {

        public static object GetCustomAttribute(this FieldInfo info, Type t)
        {
            return Attribute.GetCustomAttribute(info, t);
        }
    }
#endif
}

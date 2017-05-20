using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace FRC.NativeLibraryUtilities
{
    /// <summary>
    /// This class contains methods to initialize delegates 
    /// </summary>
    public static class NativeDelegateInitializer
    {
#if NETSTANDARD
        /// <summary>
        /// Loads and setups a native delegate.
        /// </summary>
        /// <param name="library">The library to load from</param>
        /// <param name="nativeName">The native function name to load</param>
        /// <returns>A delegate that will call the native function</returns>
        public static T SetupNativeDelegate<T>(ILibraryInformation library, string nativeName)
        {
            return Marshal.GetDelegateForFunctionPointer<T>(library.LibraryLoader.GetProcAddress(nativeName));
        }
#else
        /// <summary>
        /// Loads and setups a native delegate.
        /// </summary>
        /// <param name="library">The library to load from</param>
        /// <param name="nativeName">The native function name to load</param>
        /// <param name="delegateType">The type of delegate to create</param>
        /// <returns>A delegate that will call the native function</returns>
        public static Delegate SetupNativeDelegate(ILibraryInformation library, string nativeName, Type delegateType)
        {
            return Marshal.GetDelegateForFunctionPointer(library.LibraryLoader.GetProcAddress(nativeName), delegateType);
        }
#endif

        /// <summary>
        /// Sets up all native delegate in the type passed as the generic parameter
        /// </summary>
        /// <typeparam name="T">The type to setup the native delegates in</typeparam>
        /// <param name="library">The object containing the native library to load from</param>
        public static void SetupNativeDelegates<T>(ILibraryInformation library)
        {
#if !NETSTANDARD
            var info = typeof(T);
#else
            TypeInfo info = typeof(T).GetTypeInfo();
#endif
#if NETSTANDARD
            MethodInfo getDelegateForFunctionPointer =
                typeof(Marshal).GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "GetDelegateForFunctionPointer" && m.IsGenericMethod);
#endif
            foreach (FieldInfo field in info.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = (NativeDelegateAttribute)field.GetCustomAttribute(typeof(NativeDelegateAttribute));
                if (attribute == null) continue;
                string nativeName = attribute.NativeName ?? field.Name;
#if NETSTANDARD
                MethodInfo delegateGetter = getDelegateForFunctionPointer.MakeGenericMethod(field.FieldType);
                object setVal = delegateGetter.Invoke(null, new object[] { library.LibraryLoader.GetProcAddress(nativeName) });
#else
                object setVal = Marshal.GetDelegateForFunctionPointer(library.LibraryLoader.GetProcAddress(nativeName),
                    field.FieldType);
#endif
                field.SetValue(null, setVal);
            }
        }

        /// <summary>
        /// Gets a list of all all native delegates in the type passed as the generic parameter
        /// </summary>
        /// <typeparam name="T">The type to setup the native delegates in</typeparam>
        /// <returns>A list of all native delegates that are being requested</returns>
        public static List<string> GetNativeDelegateList<T>()
        {
            List<string> nativeList = new List<string>();
#if !NETSTANDARD
            var info = typeof(T);
#else
            TypeInfo info = typeof(T).GetTypeInfo();
#endif
            foreach (FieldInfo field in info.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = (NativeDelegateAttribute)field.GetCustomAttribute(typeof(NativeDelegateAttribute));
                if (attribute == null) continue;
                string nativeName = attribute.NativeName ?? field.Name;
                nativeList.Add(nativeName);
            }
            return nativeList;
        }

        /// <summary>
        /// Gets a list of all all native delegates in the type passed as the generic parameter
        /// </summary>
        /// <param name="type">The type to setup the native delegates in</param>
        /// <returns>A list of all native delegates that are being requested</returns>
        public static List<string> GetNativeDelegateList(Type type)
        {
            List<string> nativeList = new List<string>();
#if !NETSTANDARD
            var info = type;
#else
            TypeInfo info = type.GetTypeInfo();
#endif
            foreach (FieldInfo field in info.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = (NativeDelegateAttribute)field.GetCustomAttribute(typeof(NativeDelegateAttribute));
                if (attribute == null) continue;
                string nativeName = attribute.NativeName ?? field.Name;
                nativeList.Add(nativeName);
            }
            return nativeList;
        }
    }
}

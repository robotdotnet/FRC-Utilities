using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC
{
    /// <summary>
    /// Utility class for working with UTF8 string getting passed to native code.
    /// </summary>
    public static class UTF8String
    {
        private static readonly ConcurrentDictionary<string, CachedNativeString> s_stringCache = new ConcurrentDictionary<string, CachedNativeString>();

        private static readonly Func<string, CachedNativeString> CacheCreateFunc = (s) => new CachedNativeString(s);

        /// <summary>
        /// Creates a UTF8 string that will be cached, using the already cached string is already created
        /// </summary>
        /// <param name="str">The string to cache</param>
        /// <returns>The cached UTF8 string</returns>
        public static CachedNativeString CreateCachedUTF8String(string str)
        {
            return s_stringCache.GetOrAdd(str, CacheCreateFunc);
        }

        /// <summary>
        /// Creates a UTF8 null termincated string and stores it in a ManagedString
        /// </summary>
        /// <param name="str">The string to create as UTF8</param>
        /// <returns>The UTF8 string</returns>
        public static ManagedString CreateUTF8String(string str)
        {
            return new ManagedString(str);
        }

        /// <summary>
        /// Creates a UTF8 null termincated native string that can be disposed.
        /// </summary>
        /// <param name="str">The string to create as UTF8</param>
        /// <returns>The native UTF8 string</returns>
        public static DisposableNativeString CreateUTF8DisposableString(string str)
        {
            return new DisposableNativeString(str);
        }

        /// <summary>
        /// Reads a UTF8 string from a native pointer.
        /// </summary>
        /// <param name="str">The pointer to read from</param>
        /// <param name="size">The length of the string</param>
        /// <returns>The managed string</returns>
        public static string ReadUTF8String(IntPtr str, UIntPtr size)
        {
            unsafe
            {
#if (!NET451)
                return Encoding.UTF8.GetString((byte*)str, (int)size);
#else
                int iSize = (int)size.ToUInt64();
                byte[] data = new byte[iSize];
                for (int i = 0; i < iSize; i++)
                {
                    data[i] = ((byte*)str)[i];
                }
                return Encoding.UTF8.GetString(data);
#endif
            }
        }

        /// <summary>
        /// Reads a UTF8 string from a native pointer.
        /// </summary>
        /// <param name="str">The pointer to read from</param>
        /// <param name="size">The length of the string</param>
        /// <returns>The managed string</returns>
        public static unsafe string ReadUTF8String(byte* str, UIntPtr size)
        {
#if (!NET451)
            return Encoding.UTF8.GetString(str, (int)size);
#else
            int iSize = (int)size.ToUInt64();
            byte[] data = new byte[iSize];
            for (int i = 0; i < iSize; i++)
            {
                data[i] = str[i];
            }
            return Encoding.UTF8.GetString(data);
#endif
        }

        /// <summary>
        /// Reads a UTF8 string from a null termincated native pointer
        /// </summary>
        /// <param name="str">The pointer to read from (must be null terminated)</param>
        /// <returns>The managed string</returns>
        public static string ReadUTF8String(IntPtr str)
        {
            unsafe
            {
                byte* data = (byte*)str;
                int count = 0;
                while (true)
                {
                    if (data[count] == 0)
                    {
                        break;
                    }
                    count++;
                }

                return ReadUTF8String(str, (UIntPtr)count);
            }
        }

        /// <summary>
        /// Reads a UTF8 string from a null termincated native pointer
        /// </summary>
        /// <param name="str">The pointer to read from (must be null terminated)</param>
        /// <returns>The managed string</returns>
        public static unsafe string ReadUTF8String(byte* str)
        {
            byte* data = str;
            int count = 0;
            while (true)
            {
                if (data[count] == 0)
                {
                    break;
                }
                count++;
            }

            return ReadUTF8String(str, (UIntPtr)count);
        }
    }
}

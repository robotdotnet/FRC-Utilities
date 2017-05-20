using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC
{
    /// <summary>
    /// Holds a Cached UTF8 string to pass to native code. There is no way to initialize or dispose of this
    /// string from user code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CachedNativeString
    {
        /// <summary>
        /// Pointer to this string, null terminated. Do not modify this pointer.
        /// </summary>
        public IntPtr Buffer;
        /// <summary>
        /// The Length of this string without the null terminator;
        /// </summary>
        public UIntPtr Length;

        internal CachedNativeString(string vStr)
        {
            unsafe
            {
                fixed (char* str = vStr)
                {
                    var encoding = Encoding.UTF8;
                    int bytes = encoding.GetByteCount(str, vStr.Length);
                    Buffer = Marshal.AllocHGlobal((bytes + 1) * sizeof(byte));
                    Length = (UIntPtr)bytes;
                    byte* data = (byte*)Buffer.ToPointer();
                    encoding.GetBytes(str, vStr.Length, data, bytes);
                    data[bytes] = 0;
                }
            }
        }
    }
}

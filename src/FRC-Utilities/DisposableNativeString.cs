using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FRC
{
    /// <summary>
    /// Holds a UTF8 string to pass to native code. Make sure to properly dispose of this to avoid a memory leak
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DisposableNativeString : IDisposable
    {
        /// <summary>
        /// Pointer to this string, null terminated. Do not modify this pointer.
        /// </summary>
        public IntPtr Buffer;
        /// <summary>
        /// The Length of this string without the null terminator;
        /// </summary>
        public UIntPtr Length;

        /// <summary>
        /// Creates a new UTF8 string from a managed string
        /// </summary>
        /// <param name="vStr">The managed string</param>
        public DisposableNativeString(string vStr)
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

        /// <summary>
        /// Disposes of the native string
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(Buffer);
        }

        /// <summary>
        /// Gets the string
        /// </summary>
        /// <returns>The contained string</returns>
        public override string ToString()
        {
            return UTF8String.ReadUTF8String(Buffer, Length);
        }
    }
}

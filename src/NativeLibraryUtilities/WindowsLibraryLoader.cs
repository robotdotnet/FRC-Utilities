using System;
using System.Runtime.InteropServices;

namespace NativeLibraryUtilities
{
    public class WindowsLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            return LoadLibrary(filename);
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            IntPtr addr = GetProcAddress(dllHandle, name);
            if (addr == IntPtr.Zero)
            {
                //Address not found. Throw Exception
                throw new Exception($"Method not found: {name}");
            }
            return addr;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            FreeLibrary(handle);
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr handle, string procedureName);

        [DllImport("kernel32")]
        private static extern bool FreeLibrary(IntPtr handle);
    }

}

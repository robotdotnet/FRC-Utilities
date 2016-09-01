using System;

namespace NativeLibraryUtilities
{
    public interface ILibraryLoader
    {
        IntPtr LoadLibrary(string filename);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);

        void UnloadLibrary(IntPtr handle);
    }
}

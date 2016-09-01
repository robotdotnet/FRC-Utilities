using System;
using System.Runtime.InteropServices;

namespace NativeLibraryUtilities
{
    public class MacOsLibraryLoader : ILibraryLoader
    {
        IntPtr ILibraryLoader.LoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero) return dl;
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
            return dl;
        }

        IntPtr ILibraryLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            dlerror();
            IntPtr result = dlsym(dllHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        void ILibraryLoader.UnloadLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        [DllImport("dl")]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("dl")]
        private static extern IntPtr dlerror();

        [DllImport("dl")]
        private static extern int dlclose(IntPtr handle);
    }
}

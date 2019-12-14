using System;
using System.Runtime.InteropServices;
using FRC.ILGeneration;

namespace FRC.NativeLibraryUtilities
{
    ///<summary>
    /// Library loader for embedded devices
    /// </summary>
    public class EmbeddedLibraryLoader : ILibraryLoader
    {
        /// <inheritdoc/>
        public IntPtr NativeLibraryHandle { get; private set; } = IntPtr.Zero;

        /// <inheritdoc/>
        void ILibraryLoader.LoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero)
            {
                NativeLibraryHandle = dl;
                return;
            };
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new DllNotFoundException($"Library Could not be opened: {Marshal.PtrToStringAnsi(err)}");
            }
        }

        /// <summary>
        /// Try to load a native library from a path
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool TryLoadLibrary(string filename)
        {
            IntPtr dl = dlopen(filename, 2);
            if (dl != IntPtr.Zero)
            {
                NativeLibraryHandle = dl;
                return true;
            };
            return false;
        }

        /// <inheritdoc/>
        IntPtr IFunctionPointerLoader.GetProcAddress(string name)
        {
            dlerror();
            IntPtr result = dlsym(NativeLibraryHandle, name);
            IntPtr err = dlerror();
            if (err != IntPtr.Zero)
            {
                throw new TypeLoadException($"Method not found: {Marshal.PtrToStringAnsi(err)}");
            }
            return result;
        }

        /// <inheritdoc/>
        void ILibraryLoader.UnloadLibrary()
        {
            dlclose(NativeLibraryHandle);
        }

        [DllImport("libdl.so.2")]
#pragma warning disable IDE1006 // Naming Styles
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlerror();

        [DllImport("libdl.so.2")]
        private static extern int dlclose(IntPtr handle);
#pragma warning restore IDE1006 // Naming Styles
    }
}

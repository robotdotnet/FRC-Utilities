using System;

namespace NativeLibraryUtilities
{
    public interface ILibraryHolder
    {
        ILibraryLoader LibraryLoader { get; }
        IntPtr LibraryHandle { get; }
        OsType OsType { get; }
    }
}

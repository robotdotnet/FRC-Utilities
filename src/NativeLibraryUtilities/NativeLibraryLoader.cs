using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NativeLibraryUtilities
{
    public class NativeLibraryLoader<T> : ILibraryHolder
    {
        private readonly Dictionary<OsType, string> m_nativeLibraryName = new Dictionary<OsType, string>();

        public ILibraryLoader LibraryLoader { get; private set; }
        public IntPtr LibraryHandle { get; private set; }

        public OsType OsType { get; } = GetOsType();

        private bool m_internalExtraction;
        private string m_extractLocation;

        public string LibraryLocation { get; private set; }

        public NativeLibraryLoader(bool internalExtraction)
        {
            m_internalExtraction = internalExtraction;
            m_extractLocation = Path.GetTempFileName();
        }

        public NativeLibraryLoader(string extractLocation)
        {
            m_internalExtraction = true;
            m_extractLocation = extractLocation;
        }


        public void AddLibraryLocation(OsType osType, string libraryName)
        {
            m_nativeLibraryName.Add(osType, libraryName);
        }

        public void LoadNativeLibrary(ILibraryLoader loader, string location)
        {
            if (m_internalExtraction)
            {
                ExtractNativeLibrary(location);
                LibraryLoader = loader;
                LibraryHandle = loader.LoadLibrary(m_extractLocation);
                LibraryLocation = m_extractLocation;
            }
            else
            {
                // Otherwise directly load.
                LibraryLoader = loader;
                LibraryHandle = loader.LoadLibrary(location);
                LibraryLocation = location;
            }
        }

        public void LoadNativeLibrary(string location)
        {
            if (OsType == OsType.None)
                throw new InvalidOperationException(
                    "OS type is unknown. Must use the overload to manually load the file");

            if (!m_nativeLibraryName.ContainsKey(OsType))
                throw new InvalidOperationException("OS Type not contained in dictionary");

            switch (OsType)
            {
                case OsType.Windows32:
                case OsType.Windows64:
                    LibraryLoader = new WindowsLibraryLoader();
                    break;
                case OsType.Linux32:
                case OsType.Linux64:
                    LibraryLoader = new LinuxLibraryLoader();
                    break;
                case OsType.MacOs32:
                case OsType.MacOs64:
                    LibraryLoader = new LinuxLibraryLoader();
                    break;
            }

            if (m_internalExtraction)
            {
                ExtractNativeLibrary(location);
                LibraryHandle = LibraryLoader.LoadLibrary(m_extractLocation);
                LibraryLocation = m_extractLocation;
            }
            else
            {
                LibraryHandle = LibraryLoader.LoadLibrary(location);
                LibraryLocation = location;
            }

        }

        public void LoadNativeLibrary()
        {
            if (OsType == OsType.None)
                throw new InvalidOperationException(
                    "OS type is unknown. Must use the overload to manually load the file");

            if (!m_nativeLibraryName.ContainsKey(OsType))
                throw new InvalidOperationException("OS Type not contained in dictionary");

            switch (OsType)
            {
                case OsType.Windows32:
                case OsType.Windows64:
                    LibraryLoader = new WindowsLibraryLoader();
                    break;
                case OsType.Linux32:
                case OsType.Linux64:
                    LibraryLoader = new LinuxLibraryLoader();
                    break;
                case OsType.MacOs32:
                case OsType.MacOs64:
                    LibraryLoader = new LinuxLibraryLoader();
                    break;
            }

            if (m_internalExtraction)
            {
                ExtractNativeLibrary(m_nativeLibraryName[OsType]);
                LibraryHandle = LibraryLoader.LoadLibrary(m_extractLocation);
                LibraryLocation = m_extractLocation;
            }
            else
            {
                LibraryHandle = LibraryLoader.LoadLibrary(m_nativeLibraryName[OsType]);
                LibraryLocation = m_nativeLibraryName[OsType];
            }
 
        }

        private void ExtractNativeLibrary(string location)
        {
            byte[] bytes;
            //Load our resource file into memory
            using (Stream s = typeof(T).GetTypeInfo().Assembly.GetManifestResourceStream(location))
            {
                if (s == null || s.Length == 0)
                    throw new InvalidOperationException("File to extract cannot be null or empty");
                bytes = new byte[(int)s.Length];
                s.Read(bytes, 0, (int)s.Length);
            }
            File.WriteAllBytes(m_extractLocation, bytes);
            GC.Collect();
        }

        private static bool Is64BitOs()
        {
            return IntPtr.Size != sizeof(int);
        }

        private static bool IsWindows()
        {
#if NETSTANDARD
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            return Path.DirectorySeparatorChar == '\\';
#endif
        }

        private static OsType GetOsType()
        {
            if (IsWindows())
            {
                return Is64BitOs() ? OsType.Windows64 : OsType.Windows32;
            }
            else
            {
#if NETSTANDARD
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (Is64BitOs()) return OsType.Linux64;
                    else return OsType.Linux32;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (Is64BitOs()) return OsType.MacOs64;
                    else return OsType.MacOs32;
                }
                else
                {
                    return OsType.None;
                }
#else
                Utsname uname;
                try
                {
                    Uname.uname(out uname);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return OsType.None;
                }


                Console.WriteLine(uname.ToString());

                bool mac = uname.sysname == "Darwin";

                //Check for Bitness
                if (Is64BitOs())
                {
                    //We are 64 bit.
                    if (mac) return OsType.MacOs64;
                    return OsType.Linux64;
                }
                else
                {
                    //We are 64 32 bit process.
                    if (mac) return OsType.MacOs32;
                    return OsType.Linux32;
                }
#endif
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FRC.NativeLibraryUtilties
{
    /// <summary>
    /// This class handles loading of a native library
    /// </summary>
    public class NativeLibraryLoader : ILibraryInformation
    {
        private readonly Dictionary<OsType, string> m_nativeLibraryName = new Dictionary<OsType, string>();

        /// <inheritdoc/>
        public ILibraryLoader LibraryLoader { get; private set; }
        /// <inheritdoc/>
        public OsType OsType { get; } = GetOsType();
        /// <inheritdoc/>
        public bool UsingTempFile { get; private set; }

        /// <inheritdoc/>
        public string LibraryLocation { get; private set; }

        /// <summary>
        /// Checks if the current system is a roboRIO
        /// </summary>
        /// <returns>True if running on a roboRIO</returns>
        public static bool CheckIsRoboRio()
        {
            return File.Exists("/usr/local/frc/bin/frcRunRobot.sh");
        }

        /// <summary>
        /// Add a file location to be used when automatically searching for a library to load
        /// </summary>
        /// <param name="osType">The OsType to associate with the file</param>
        /// <param name="libraryName">The file to load on that OS</param>
        public void AddLibraryLocation(OsType osType, string libraryName)
        {
            m_nativeLibraryName.Add(osType, libraryName);
        }

        /// <summary>
        /// Loads a native library using the specified loader and file
        /// </summary>
        /// <typeparam name="T">The type containing the native resource, if it is embedded.</typeparam>
        /// <param name="loader">The LibraryLoader to use</param>
        /// <param name="location">The file location. Can be either an embedded resource, or a direct file location</param>
        /// <param name="directLoad">True to load the file directly from disk, otherwise false to extract from embedded</param>
        /// <param name="extractLocation">The location to extract to if the file is embedded. On null, it extracts to a temp file</param>
        public void LoadNativeLibrary<T>(ILibraryLoader loader, string location, bool directLoad = false, string extractLocation = null)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader), "Library loader cannot be null");
            if (location == null)
                throw new ArgumentNullException(nameof(location), "Library location cannot be null");

            // Set to use temp file if extractLocation is null
            if (StringUtil.IsNullOrWhiteSpace(extractLocation) && !directLoad)
            {
                extractLocation = Path.GetTempFileName();
                UsingTempFile = true;
            }

            // RoboRIO or Direct Load
            if (directLoad)
            {
                LibraryLoader = loader;
                loader.LoadLibrary(location);
                LibraryLocation = location;
            }
            else
            // If we are loading from extraction, extract then load
            {
                ExtractNativeLibrary(location, extractLocation, typeof(T));
                LibraryLoader = loader;
                loader.LoadLibrary(extractLocation);
                LibraryLocation = extractLocation;
            }
        }

        /// <summary>
        /// Loads a native library using the specified file. The OS is determined automatically
        /// </summary>
        /// <typeparam name="T">The type containing the native resource, if it is embedded.</typeparam>
        /// <param name="location">The file location. Can be either an embedded resource, or a direct file location</param>
        /// <param name="directLoad">True to load the file directly from disk, otherwise false to extract from embedded</param>
        /// <param name="extractLocation">The location to extract to if the file is embedded. On null, it extracts to a temp file</param>
        public void LoadNativeLibrary<T>(string location, bool directLoad = false, string extractLocation = null)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location), "Library location cannot be null");

            OsType osType = OsType;

            if (osType == OsType.None)
                throw new InvalidOperationException(
                    "OS type is unknown. Must use the overload to manually load the file");

            if (!m_nativeLibraryName.ContainsKey(osType) && !directLoad)
                throw new InvalidOperationException("OS Type not contained in dictionary");

            switch (osType)
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
                    LibraryLoader = new MacOsLibraryLoader();
                    break;
                case OsType.LinuxArmhf:
                case OsType.LinuxRaspbian:
                case OsType.roboRIO:
                    LibraryLoader = new EmbeddedLibraryLoader();
                    break;
            }

            LoadNativeLibrary<T>(LibraryLoader, location, directLoad, extractLocation);
        }

        /// <summary>
        /// Loads a native library, using locations added using <see cref="AddLibraryLocation"/>
        /// </summary>
        /// <typeparam name="T">The type containing the native resource, if it is embedded.</typeparam>
        /// <param name="directLoad">True to load the file directly from disk, otherwise false to extract from embedded</param>
        /// <param name="extractLocation">The location to extract to if the file is embedded. On null, it extracts to a temp file</param>
        public void LoadNativeLibrary<T>(bool directLoad = false, string extractLocation = null)
        {
            OsType osType = OsType;

            if (osType == OsType.None)
                throw new InvalidOperationException(
                    "OS type is unknown. Must use the overload to manually load the file");

            if (!m_nativeLibraryName.ContainsKey(osType) && !directLoad)
                throw new InvalidOperationException("OS Type not contained in dictionary");

            switch (osType)
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
                    LibraryLoader = new MacOsLibraryLoader();
                    break;
                case OsType.LinuxArmhf:
                case OsType.LinuxRaspbian:
                case OsType.roboRIO:
                    LibraryLoader = new EmbeddedLibraryLoader();
                    break;
            }

            LoadNativeLibrary<T>(LibraryLoader, m_nativeLibraryName[osType], directLoad, extractLocation);
        }

        /// <summary>
        /// Loads a native library with a reflected assembly holding the native libraries
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to reflect into and load from</param>
        /// <param name="localLoadOnRio">True to force a local load on the RoboRIO</param>
        public void LoadNativeLibraryFromReflectedAssembly(string assemblyName, bool localLoadOnRio = true)
        {
            if (localLoadOnRio && CheckIsRoboRio())
            {
                ILibraryLoader loader = new EmbeddedLibraryLoader();
                LibraryLoader = loader;
                var location = m_nativeLibraryName[OsType.roboRIO];
                loader.LoadLibrary(location);
                LibraryLocation = location;
                return;
            }

            AssemblyName name = new AssemblyName(assemblyName);
            Assembly asm;
            try
            {
                asm = Assembly.Load(name);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to load desktop libraries. Please ensure that the {assemblyName} is installed and referenced by your project", e);
            }

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
                    LibraryLoader = new MacOsLibraryLoader();
                    break;
                case OsType.LinuxArmhf:
                case OsType.LinuxRaspbian:
                case OsType.roboRIO:
                    LibraryLoader = new EmbeddedLibraryLoader();
                    break;
            }

            if (LibraryLoader == null)
                throw new ArgumentNullException(nameof(LibraryLoader), "Library loader cannot be null");

            string extractLocation = Path.GetTempFileName();
            UsingTempFile = true;

            ExtractNativeLibrary(m_nativeLibraryName[OsType], extractLocation, asm);
            LibraryLoader.LoadLibrary(extractLocation);
            LibraryLocation = extractLocation;
        }

        private void ExtractNativeLibrary(string resourceLocation, string extractLocation, Assembly asm)
        {
            byte[] bytes;
            //Load our resource file into memory
            using (Stream s = asm.GetManifestResourceStream(resourceLocation))
            {
                if (s == null || s.Length == 0)
                    throw new InvalidOperationException("File to extract cannot be null or empty");
                bytes = new byte[(int)s.Length];
                s.Read(bytes, 0, (int)s.Length);
            }
            File.WriteAllBytes(extractLocation, bytes);
            GC.Collect();
        }

        private void ExtractNativeLibrary(string resourceLocation, string extractLocation, Type type)
        {
#if !NETSTANDARD
            ExtractNativeLibrary(resourceLocation, extractLocation, type.Assembly);
#else
            ExtractNativeLibrary(resourceLocation, extractLocation, type.GetTypeInfo().Assembly);
#endif
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

        /// <summary>
        /// Gets the OS Type of the current running system.
        /// </summary>
        /// <returns></returns>
        public static OsType GetOsType()
        {
            if (IsWindows())
            {
                return Is64BitOs() ? OsType.Windows64 : OsType.Windows32;
            }
            else
            {
                if (CheckIsRoboRio())
                {
                    return OsType.roboRIO;
                }
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

                bool mac = uname.sysname == "Darwin";
                bool armv7 = uname.machine.ToLower().Contains("armv7");
                bool armv6 = uname.machine.ToLower().Contains("armv6");

                if (armv7)
                {
                    try
                    {
                        string text = File.ReadAllText("/etc/os-release");
                        if (text.Contains("ID=raspbian"))
                        {
                            return OsType.LinuxRaspbian;
                        }
                        else
                        {
                            return OsType.LinuxArmhf;
                        }
                    }
                    catch
                    {
                        return OsType.LinuxArmhf;
                    }
                }
                if (armv6)
                {
                    throw new PlatformNotSupportedException("Arm v6 Devices (most likely a Pi 1 or a Pi Zero) are not supported");
                }

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

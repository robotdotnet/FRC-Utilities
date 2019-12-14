using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace FRC.NativeLibraryUtilities
{
    internal sealed class Utsname
        : IEquatable<Utsname>
    {
        private readonly string sysname;
        private readonly string nodename;
        private readonly string release;
        private readonly string version;
        private readonly string machine;
        private readonly string domainname;

        public Utsname(string sysname, string nodename, string release, string version, string machine, string domainname)
        {
            this.sysname = sysname;
            this.nodename = nodename;
            this.release = release;
            this.version = version;
            this.machine = machine;
            this.domainname = domainname;
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return sysname.GetHashCode() ^ nodename.GetHashCode() ^
                release.GetHashCode() ^ version.GetHashCode() ^
                machine.GetHashCode() ^ domainname.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Utsname u = (Utsname)obj;
            return Equals(u);
        }

        public bool Equals(Utsname value)
        {
            return value.sysname == sysname && value.nodename == nodename &&
                value.release == release && value.version == version &&
                value.machine == machine && value.domainname == domainname;
        }

        // Generate string in /etc/passwd format
        public override string ToString()
        {
            return $"{sysname} {nodename} {release} {version} {machine}";
        }

        public static bool operator ==(Utsname lhs, Utsname rhs)
        {
            return Object.Equals(lhs, rhs);
        }

        public static bool operator !=(Utsname lhs, Utsname rhs)
        {
            return !Object.Equals(lhs, rhs);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable IDE1006 // Naming Styles
    internal struct _Utsname
#pragma warning restore IDE1006 // Naming Styles
    {
        public IntPtr sysname;
        public IntPtr nodename;
        public IntPtr release;
        public IntPtr version;
        public IntPtr machine;
        public IntPtr domainname;
        public IntPtr _buf_;
    }

    internal class Uname
    {
        private static void CopyUtsname(out Utsname to, ref _Utsname from)
        {
            try
            {
                to = new Utsname(
                    Marshal.PtrToStringAnsi(from.sysname),
                    Marshal.PtrToStringAnsi(from.nodename),
                    Marshal.PtrToStringAnsi(from.release),
                    Marshal.PtrToStringAnsi(from.version),
                    Marshal.PtrToStringAnsi(from.machine),
                    Marshal.PtrToStringAnsi(from.domainname)
               );
            }
            finally
            {
                free(from._buf_);
                from._buf_ = IntPtr.Zero;
            }
        }

        internal const string MPH = "MonoPosixHelper";
        internal const string LIBC = "libc";
#pragma warning disable IDE1006 // Naming Styles

        [DllImport(LIBC, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free(IntPtr ptr);

        [DllImport(MPH, SetLastError = true,
             EntryPoint = "Mono_Posix_Syscall_uname")]
        private static extern int sys_uname(out _Utsname buf);


        public static int uname(out Utsname? buf)
#pragma warning restore IDE1006 // Naming Styles
        {
            int r = sys_uname(out _Utsname _buf);
            buf = null;
            if (r == 0)
            {
                CopyUtsname(out buf, ref _buf);
            }
            return r;
        }
    }
}

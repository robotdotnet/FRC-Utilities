using System;

namespace FRC.NativeLibraryUtilities
{
    /// <summary>
    /// Specifies that the attributed field should be considered a target for native initialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NativeCallAttribute : Attribute
    {
        /// <summary>
        /// Gets the native name for this field if set.
        /// </summary>
        public string? NativeName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="NativeCallAttribute"/>,
        /// using the name of the field as the native name.
        /// </summary>
        public NativeCallAttribute()
        {
            NativeName = null;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NativeCallAttribute"/>,
        /// with the name of the native method passed in.
        /// </summary>
        /// <param name="nativeName"></param>
        public NativeCallAttribute(string nativeName)
        {
            NativeName = nativeName;
        }
    }
}

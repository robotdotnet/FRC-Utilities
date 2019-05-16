﻿using System;

namespace FRC.NativeLibraryUtilities
{
    /// <summary>
    /// Specifies that the attributed field should be considered a target for native initialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NativeCallAtrribute : Attribute
    {
        /// <summary>
        /// Gets the native name for this field if set.
        /// </summary>
        public string NativeName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="NativeCallAtrribute"/>,
        /// using the name of the field as the native name.
        /// </summary>
        public NativeCallAtrribute()
        {
            NativeName = null;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NativeCallAtrribute"/>,
        /// with the name of the native method passed in.
        /// </summary>
        /// <param name="nativeName"></param>
        public NativeCallAtrribute(string nativeName)
        {
            NativeName = nativeName;
        }
    }
}
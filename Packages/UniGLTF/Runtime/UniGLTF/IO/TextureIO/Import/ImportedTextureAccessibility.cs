using System;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Accessibility of imported texture bitmap
    /// </summary>
    public enum ImportedTexturesAccessibility
    {
        /// <summary>
        /// Platform dependent.
        /// see: ToMarkNonReadable()
        /// </summary>
        Auto,

        /// <summary>
        /// Bitmaps are accessible. 
        /// Specify explicitly when exporting an imported model again.
        /// <summary>
        Readable,

        /// <summary>
        /// It can save memory usage. Recommended.
        /// </summary>
        NonReadable,
    }

    public static class TextureReadableParamExtensions
    {
        /// <summary>
        /// convert to Texture2D.LoadImage param
        /// </summary>
        public static bool ToMarkNonReadable(this ImportedTexturesAccessibility self)
        {
            switch (self)
            {
                case ImportedTexturesAccessibility.Auto:
                    if (Application.isPlaying)
                    {
                        // change behaviour from v0.128.4
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case ImportedTexturesAccessibility.Readable:
                    return false;

                case ImportedTexturesAccessibility.NonReadable:
                    return true;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
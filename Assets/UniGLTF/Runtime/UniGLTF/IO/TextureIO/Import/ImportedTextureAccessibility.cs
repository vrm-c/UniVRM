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
        /// IsEditor ? Readable : NonReadable
        /// </summary>
        Default,

        /// <summary>
        /// Bitmaps are accessible. 
        /// Specify explicitly when exporting an imported model again
        /// <summary>
        Readable,

        /// <summary>
        /// It can save memory usage. Recommended
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
                case ImportedTexturesAccessibility.Default:
                    if (Application.isEditor)
                    {
                        return false;
                    }
                    else
                    {
                        // v0.128.4 からの挙動変更
                        return true;
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
/// <summary>
/// https://gist.github.com/szimek/763999
/// </summary>
namespace UniGLTF
{
    public enum glComponentType : int
    {
        BYTE = 5120, // signed ?
        UNSIGNED_BYTE = 5121,

        SHORT = 5122,
        UNSIGNED_SHORT = 5123,

        //INT = 5124,
        UNSIGNED_INT = 5125,

        FLOAT = 5126,
    }

    public static class glComponentTypeExtensions
    {
        public static int GetByteSize(this glComponentType self)
        {
            switch (self)
            {
                case glComponentType.BYTE: return 1;
                case glComponentType.UNSIGNED_BYTE: return 1;
                case glComponentType.SHORT: return 2;
                case glComponentType.UNSIGNED_SHORT: return 2;
                case glComponentType.UNSIGNED_INT: return 4;
                case glComponentType.FLOAT: return 4;
                default: throw new System.NotImplementedException();
            }
        }
    }

    public enum glBufferTarget : int
    {
        NONE = 0,
        ARRAY_BUFFER = 34962,
        ELEMENT_ARRAY_BUFFER = 34963,
    }

    public enum glFilter : int
    {
        NONE = 0,
        NEAREST = 9728,
        LINEAR = 9729,

        #region for minFilter only
        NEAREST_MIPMAP_NEAREST = 9984,
        LINEAR_MIPMAP_NEAREST = 9985,
        NEAREST_MIPMAP_LINEAR = 9986,
        LINEAR_MIPMAP_LINEAR = 9987,
        #endregion
    }

    public enum glWrap : int
    {
        NONE = 0,
        CLAMP_TO_EDGE = 33071,
        REPEAT = 10497,
        MIRRORED_REPEAT = 33648,
    }
}

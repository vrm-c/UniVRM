using System;


namespace UniGLTF
{
    public class UniGLTFException : Exception
    {
        public UniGLTFException(string fmt, params object[] args) : this(string.Format(fmt, args)) { }
        public UniGLTFException(string msg) : base(msg) { }
    }

    public class UniGLTFNotSupportedException : UniGLTFException
    {
        public UniGLTFNotSupportedException(string fmt, params object[] args) : this(string.Format(fmt, args)) { }
        public UniGLTFNotSupportedException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception in parse the glb header
    /// </summary>
    public class GlbParseException : UniGLTFException
    {
        public GlbParseException(string msg) : base(msg) { }
    }
}

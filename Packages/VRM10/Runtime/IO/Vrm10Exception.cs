using System;

namespace UniVRM10
{
    public class Vrm10Exception : Exception
    {
        public Vrm10Exception(string msg) : base(msg)
        { }
    }

    public class Vrm10NoExtensionException : Vrm10Exception
    {
        public Vrm10NoExtensionException(string msg) : base(msg)
        { }
    }
}

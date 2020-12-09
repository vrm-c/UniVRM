using System;


namespace VRM
{
    class VRMException : Exception
    {
        public VRMException()
        { }
        public VRMException(string msg) : base(msg)
        { }
        public VRMException(string msg, params object[] args) : base(string.Format(msg, args))
        { }
    }
}

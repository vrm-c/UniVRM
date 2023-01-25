using System;

namespace VRMShaders
{
    public interface IDisposableResourceList : IDisposable
    {
        public void PushDisposable(UnityEngine.Object disposable);
    }
}

using System;
using UnityEngine.Profiling;

namespace SphereTriangle
{
    public struct ProfileSample : IDisposable
    {
        string _name;

        public ProfileSample(string name)
        {
            _name = name;
            Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            Profiler.EndSample();
        }
    }
}
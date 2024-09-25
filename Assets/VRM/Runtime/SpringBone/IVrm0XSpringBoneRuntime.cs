using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public struct SpringRuntimeFrameInfo
    {
        public float DeltaTime;
        public bool UseRuntimeScalingSupport;
    }

    public interface IVrm0XSpringBoneRuntime
    {
        Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller);

        void Reset();

        void Process(SpringRuntimeFrameInfo frame);
    }
}
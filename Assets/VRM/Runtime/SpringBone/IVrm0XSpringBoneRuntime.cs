using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public interface IVrm0XSpringBoneRuntime
    {
        Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller);

        void Reset();

        void Process(float deltaTime);
    }
}
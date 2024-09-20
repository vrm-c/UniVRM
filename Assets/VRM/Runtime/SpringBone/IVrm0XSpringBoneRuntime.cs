using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public interface IVrm0XSpringBoneRuntime
    {
        public Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller);
    }
}
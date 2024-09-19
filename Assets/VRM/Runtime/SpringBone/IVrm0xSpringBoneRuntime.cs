using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public interface IVrm0xSpringBoneRuntime
    {
        public Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller);
    }
}
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public interface IVRM0SpringBoneRuntime
    {
        public Task InitializeAsync(GameObject vrm, IAwaitCaller awaitCaller);
    }
}
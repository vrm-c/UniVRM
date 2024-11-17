using System;
using System.Threading.Tasks;
using UniGLTF;
using UniVRM10;

namespace RotateParticle
{
    public interface IRotateParticleSystem : IDisposable
    {
        Task InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller);
        void Process(float deltaTime);
        void ResetInitialRotation();
        void DrawGizmos();
    }
}
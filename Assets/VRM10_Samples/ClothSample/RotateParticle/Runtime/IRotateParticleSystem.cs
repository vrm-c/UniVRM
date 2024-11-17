using System;
using System.Collections.Generic;
using RotateParticle.Components;
using UniVRM10;

namespace RotateParticle
{
    public interface IRotateParticleSystem : IDisposable
    {
        void Initialize(IEnumerable<Warp> warps, IEnumerable<RectCloth> cloths);
        void Process(float deltaTime);
        void ResetInitialRotation();
        void DrawGizmos();
    }
}
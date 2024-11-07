using System;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UniVRM10;

namespace RotateParticle
{
    /// <summary>
    /// SpcriptedImporter 経由の import 向け。
    /// NativeArray の確保や DontDestroyOnLoad を回避。
    /// </summary>
    public class RotateParticleSpringboneRuntime : IVrm10SpringBoneRuntime
    {
        Action<Vrm10Instance> _setup;

        public RotateParticleSpringboneRuntime(Action<Vrm10Instance> setup)
        {
            _setup = setup;
        }

        public void Dispose()
        {
        }

        public Task InitializeAsync(Vrm10Instance instance, IAwaitCaller awaitCaller)
        {
            if (_setup != null)
            {
                _setup(instance);
            }
            var system = instance.GetComponent<RotateParticleSystem>();
            system.Initialize();

            return Task.CompletedTask;
        }

        public void Process()
        {
        }

        public bool ReconstructSpringBone()
        {
            return false;
        }

        public void RestoreInitialTransform()
        {
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
        }
    }
}
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using VRM.FastSpringBones.Registries;
using VRM.FastSpringBones.Schedulers;

namespace VRM.FastSpringBones.Components
{
    /// <summary>
    /// Jobを連続して発火させるComponent
    /// シーンに1つだけあればいい
    /// </summary>
    [DefaultExecutionOrder(11000)]
    public sealed class FastSpringBoneScheduler : MonoBehaviour
    {
        [SerializeField] private bool showGizmos;

        private CustomSampler _updateSampler;

        private PullTransformJobScheduler _pullTransformJobScheduler;
        private PushTransformJobScheduler _pushTransformJobScheduler;
        private UpdateSpringBoneJobScheduler _updateSpringBoneJobScheduler;

        private RootBoneRegistry _rootBoneRegistry;
        private ColliderGroupRegistry _colliderGroupRegistry;

        private JobHandle _prevJobHandle;

        public bool ShowGizmos { get => showGizmos; set => showGizmos = value; }

        public void Initialize(
            RootBoneRegistry rootBoneRegistry,
            TransformRegistry transformRegistry,
            ColliderGroupRegistry colliderGroupRegistry)
        {
            _rootBoneRegistry = rootBoneRegistry;
            _colliderGroupRegistry = colliderGroupRegistry;

            _updateSampler = CustomSampler.Create("FastSpringBone(Update)");

            _pullTransformJobScheduler = new PullTransformJobScheduler(transformRegistry);
            _pushTransformJobScheduler = new PushTransformJobScheduler(transformRegistry);
            _updateSpringBoneJobScheduler = new UpdateSpringBoneJobScheduler(_rootBoneRegistry);

            _rootBoneRegistry.SubscribeOnValueChanged(OnRootBoneChanged);
        }

        private void OnDestroy()
        {
            _rootBoneRegistry.UnSubscribeOnValueChanged(OnRootBoneChanged);
            _prevJobHandle.Complete();

            _pullTransformJobScheduler.Dispose();
            _pushTransformJobScheduler.Dispose();
            _updateSpringBoneJobScheduler.Dispose();
        }

        private void OnRootBoneChanged()
        {
            _prevJobHandle.Complete();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!ShowGizmos) return;

            _prevJobHandle.Complete();

            Gizmos.color = Color.blue;
            foreach (var rootBoneWrapper in _rootBoneRegistry.Items)
            {
                rootBoneWrapper.Value.DrawGizmos();
            }

            Gizmos.color = Color.yellow;
            foreach (var colliderGroup in _colliderGroupRegistry.Items)
            {
                colliderGroup.DrawGizmos();
            }
        }
#endif

        private void LateUpdate()
        {
            _updateSampler.Begin();

            _prevJobHandle.Complete();

            var tempJobHandle = default(JobHandle);
            tempJobHandle = _pullTransformJobScheduler.Schedule(tempJobHandle);
            tempJobHandle = _updateSpringBoneJobScheduler.Schedule(tempJobHandle);
            tempJobHandle = _pushTransformJobScheduler.Schedule(tempJobHandle);

            _prevJobHandle = tempJobHandle;

            _updateSampler.End();
        }
    }
}
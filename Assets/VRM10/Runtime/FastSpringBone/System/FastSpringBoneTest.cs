using UnityEngine;

namespace UniVRM10.FastSpringBones.System
{
    public sealed class FastSpringBoneTest : MonoBehaviour
    {
        [SerializeField] private FastSpringBoneSpring[] springs;

        private FastSpringBoneScheduler _scheduler;

        private void Start()
        {
            _scheduler = new FastSpringBoneScheduler(springs);
        }

        private void LateUpdate()
        {
            _scheduler.Schedule();
        }

        private void OnDestroy()
        {
            _scheduler.Dispose();
        }
    }
}
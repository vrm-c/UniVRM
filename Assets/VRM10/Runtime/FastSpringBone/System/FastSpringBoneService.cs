using UnityEngine;

namespace UniVRM10.FastSpringBones.System
{
    [DefaultExecutionOrder(11000)]
    public sealed class FastSpringBoneService : MonoBehaviour
    {
        public FastSpringBoneBufferCombiner BufferCombiner { get; private set; }
        private FastSpringBoneScheduler _fastSpringBoneScheduler;

        private static FastSpringBoneService _instance;

        public static FastSpringBoneService Instance
        {
            get
            {
                if (_instance) return _instance;

                var gameObject = new GameObject("FastSpringBone Service");
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<FastSpringBoneService>();

                return _instance;
            }
        }

        /// <summary>
        /// 専有しているインスタンスを破棄する
        /// </summary>
        public static void Free()
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }

        private void Awake()
        {
            BufferCombiner = new FastSpringBoneBufferCombiner();
            _fastSpringBoneScheduler = new FastSpringBoneScheduler(BufferCombiner);
        }

        private void OnDestroy()
        {
            _fastSpringBoneScheduler.Dispose();
        }

        private void LateUpdate()
        {
            _fastSpringBoneScheduler.Schedule().Complete();
        }
    }
}
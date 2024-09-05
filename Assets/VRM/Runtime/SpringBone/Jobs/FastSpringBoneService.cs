using UniGLTF.SpringBoneJobs;
using UnityEngine;
using UnityEngine.Profiling;


namespace VRM.SpringBoneJobs
{
    /// <summary>
    /// シングルトン。FastSpringBone の ServiceLocator 兼 EntryPoint。
    /// シーンすべてのVRMのSpringBoneをバッチングしてまとめて実行する。
    /// </summary>
    [DefaultExecutionOrder(11000)]
    public class FastSpringBoneService : MonoBehaviour
    {
        private CustomSampler _updateSampler = CustomSampler.Create("FastSpringBone(Update)");

        public FastSpringBoneBufferCombiner BufferCombiner { get; private set; }
        private FastSpringBoneScheduler _fastSpringBoneScheduler;

        private static FastSpringBoneService _instance;

        public static FastSpringBoneService Instance
        {
            get
            {
                if (_instance) return _instance;

#if UNITY_2022_3_OR_NEWER
                _instance = FindFirstObjectByType<FastSpringBoneService>();
#else
                _instance = FindObjectOfType<FastSpringBoneService>();
#endif
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

        private void OnEnable()
        {
            BufferCombiner = new FastSpringBoneBufferCombiner();
            _fastSpringBoneScheduler = new FastSpringBoneScheduler(BufferCombiner);
        }

        private void OnDisable()
        {
            BufferCombiner.Dispose();
            _fastSpringBoneScheduler.Dispose();
        }

        private void LateUpdate()
        {
            _updateSampler.Begin();

            _fastSpringBoneScheduler.Schedule(Time.deltaTime).Complete();

            _updateSampler.End();
        }
    }
}
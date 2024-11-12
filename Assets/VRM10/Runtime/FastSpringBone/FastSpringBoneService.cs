using UniGLTF.SpringBoneJobs;
using UnityEngine;

namespace UniVRM10.FastSpringBones
{
    [DefaultExecutionOrder(11010)]
    /// <summary>
    /// VRM-1.0 ではコンポーネントの処理順が規定されている
    /// 
    /// 1. ヒューマノイドボーンを解決
    /// 2. 頭の位置が決まるのでLookAtを解決
    /// 3. ExpressionUpdate
    /// 4. コンストレイントを解決
    /// 5. SpringBoneを解決
    /// 
    /// 1~4 は Vrm10Runtime が管理し LateUpdate で処理される。
    /// このクラスは DefaultExecutionOrder(11000) によりその後ろにまわる。
    /// 
    /// # [Manual update]
    /// 
    /// foreach(var vrmInstance in allVrm)
    /// {
    ///     vrmInstance.UpdateType = None;
    ///     vrmInstance.Runtime.Process();
    /// }
    /// FastSpringBoneService.Instance.UpdateType = Manual;
    /// FastSpringBoneService.Instance.ManualUpdate();
    ///
    /// </summary>
    public sealed class FastSpringBoneService : MonoBehaviour
    {
        public enum UpdateTypes
        {
            Manual,
            LateUpdate,
        }

        [SerializeField, Header("Runtime")]
        public UpdateTypes UpdateType = UpdateTypes.LateUpdate;


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
            if (UpdateType == UpdateTypes.LateUpdate)
            {
                _fastSpringBoneScheduler.Schedule(Time.deltaTime).Complete();
            }
        }

        public void ManualUpdate(float deltaTime)
        {
            if (UpdateType != UpdateTypes.Manual)
            {
                throw new global::System.ArgumentException("require UpdateTypes.Manual");
            }
            _fastSpringBoneScheduler.Schedule(deltaTime).Complete();
        }

        public void OnDrawGizmosSelected()
        {
            if (enabled)
            {
                BufferCombiner.DrawGizmos();
            }
        }
    }
}
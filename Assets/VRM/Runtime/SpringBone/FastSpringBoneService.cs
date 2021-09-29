using UnityEngine;
using VRM.FastSpringBones.Components;
using VRM.FastSpringBones.Registries;

namespace VRM
{
    /// <summary>
    /// Scene 上に単一で存在する、FastSpringBone の ServiceLocator 兼 EntryPoint
    /// </summary>
    public class FastSpringBoneService : MonoBehaviour
    {
        public RootBoneRegistry RootBoneRegistry { get; private set; }
        public TransformRegistry TransformRegistry { get; private set; }
        public ColliderGroupRegistry ColliderGroupRegistry { get; private set; }
        public FastSpringBoneScheduler FastSpringBoneScheduler { get; private set; }

        private static FastSpringBoneService _instance;

        public static FastSpringBoneService Instance
        {
            get
            {
                if (!_instance)
                {
                    var gameObject = new GameObject("FastSpringBone Service");
                    DontDestroyOnLoad(gameObject);
                    _instance = gameObject.AddComponent<FastSpringBoneService>();
                }
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
            RootBoneRegistry = new RootBoneRegistry();
            TransformRegistry = new TransformRegistry();
            ColliderGroupRegistry = new ColliderGroupRegistry();
            FastSpringBoneScheduler = gameObject.AddComponent<FastSpringBoneScheduler>();
            FastSpringBoneScheduler.Initialize(
                RootBoneRegistry,
                TransformRegistry,
                ColliderGroupRegistry);
        }
    }
}
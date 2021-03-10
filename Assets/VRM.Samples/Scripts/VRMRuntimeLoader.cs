#pragma warning disable 0414
using System.IO;
using UniGLTF;
using UnityEngine;


namespace VRM.Samples
{
    public class VRMRuntimeLoader : MonoBehaviour
    {
        [SerializeField]
        bool m_loadAsync = default;

        [SerializeField, Header("GUI")]
        CanvasManager m_canvas = default;

        [SerializeField]
        LookTarget m_faceCamera = default;

        [SerializeField, Header("loader")]
        UniHumanoid.HumanPoseTransfer m_source;

        [SerializeField]
        UniHumanoid.HumanPoseTransfer m_target;

        [SerializeField, Header("runtime")]
        VRMFirstPerson m_firstPerson;

        VRMBlendShapeProxy m_blendShape;

        void SetupTarget()
        {
            if (m_target != null)
            {
                m_target.Source = m_source;
                m_target.SourceType = UniHumanoid.HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

                m_blendShape = m_target.GetComponent<VRMBlendShapeProxy>();

                m_firstPerson = m_target.GetComponent<VRMFirstPerson>();

                var animator = m_target.GetComponent<Animator>();
                if (animator != null)
                {
                    m_firstPerson.Setup();

                    if (m_faceCamera != null)
                    {
                        m_faceCamera.Target = animator.GetBoneTransform(HumanBodyBones.Head);
                    }
                }
            }
        }

        private void Awake()
        {
            SetupTarget();
        }

        private void Start()
        {
            if (m_canvas == null)
            {
                Debug.LogWarning("no canvas");
                return;
            }

            m_canvas.LoadVRMButton.onClick.AddListener(LoadVRMClicked);
            m_canvas.LoadBVHButton.onClick.AddListener(LoadBVHClicked);
        }

        async void LoadVRMClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("open VRM", ".vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            // GLB形式でJSONを取得しParseします
            var parser = new GltfParser();
            parser.Parse(path, bytes);

            using (var context = new VRMImporterContext(parser))
            {
                // metaを取得(todo: thumbnailテクスチャのロード)
                var meta = await context.ReadMetaAsync();
                Debug.LogFormat("meta: title:{0}", meta.Title);

                // ParseしたJSONをシーンオブジェクトに変換していく
                if (m_loadAsync)
                {
                    await context.LoadAsync();
                }
                else
                {
                    context.Load();
                }

                OnLoaded(context);
            }
        }

        /// <summary>
        /// メタが不要な場合のローダー
        /// </summary>
        async void LoadVRMClicked_without_meta()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("open VRM", ".vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            // GLB形式でJSONを取得しParseします
            var parser = new GltfParser();
            parser.ParseGlb(bytes);

            var context = new VRMImporterContext(parser);
            if (m_loadAsync)
            {
                await context.LoadAsync();
            }
            else
            {
                context.Load();
            }
            OnLoaded(context);
        }

        void LoadBVHClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("open BVH", ".bvh");
            if (!string.IsNullOrEmpty(path))
            {
                LoadBvh(path);
            }
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open BVH", "", "bvh");
            if (!string.IsNullOrEmpty(path))
            {
                LoadBvh(path);
            }
#else
            LoadBvh(Application.dataPath + "/default.bvh");
#endif
        }

        void OnLoaded(VRMImporterContext context)
        {
            var root = context.Root;

            root.transform.SetParent(transform, false);

            //メッシュを表示します
            context.ShowMeshes();
            context.DisposeOnGameObjectDestroyed();

            // add motion
            var humanPoseTransfer = root.AddComponent<UniHumanoid.HumanPoseTransfer>();
            if (m_target != null)
            {
                GameObject.Destroy(m_target.gameObject);
            }
            m_target = humanPoseTransfer;
            SetupTarget();
        }

        void LoadBvh(string path)
        {
            Debug.LogFormat("ImportBvh: {0}", path);
            var context = new UniHumanoid.BvhImporterContext();

            context.Parse(path);
            context.Load();

            if (m_source != null)
            {
                GameObject.Destroy(m_source.gameObject);
            }
            m_source = context.Root.GetComponent<UniHumanoid.HumanPoseTransfer>();

            SetupTarget();
        }
    }
}

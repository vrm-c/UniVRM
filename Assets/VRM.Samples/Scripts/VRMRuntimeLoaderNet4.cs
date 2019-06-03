#pragma warning disable 0414
using System;
using System.IO;
using UnityEngine;
#if (NET_4_6 && UNITY_2017_1_OR_NEWER)
using System.Threading.Tasks;
#endif


namespace VRM.Samples
{
    public class VRMRuntimeLoaderNet4 : MonoBehaviour
    {
        [SerializeField, Header("GUI")]
        CanvasManager m_canvas;

        [SerializeField]
        LookTarget m_faceCamera;

        [SerializeField, Header("loader")]
        UniHumanoid.HumanPoseTransfer m_source;

        [SerializeField]
        UniHumanoid.HumanPoseTransfer m_target;

        [SerializeField, Header("runtime")]
        VRMFirstPerson m_firstPerson;

#if (NET_4_6 && UNITY_2017_1_OR_NEWER && !UNITY_WEBGL)
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

        // Byte列を得る
        async static Task<Byte[]> ReadBytesAsync(string path)
        {
            return File.ReadAllBytes(path);
        }

        async static Task<VRMImporterContext> LoadAsync(Byte[] bytes)
        {
            var context = new VRMImporterContext();

            // GLB形式でJSONを取得しParseします
            context.ParseGlb(bytes);

            // ParseしたJSONをシーンオブジェクトに変換していく
            await context.LoadAsyncTask();

            return context;
        }

        /// <summary>
        /// Taskで非同期にロードする例
        /// </summary>
        async void LoadVRMClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("open VRM", ".vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }


            var context = new VRMImporterContext();

            var bytes = await ReadBytesAsync(path);

            // GLB形式でJSONを取得しParseします
            context.ParseGlb(bytes);

            // metaを取得(todo: thumbnailテクスチャのロード)
            var meta = context.ReadMeta();
            Debug.LogFormat("meta: title:{0}", meta.Title);

            // ParseしたJSONをシーンオブジェクトに変換していく
            var now = Time.time;
            await context.LoadAsyncTask();

            var delta = Time.time - now;
            Debug.LogFormat("LoadVrmAsync {0:0.0} seconds", delta);
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
#endif
    }
}

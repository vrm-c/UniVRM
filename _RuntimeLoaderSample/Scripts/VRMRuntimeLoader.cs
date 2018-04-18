#pragma warning disable 0414
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;


namespace VRM
{
    public class VRMRuntimeLoader : MonoBehaviour
    {
        [SerializeField]
        bool m_loadAsync;

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

        void LoadVRMClicked()
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

            var bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            var context = new VRMImporterContext(path);

            // GLB形式でJSONを取得しParseします
            context.ParseVrm(bytes);


            // metaを取得(todo: thumbnailテクスチャのロード)
            var meta = context.ReadMeta();
            Debug.LogFormat("meta: title:{0}", meta.Title);


            // ParseしたJSONをシーンオブジェクトに変換していく
            if (m_loadAsync)
            {
                LoadAsync(context);
            }
            else
            {
                VRMImporter.LoadFromBytes(context);
                OnLoaded(context.Root);
            }
        }

        /// <summary>
        /// メタが不要な場合のローダー
        /// </summary>
        void LoadVRMClicked_without_meta()
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

#if true
            var bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            if (m_loadAsync)
            {
                // ローカルファイルシステムからロードします
                VRMImporter.LoadVrmAsync(bytes, OnLoaded);
            }
            else
            {
                var root=VRMImporter.LoadFromBytes(bytes);
                OnLoaded(root);
            }

#else
            // ParseしたJSONをシーンオブジェクトに変換していく
            if (m_loadAsync)
            {
                // ローカルファイルシステムからロードします
                VRMImporter.LoadVrmAsync(path, OnLoaded);
            }
            else
            {
                var root=VRMImporter.LoadFromPath(path);
                OnLoaded(root);
            }
#endif
        }


        void LoadAsync(VRMImporterContext context)
        {
#if true
            var now = Time.time;
            VRMImporter.LoadVrmAsync(context, go=> {
                var delta = Time.time - now;
                Debug.LogFormat("LoadVrmAsync {0:0.0} seconds", delta);
                OnLoaded(go);
            });
#else
            // ローカルファイルシステムからロードします
            VRMImporter.LoadVrmAsync(path, OnLoaded);
#endif
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

        void OnLoaded(GameObject root)
        {
            root.transform.SetParent(transform, false);

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
            var context = new UniHumanoid.ImporterContext
            {
                Path = path
            };
            UniHumanoid.BvhImporter.Import(context);

            if (m_source != null)
            {
                GameObject.Destroy(m_source.gameObject);
            }
            m_source = context.Root.GetComponent<UniHumanoid.HumanPoseTransfer>();

            SetupTarget();
        }
    }
}

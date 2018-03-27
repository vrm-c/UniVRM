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

#if UNITY_STANDALONE_WIN
        #region GetOpenFileName
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        public static bool GetOpenFileName1([In, Out] OpenFileName ofn)
        {
            return GetOpenFileName(ofn);
        }
        static string Filter(params string[] filters)
        {
            return string.Join("\0", filters) + "\0";
        }
        static string FileDialog(string extension)
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = Filter("All Files", "*.*", extension, "*" + extension);
            ofn.filterIndex = 2;
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = UnityEngine.Application.dataPath;
            ofn.title = "Upload Image";
            ofn.defExt = "PNG";
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
            if (!GetOpenFileName(ofn))
            {
                return null;
                //FileDialogResult("file:///" + ofn.file);
            }        // later, possibly in some other method:

            return ofn.file;

        }
        #endregion
#endif

        VRMBlendShapeProxy m_blendShape;

        void SetupTarget()
        {
            if (m_target != null)
            {
                m_target.Source = m_source;

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
            var path = FileDialog(".vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (!string.IsNullOrEmpty(path))
            {
                if (m_loadAsync)
                {
                    LoadAsync(path);
                }
                else
                {
                    LoadSync(path);
                }
            }
        }

        void LoadAsync(string path)
        {
#if true
            // ネットワーク経由等でbyte列が取得できる場合はこちら
            var bytes = File.ReadAllBytes(path);
            VRMImporter.LoadVrmAsync(bytes, OnLoaded);
#else
            // ローカルファイルシステムからロードします
            VRMImporter.LoadVrmAsync(path, OnLoaded);
#endif
        }

        void LoadSync(string path)
        {
#if true
            // ネットワーク経由等でbyte列が取得できる場合はこちら
            var bytes = File.ReadAllBytes(path);
            LoadSync(bytes);
#else
            // ローカルファイルシステムからロードします
            VRMImporter.LoadFromPath(path);
#endif
        }

        void LoadSync(byte[] bytes)
        {
            var context = VRMImporter.LoadFromBytes(bytes);
            OnLoaded(context.Root);
        }

        void LoadBVHClicked()
        {
            var path = FileDialog(".bvh");
            if (!string.IsNullOrEmpty(path))
            {
                LoadBvh(path);
            }
        }

        void OnLoaded(GameObject root)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = root;
#endif

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

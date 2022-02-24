#pragma warning disable 0414
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;


namespace UniVRM10.FirstPersonSample
{
    public class VRM10RuntimeLoader : MonoBehaviour
    {
        [SerializeField, Header("GUI")]
        VRM10CanvasManager m_canvas = default;

        [SerializeField]
        VRM10LookTarget m_faceCamera = default;

        [SerializeField, Header("loader")]
        UniHumanoid.HumanPoseTransfer m_source;

        [SerializeField]
        UniHumanoid.HumanPoseTransfer m_target;


        void SetupTarget(UniHumanoid.HumanPoseTransfer m_target)
        {
            if (m_target == null)
            {
                return;
            }

            m_target.Source = m_source;
            m_target.SourceType = UniHumanoid.HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

            var animator = m_target.GetComponent<Animator>();
            if (animator != null)
            {
                if (m_faceCamera != null)
                {
                    m_faceCamera.Target = animator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
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
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", ".vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var instance = await LoadAsync(path, new VRMShaders.RuntimeOnlyAwaitCaller());

            var root = instance.gameObject;
            root.transform.SetParent(transform, false);

            // add motion
            var humanPoseTransfer = root.AddComponent<UniHumanoid.HumanPoseTransfer>();
            if (m_target != null)
            {
                GameObject.Destroy(m_target.gameObject);
            }
            m_target = humanPoseTransfer;
            SetupTarget(m_target);
        }

        async Task<Vrm10Instance> LoadAsync(string path, VRMShaders.IAwaitCaller awaitCaller)
        {
            var instance = await Vrm10.LoadPathAsync(path, awaitCaller: awaitCaller);

            // VR用 FirstPerson 設定
            await instance.Vrm.FirstPerson.SetupAsync(instance.gameObject, awaitCaller);

            return instance;
        }

        void LoadBVHClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open BVH", ".bvh");
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

            SetupTarget(m_target);
        }
    }
}

﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;
using VRMShaders;


namespace VRM.SimpleViewer
{
    /// <summary>
    /// UI event handling
    /// </summary>
    public class ViewerUI : MonoBehaviour
    {
        #region UI
        [SerializeField]
        Text m_version = default;

        [SerializeField]
        Button m_open = default;

        [SerializeField]
        Toggle m_enableLipSync = default;

        [SerializeField]
        Toggle m_enableAutoBlink = default;

        [SerializeField]
        Toggle m_useUrpMaterial = default;

        [SerializeField]
        Toggle m_useAsync = default;

        [SerializeField]
        Toggle m_loadAnimation = default;

        [SerializeField]
        Toggle m_useFastSpringBone = default;
        #endregion

        [SerializeField]
        HumanPoseTransfer m_src = default;

        [SerializeField]
        GameObject m_target = default;

        [SerializeField]
        GameObject Root = default;

        [SerializeField]
        Button m_reset = default;

        [SerializeField]
        TextAsset m_motion;

        [Serializable]
        class TextFields
        {
            [SerializeField, Header("Info")]
            Text m_textModelTitle = default;
            [SerializeField]
            Text m_textModelVersion = default;
            [SerializeField]
            Text m_textModelAuthor = default;
            [SerializeField]
            Text m_textModelContact = default;
            [SerializeField]
            Text m_textModelReference = default;
            [SerializeField]
            RawImage m_thumbnail = default;

            [SerializeField, Header("CharacterPermission")]
            Text m_textPermissionAllowed = default;
            [SerializeField]
            Text m_textPermissionViolent = default;
            [SerializeField]
            Text m_textPermissionSexual = default;
            [SerializeField]
            Text m_textPermissionCommercial = default;
            [SerializeField]
            Text m_textPermissionOther = default;

            [SerializeField, Header("DistributionLicense")]
            Text m_textDistributionLicense = default;
            [SerializeField]
            Text m_textDistributionOther = default;

            public void Start()
            {
                m_textModelTitle.text = "";
                m_textModelVersion.text = "";
                m_textModelAuthor.text = "";
                m_textModelContact.text = "";
                m_textModelReference.text = "";

                m_textPermissionAllowed.text = "";
                m_textPermissionViolent.text = "";
                m_textPermissionSexual.text = "";
                m_textPermissionCommercial.text = "";
                m_textPermissionOther.text = "";

                m_textDistributionLicense.text = "";
                m_textDistributionOther.text = "";
            }

            public void UpdateMeta(VRMMetaObject meta)
            {
                m_textModelTitle.text = meta.Title;
                m_textModelVersion.text = meta.Version;
                m_textModelAuthor.text = meta.Author;
                m_textModelContact.text = meta.ContactInformation;
                m_textModelReference.text = meta.Reference;

                m_textPermissionAllowed.text = meta.AllowedUser.ToString();
                m_textPermissionViolent.text = meta.ViolentUssage.ToString();
                m_textPermissionSexual.text = meta.SexualUssage.ToString();
                m_textPermissionCommercial.text = meta.CommercialUssage.ToString();
                m_textPermissionOther.text = meta.OtherPermissionUrl;

                m_textDistributionLicense.text = meta.LicenseType.ToString();
                m_textDistributionOther.text = meta.OtherLicenseUrl;

                m_thumbnail.texture = meta.Thumbnail;
            }
        }

        [SerializeField]
        TextFields m_texts = default;

        [Serializable]
        class UIFields
        {
            [SerializeField]
            Toggle ToggleMotionTPose = default;

            [SerializeField]
            Toggle ToggleMotionBVH = default;

            [SerializeField]
            ToggleGroup ToggleMotion = default;

            Toggle m_activeToggleMotion = default;

            public void UpdateToggle(Action onBvh, Action onTPose)
            {
                var value = ToggleMotion.ActiveToggles().FirstOrDefault();
                if (value == m_activeToggleMotion)
                    return;

                m_activeToggleMotion = value;
                if (value == ToggleMotionTPose)
                {
                    onTPose();
                }
                else if (value == ToggleMotionBVH)
                {
                    onBvh();
                }
                else
                {
                    Debug.Log("motion: no toggle");
                }
            }
        }
        [SerializeField]
        UIFields m_ui = default;

        [SerializeField]
        HumanPoseClip m_pose = default;

        private void Reset()
        {
            var buttons = GameObject.FindObjectsOfType<Button>();
            m_open = buttons.First(x => x.name == "Open");

            m_reset = buttons.First(x => x.name == "ResetSpringBone");

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_useFastSpringBone = toggles.First(x => x.name == "UseFastSpringBone");
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");

            m_src = GameObject.FindObjectOfType<HumanPoseTransfer>();

            m_target = GameObject.FindObjectOfType<TargetMover>().gameObject;
        }

        Loaded m_loaded;

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_open.onClick.AddListener(OnOpenClicked);
            m_useFastSpringBone.onValueChanged.AddListener(OnUseFastSpringBoneValueChanged);
            OnUseFastSpringBoneValueChanged(m_useFastSpringBone.isOn);

            m_reset.onClick.AddListener(() => m_loaded?.ResetSpring());

            // load initial bvh
            if (m_motion != null)
            {
                LoadMotion("tmp.bvh", m_motion.text);
            }

            string[] cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                LoadPathAsync(cmds[1]);
            }

            m_texts.Start();
        }

        private void LoadMotion(string path, string source)
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            m_src = context.Root.GetComponent<HumanPoseTransfer>();
            m_src.GetComponent<Renderer>().enabled = false;
            m_loaded?.EnableBvh(m_src);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            m_ui.UpdateToggle(
                () => m_loaded?.EnableBvh(m_src),
                () => m_loaded?.EnableTPose(m_pose));

            if (m_loaded != null)
            {
                m_loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                m_loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                m_loaded.Update();
            }
        }

        IEnumerator LoadCoroutine(string url)
        {
            var www = new UnityEngine.Networking.UnityWebRequest(url);
            yield return www;
            var task = LoadBytesAsync("WebGL.vrm", www.downloadHandler.data);
        }

        /// <summary>
        /// for WebGL
        /// call from OpenFile.jslib
        /// </summary>
        public void FileSelected(string url)
        {
            Debug.Log($"FileSelected: {url}");
            StartCoroutine(LoadCoroutine(url));
        }

        void OnOpenClicked()
        {
            var path = FileUtil.OpenFileDialog("Open VRM", "vrm", "bvh");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            LoadPathAsync(path);
        }

        async void LoadPathAsync(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{path} not exists");
                return;
            }
            var bytes = File.ReadAllBytes(path);
            await LoadBytesAsync(path, bytes);
        }

        public async Task LoadBytesAsync(string path, byte[] bytes)
        {
            var size = bytes != null ? bytes.Length : 0;
            Debug.Log($"LoadModelAsync: {path}: {size}bytes");

            var ext = Path.GetExtension(path).ToLower();
            if (ext == ".bvh")
            {
                // bvh motion
                LoadMotion(path, File.ReadAllText(path));
                return;
            }

            // cleanup
            if (m_loaded != null)
            {
                m_loaded.Dispose();
                m_loaded = null;
            }

            // vrm
            VrmUtility.MaterialGeneratorCallback materialCallback = (VRM.glTF_VRM_extensions vrm) => GetVrmMaterialGenerator(m_useUrpMaterial.isOn, vrm);
            VrmUtility.MetaCallback metaCallback = m_texts.UpdateMeta;
            var instance = await VrmUtility.LoadBytesAsync(path, bytes, GetIAwaitCaller(m_useAsync.isOn), materialCallback, metaCallback, loadAnimation: m_loadAnimation.isOn);

            if (m_useFastSpringBone.isOn)
            {
                var _ = FastSpringBoneReplacer.ReplaceAsync(instance.Root);
            }

            instance.EnableUpdateWhenOffscreen();
            instance.ShowMeshes();

            m_loaded = new Loaded(instance, m_src, m_target.transform);
        }

        void OnUseFastSpringBoneValueChanged(bool flag)
        {
            m_reset.gameObject.SetActive(!flag);
        }

        static IMaterialDescriptorGenerator GetGltfMaterialGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new GltfUrpMaterialDescriptorGenerator();
            }
            else
            {
                return new GltfMaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialGenerator(bool useUrp, VRM.glTF_VRM_extensions vrm)
        {
            if (useUrp)
            {
                return new VRM.VRMUrpMaterialDescriptorGenerator(vrm);
            }
            else
            {
                return new VRM.VRMMaterialDescriptorGenerator(vrm);
            }
        }

        static IAwaitCaller GetIAwaitCaller(bool useAsync)
        {
            if (useAsync)
            {
#if UNITY_WEBGL
                return new RuntimeOnlyNoThreadAwaitCaller();
#else                
                return new RuntimeOnlyAwaitCaller();
#endif
            }
            else
            {
                return new ImmediateCaller();
            }
        }
    }
}

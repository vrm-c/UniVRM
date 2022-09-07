using System;
using System.IO;
using System.Linq;
using System.Threading;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        Text m_version = default;

        [SerializeField]
        Button m_open = default;

        [SerializeField]
        Toggle m_enableLipSync = default;

        [SerializeField]
        Toggle m_enableAutoBlink = default;

        [SerializeField]
        Toggle m_enableAutoExpression = default;

        [SerializeField]
        Toggle m_useUrpMaterial = default;

        [SerializeField]
        Toggle m_useAsync = default;

        [Header("Runtime")]
        [SerializeField]
        Animator m_src = default;

        [SerializeField]
        GameObject m_target = default;

        [SerializeField]
        GameObject Root = default;

        [SerializeField]
        TextAsset m_motion;

        private CancellationTokenSource _cancellationTokenSource;

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

            public void UpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta, Migration.Vrm0Meta meta0)
            {
                m_thumbnail.texture = thumbnail;

                if (meta != null)
                {
                    m_textModelTitle.text = meta.Name;
                    m_textModelVersion.text = meta.Version;
                    m_textModelAuthor.text = meta.Authors[0];
                    m_textModelContact.text = meta.ContactInformation;
                    if (meta.References != null && meta.References.Count > 0)
                    {
                        m_textModelReference.text = meta.References[0];
                    }
                    // m_textPermissionAllowed.text = meta.AllowedUser.ToString();
                    m_textPermissionViolent.text = meta.AllowExcessivelyViolentUsage.ToString();
                    m_textPermissionSexual.text = meta.AllowExcessivelySexualUsage.ToString();
                    m_textPermissionCommercial.text = meta.CommercialUsage.ToString();
                    // m_textPermissionOther.text = meta.OtherPermissionUrl;

                    // m_textDistributionLicense.text = meta.ModificationLicense.ToString();
                    m_textDistributionOther.text = meta.OtherLicenseUrl;
                }

                if (meta0 != null)
                {
                    m_textModelTitle.text = meta0.title;
                    m_textModelVersion.text = meta0.version;
                    m_textModelAuthor.text = meta0.author;
                    m_textModelContact.text = meta0.contactInformation;
                    m_textModelReference.text = meta0.reference;
                    m_textPermissionAllowed.text = meta0.allowedUser.ToString();
                    m_textPermissionViolent.text = meta0.violentUsage.ToString();
                    m_textPermissionSexual.text = meta0.sexualUsage.ToString();
                    m_textPermissionCommercial.text = meta0.commercialUsage.ToString();
                    m_textPermissionOther.text = meta0.otherPermissionUrl;
                    // m_textDistributionLicense.text = meta0.ModificationLicense.ToString();
                    m_textDistributionOther.text = meta0.otherLicenseUrl;
                }
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

            public bool IsBvhEnabled
            {
                get => ToggleMotion.ActiveToggles().FirstOrDefault() == ToggleMotionBVH;
                set
                {
                    ToggleMotionTPose.isOn = !value;
                    ToggleMotionBVH.isOn = value;
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

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");
            m_enableAutoExpression = toggles.First(x => x.name == "EnableAutoExpression");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");

            m_src = GameObject.FindObjectOfType<Animator>();

            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
        }

        class Loaded : IDisposable
        {
            RuntimeGltfInstance m_instance;
            Vrm10Instance m_controller;

            VRM10AIUEO m_lipSync;
            bool m_enableLipSyncValue;
            public bool EnableLipSyncValue
            {
                set
                {
                    if (m_enableLipSyncValue == value) return;
                    m_enableLipSyncValue = value;
                    if (m_lipSync != null)
                    {
                        m_lipSync.enabled = m_enableLipSyncValue;
                    }
                }
            }

            VRM10AutoExpression m_autoExpression;
            bool m_enableAutoExpressionValue;
            public bool EnableAutoExpressionValue
            {
                set
                {
                    if (m_enableAutoExpressionValue == value) return;
                    m_enableAutoExpressionValue = value;
                    if (m_autoExpression != null)
                    {
                        m_autoExpression.enabled = m_enableAutoExpressionValue;
                    }
                }
            }

            VRM10Blinker m_blink;
            bool m_enableBlinkValue;
            public bool EnableBlinkValue
            {
                set
                {
                    if (m_blink == value) return;
                    m_enableBlinkValue = value;
                    if (m_blink != null)
                    {
                        m_blink.enabled = m_enableBlinkValue;
                    }
                }
            }

            public Loaded(RuntimeGltfInstance instance, Transform lookAtTarget)
            {
                m_instance = instance;

                m_controller = instance.GetComponent<Vrm10Instance>();
                if (m_controller != null)
                {
                    // VRM
                    m_controller.UpdateType = Vrm10Instance.UpdateTypes.LateUpdate; // after HumanPoseTransfer's setPose
                    {
                        m_lipSync = instance.gameObject.AddComponent<VRM10AIUEO>();
                        m_blink = instance.gameObject.AddComponent<VRM10Blinker>();
                        m_autoExpression = instance.gameObject.AddComponent<VRM10AutoExpression>();

                        m_controller.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze;
                        m_controller.Gaze = lookAtTarget;
                    }
                }

                var animation = instance.GetComponent<Animation>();
                if (animation && animation.clip != null)
                {
                    // GLTF animation
                    animation.Play(animation.clip.name);
                }
            }

            public void Dispose()
            {
                // destroy GameObject
                GameObject.Destroy(m_instance.gameObject);
            }

            public void UpdatePose(bool useBvh, Animator bvhAnimator)
            {
                var controlRig = m_controller.Runtime.ControlRig;

                foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
                {
                    if (bone == HumanBodyBones.LastBone)
                    {
                        continue;
                    }

                    var controlRigBone = controlRig.GetBoneTransform(bone);
                    if (controlRigBone == null)
                    {
                        continue;
                    }

                    if (useBvh && bvhAnimator != null)
                    {
                        var bvhBone = bvhAnimator.GetBoneTransform(bone);
                        if (bvhBone != null)
                        {
                            // set normalized pose
                            controlRigBone.localRotation = bvhBone.localRotation;
                        }

                        if (bone == HumanBodyBones.Hips)
                        {
                            controlRigBone.position = bvhBone.position * controlRig.InitialHipsHeight;
                        }
                    }
                    else
                    {
                        controlRig.EnforceTPose();
                    }
                }
            }
        }
        Loaded m_loaded;

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_open.onClick.AddListener(OnOpenClicked);

            // load initial bvh
            if (m_motion != null)
            {
                LoadMotion(m_motion.text);
            }

            string[] cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                LoadModel(cmds[1]);
            }

            m_texts.Start();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Dispose();
        }

        private void LoadMotion(string source)
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse("tmp.bvh", source);
            context.Load();
            SetMotion(context.Root.GetComponent<Animator>());
        }

        private void Update()
        {
            if (m_loaded != null)
            {
                m_loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                m_loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                m_loaded.EnableAutoExpressionValue = m_enableAutoExpression.isOn;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
            }

            if (m_loaded != null)
            {
                m_loaded.UpdatePose(m_ui.IsBvhEnabled, m_src);
            }
        }

        void OnOpenClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", "vrm", "glb", "bvh", "gltf", "zip");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".gltf":
                case ".glb":
                case ".vrm":
                case ".zip":
                    LoadModel(path);
                    break;

                case ".bvh":
                    LoadMotion(path);
                    break;
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new Vrm10UrpMaterialDescriptorGenerator();
            }
            else
            {
                return new Vrm10MaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(bool useUrp)
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

        async void LoadModel(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                Debug.LogFormat("{0}", path);
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: m_useAsync.enabled ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller(),
                    materialGenerator: GetVrmMaterialDescriptorGenerator(m_useUrpMaterial.isOn),
                    vrmMetaInformationCallback: m_texts.UpdateMeta,
                    ct: cancellationToken);
                if (vrm10Instance != null)
                {
                    // test. error にならなければよい
                    vrm10Instance.Runtime.Expression.SetWeight(ExpressionKey.Aa, 0);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        UnityObjectDestoyer.DestroyRuntimeOrEditor(vrm10Instance.gameObject);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    SetModel(vrm10Instance.GetComponent<RuntimeGltfInstance>());
                }
                else
                {
                    // NOTE: load as glTF model if failed to load as VRM 1.0.
                    // TODO: Hand over CancellationToken
                    var gltfModel = await GltfUtility.LoadAsync(path,
                    awaitCaller: m_useAsync.enabled ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller());
                    if (gltfModel == null)
                    {
                        throw new Exception("Failed to load the file as glTF format.");
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        gltfModel.Dispose();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    SetModel(gltfModel);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.LogWarning($"Canceled to Load: {path}");
                }
                else
                {
                    Debug.LogError($"Failed to Load: {path}");
                    Debug.LogException(ex);
                }
            }
        }

        void SetModel(RuntimeGltfInstance instance)
        {
            // cleanup
            if (m_loaded != null)
            {
                m_loaded.Dispose();
                m_loaded = null;
            }

            instance.ShowMeshes();
            instance.EnableUpdateWhenOffscreen();
            m_loaded = new Loaded(instance, m_target.transform);
        }

        void SetMotion(Animator src)
        {
            m_src = src;
            m_ui.IsBvhEnabled = true;
        }
    }
}

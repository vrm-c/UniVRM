using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerUI : MonoBehaviour
    {
        [SerializeField]
        GameObject Root = default;
        [SerializeField]
        Text m_version = default;
        [SerializeField]
        Transform m_faceCamera = default;

        [Header("Material")]
        [SerializeField]
        Material m_pbrOpaqueMaterial = default;
        [SerializeField]
        Material m_pbrAlphaBlendMaterial = default;
        [SerializeField]
        Material m_mtoonMaterialOpaque = default;
        [SerializeField]
        Material m_mtoonMaterialAlphaBlend = default;

        [Header("UI")]
        [SerializeField]
        Toggle m_useCustomPbrMaterial = default;
        [SerializeField]
        Toggle m_useCustomMToonMaterial = default;

        [SerializeField]
        Button m_openModel = default;

        [SerializeField]
        Button m_openMotion = default;

        [SerializeField]
        Button m_pastePose = default;

        [SerializeField]
        Toggle m_showBoxMan = default;

        [SerializeField]
        Toggle m_useAsync = default;

        [SerializeField]
        TextAsset m_motion;

        [SerializeField, Header("springbone")]
        Toggle m_useSpringboneSingelton = default;
        [SerializeField]
        Toggle m_springbonePause = default;
        [SerializeField]
        Toggle m_springboneScaling = default;
        [SerializeField]
        Slider m_springboneExternalX = default;
        [SerializeField]
        Slider m_springboneExternalY = default;
        [SerializeField]
        Slider m_springboneExternalZ = default;

        [SerializeField]
        Button m_resetSpringBone = default;
        [SerializeField]
        Button m_reconstructSpringBone = default;

        [SerializeField, Header("expression")]
        Toggle m_enableAutoExpression = default;
        [Serializable]
        class EmotionFields
        {
            public Slider m_expression;
            public Toggle m_binary;
            public bool m_useOverride;
            public Dropdown m_overrideMouth;
            public Dropdown m_overrideBlink;
            public Dropdown m_overrideLookAt;

            public void Reset(ObjectMap map, string name, bool useOveride)
            {
                m_expression = map.Get<Slider>($"Slider{name}");
                m_binary = map.Get<Toggle>($"Binary{name}");
                m_useOverride = useOveride;
                if (useOveride)
                {
                    m_overrideMouth = map.Get<Dropdown>($"Override{name}Mouth");
                    m_overrideBlink = map.Get<Dropdown>($"Override{name}Blink");
                    m_overrideLookAt = map.Get<Dropdown>($"Override{name}LookAt");
                }
            }

            static int GetOverrideIndex(UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType value)
            {
                switch (value)
                {
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none: return 0;
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block: return 1;
                    case UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.blend: return 2;
                    default: return -1;
                }
            }

            static UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType ToOverrideType(int index)
            {
                switch (index)
                {
                    case 0: return UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.none;
                    case 1: return UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.block;
                    case 2: return UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType.blend;
                    default: throw new ArgumentException();
                }
            }

            public void OnLoad(VRM10Expression expression)
            {
                m_binary.isOn = expression.IsBinary;
                if (m_useOverride)
                {
                    m_overrideMouth.SetValueWithoutNotify(GetOverrideIndex(expression.OverrideMouth));
                    m_overrideBlink.SetValueWithoutNotify(GetOverrideIndex(expression.OverrideBlink));
                    m_overrideLookAt.SetValueWithoutNotify(GetOverrideIndex(expression.OverrideLookAt));
                }
            }

            public void ApplyRuntime(VRM10Expression expression)
            {
                expression.IsBinary = m_binary.isOn;
                if (m_useOverride)
                {
                    expression.OverrideMouth = ToOverrideType(m_overrideMouth.value);
                    expression.OverrideBlink = ToOverrideType(m_overrideBlink.value);
                    expression.OverrideLookAt = ToOverrideType(m_overrideLookAt.value);
                }
            }
        }
        [SerializeField]
        EmotionFields m_happy;
        [SerializeField]
        EmotionFields m_angry;
        [SerializeField]
        EmotionFields m_sad;
        [SerializeField]
        EmotionFields m_relaxed;
        [SerializeField]
        EmotionFields m_surprised;

        [SerializeField]
        Toggle m_enableLipSync = default;
        [SerializeField]
        EmotionFields m_lipAa = default;
        [SerializeField]
        EmotionFields m_lipIh = default;
        [SerializeField]
        EmotionFields m_lipOu = default;
        [SerializeField]
        EmotionFields m_lipEe = default;
        [SerializeField]
        EmotionFields m_lipOh = default;

        [SerializeField]
        Toggle m_enableAutoBlink = default;
        [SerializeField]
        EmotionFields m_blink = default;

        [SerializeField]
        GameObject m_lookAtTarget = default;
        [SerializeField]
        Toggle m_useLookAtTarget = default;
        [SerializeField]
        Slider m_yaw = default;
        [SerializeField]
        Slider m_pitch = default;

        [Serializable]
        class TextFields
        {
            [SerializeField]
            Text m_textModelTitle = default;
            [SerializeField]
            Text m_textModelVersion = default;
            [SerializeField]
            Text m_textModelAuthor = default;
            [SerializeField]
            Text m_textModelCopyright = default;
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

            public void Reset(ObjectMap map)
            {
                m_textModelTitle = map.Get<Text>("Title (1)");
                m_textModelVersion = map.Get<Text>("Version (1)");
                m_textModelAuthor = map.Get<Text>("Author (1)");
                m_textModelCopyright = map.Get<Text>("Copyright (1)");
                m_textModelContact = map.Get<Text>("Contact (1)");
                m_textModelReference = map.Get<Text>("Reference (1)");

                m_textPermissionAllowed = map.Get<Text>("AllowedUser (1)");
                m_textPermissionViolent = map.Get<Text>("Violent (1)");
                m_textPermissionSexual = map.Get<Text>("Sexual (1)");
                m_textPermissionCommercial = map.Get<Text>("Commercial (1)");
                m_textPermissionOther = map.Get<Text>("Other (1)");

                m_textDistributionLicense = map.Get<Text>("LicenseType (1)");
                m_textDistributionOther = map.Get<Text>("OtherLicense (1)");

#if UNITY_2022_3_OR_NEWER
                var images = GameObject.FindObjectsByType<RawImage>(FindObjectsSortMode.InstanceID);
#else
                var images = GameObject.FindObjectsOfType<RawImage>();
#endif
                m_thumbnail = images.First(x => x.name == "RawImage");
            }

            public void Start()
            {
                m_textModelTitle.text = "";
                m_textModelVersion.text = "";
                m_textModelAuthor.text = "";
                m_textModelCopyright.text = "";
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
                    m_textModelCopyright.text = meta.CopyrightInformation;
                    m_textModelContact.text = meta.ContactInformation;
                    if (meta.References != null && meta.References.Count > 0)
                    {
                        m_textModelReference.text = meta.References[0];
                    }
                    m_textPermissionAllowed.text = meta.AvatarPermission.ToString();
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

            public void Reset(ObjectMap map)
            {
                ToggleMotionTPose = map.Get<Toggle>("TPose");
                ToggleMotionBVH = map.Get<Toggle>("BVH");
                ToggleMotion = map.Get<ToggleGroup>("_Motion_");
            }

            public bool IsTPose
            {
                get => ToggleMotion.ActiveToggles().FirstOrDefault() == ToggleMotionTPose;
                set
                {
                    ToggleMotionTPose.isOn = value;
                    ToggleMotionBVH.isOn = !value;
                }
            }
        }
        [SerializeField]
        UIFields m_ui = default;

        class ObjectMap
        {
            Dictionary<string, GameObject> _map = new();
            public IReadOnlyDictionary<string, GameObject> Objects => _map;

            public ObjectMap(GameObject root)
            {
                foreach (var x in root.GetComponentsInChildren<Transform>())
                {
                    _map[x.name] = x.gameObject;
                }
            }

            public T Get<T>(string name) where T : Component
            {
                return _map[name].GetComponent<T>();
            }
        }

        private void Reset()
        {
            var map = new ObjectMap(gameObject);
            Root = map.Objects["Root"];
            m_useCustomPbrMaterial = map.Get<Toggle>("CustomPbrMaterial");
            m_useCustomMToonMaterial = map.Get<Toggle>("CustomMToonMaterial");
            m_openModel = map.Get<Button>("OpenModel");
            m_openMotion = map.Get<Button>("OpenMotion");
            m_pastePose = map.Get<Button>("PastePose");
            m_showBoxMan = map.Get<Toggle>("ShowBoxMan");
            m_useAsync = map.Get<Toggle>("UseAsync");
            m_useSpringboneSingelton = map.Get<Toggle>("UseSingleton");
            m_springbonePause = map.Get<Toggle>("PauseSpringBone");
            m_resetSpringBone = map.Get<Button>("ResetSpringBone");
            m_reconstructSpringBone = map.Get<Button>("ReconstructSpringBone");
            m_version = map.Get<Text>("VrmVersion");

            m_texts.Reset(map);
            m_ui.Reset(map);
            m_springboneScaling = map.Get<Toggle>("ScalingSpringBone");
            m_springboneExternalX = map.Get<Slider>("SliderExternalX");
            m_springboneExternalY = map.Get<Slider>("SliderExternalY");
            m_springboneExternalZ = map.Get<Slider>("SliderExternalZ");
            m_enableAutoExpression = map.Get<Toggle>("EnableAutoExpression");
            m_happy.Reset(map, "Happy", true);
            m_angry.Reset(map, "Angry", true);
            m_sad.Reset(map, "Sad", true);
            m_relaxed.Reset(map, "Relaxed", true);
            m_surprised.Reset(map, "Surprised", true);

            m_enableLipSync = map.Get<Toggle>("EnableLipSync");
            m_lipAa.Reset(map, "Aa", false);
            m_lipIh.Reset(map, "Ih", false);
            m_lipOu.Reset(map, "Ou", false);
            m_lipEe.Reset(map, "Ee", false);
            m_lipOh.Reset(map, "Oh", false);

            m_enableAutoBlink = map.Get<Toggle>("EnableAutoBlink");
            m_blink.Reset(map, "Blink", false);

            m_useLookAtTarget = map.Get<Toggle>("UseLookAtTarget");
            m_yaw = map.Get<Slider>("SliderYaw");
            m_pitch = map.Get<Slider>("SliderPitch");

#if UNITY_2022_3_OR_NEWER
            m_lookAtTarget = GameObject.FindFirstObjectByType<VRM10TargetMover>().gameObject;
#else
            m_lookAtTarget = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
#endif
        }

        static class ArgumentChecker
        {
            static string[] Supported = {
                ".gltf",
                ".glb",
                ".vrm",
                ".zip",
            };

            static string UnityHubPath => System.Environment.GetEnvironmentVariable("ProgramFiles") + "\\Unity\\Hub";

            public static bool IsLoadable(string path)
            {
                if (!File.Exists(path))
                {
                    // not exists
                    return false;
                }

                if (Application.isEditor)
                {
                    // skip editor argument
                    // {UnityHub_Resources}\PackageManager\ProjectTemplates\com.unity.template.3d-5.0.4.tgz
                    if (path.StartsWith(UnityHubPath))
                    {
                        return false;
                    }
                }

                var ext = Path.GetExtension(path).ToLower();
                if (!Supported.Contains(ext))
                {
                    // unknown extension
                    return false;
                }

                return true;
            }

            public static bool TryGetFirstLoadable(out string cmd)
            {
                foreach (var arg in System.Environment.GetCommandLineArgs())
                {
                    if (ArgumentChecker.IsLoadable(arg))
                    {
                        cmd = arg;
                        return true;
                    }
                }

                cmd = default;
                return false;
            }
        }

        VRM10ViewerController m_controller;
        VRM10AutoExpression m_autoEmotion;
        VRM10Blinker m_autoBlink;
        VRM10AIUEO m_autoLipsync;

        void OnLoaded(Loaded loaded)
        {
            m_showBoxMan.isOn = false;
            m_happy.OnLoad(loaded.Instance.Vrm.Expression.Happy);
            m_angry.OnLoad(loaded.Instance.Vrm.Expression.Angry);
            m_sad.OnLoad(loaded.Instance.Vrm.Expression.Sad);
            m_relaxed.OnLoad(loaded.Instance.Vrm.Expression.Relaxed);
            m_surprised.OnLoad(loaded.Instance.Vrm.Expression.Surprised);
            m_lipAa.OnLoad(loaded.Instance.Vrm.Expression.Aa);
            m_lipIh.OnLoad(loaded.Instance.Vrm.Expression.Ih);
            m_lipOu.OnLoad(loaded.Instance.Vrm.Expression.Ou);
            m_lipEe.OnLoad(loaded.Instance.Vrm.Expression.Ee);
            m_lipOh.OnLoad(loaded.Instance.Vrm.Expression.Oh);
            m_blink.OnLoad(loaded.Instance.Vrm.Expression.Blink);
        }

        LoadOptions MakeLoadOptions()
        {
            return new LoadOptions()
            {
                UseAsync = m_useAsync.isOn,
                UseSpringboneSingelton = m_useSpringboneSingelton.isOn,
                UseCustomPbrMaterial = m_useCustomPbrMaterial.isOn,
                UseCustomMToonMaterial = m_useCustomMToonMaterial.isOn,
            };
        }

        private void Start()
        {
            m_controller = new(
                (m_mtoonMaterialOpaque != null && m_mtoonMaterialAlphaBlend != null) ? new TinyMToonrMaterialImporter(m_mtoonMaterialOpaque, m_mtoonMaterialAlphaBlend) : null,
                (m_pbrOpaqueMaterial != null && m_pbrAlphaBlendMaterial != null) ? new TinyPbrMaterialImporter(m_pbrOpaqueMaterial, m_pbrAlphaBlendMaterial) : null
                );
            m_controller.OnUpdateMeta += m_texts.UpdateMeta;
            m_controller.OnLoaded += OnLoaded;

            // URP かつ WebGL で有効にする
            m_useCustomMToonMaterial.isOn = Application.platform == RuntimePlatform.WebGLPlayer && GraphicsSettings.renderPipelineAsset != null;

            m_autoEmotion = gameObject.AddComponent<VRM10AutoExpression>();
            m_autoBlink = gameObject.AddComponent<VRM10Blinker>();
            m_autoLipsync = gameObject.AddComponent<VRM10AIUEO>();

            m_version.text = string.Format("VRM10ViewerUI {0}", PackageVersion.VERSION);

            m_openModel.onClick.AddListener(() => m_controller.OnOpenModelClicked(MakeLoadOptions()));
            m_openMotion.onClick.AddListener(m_controller.OnOpenMotionClicked);
            m_pastePose.onClick.AddListener(m_controller.OnPastePoseClicked);
            m_resetSpringBone.onClick.AddListener(m_controller.OnResetSpringBoneClicked);
            m_reconstructSpringBone.onClick.AddListener(m_controller.OnReconstructSpringBoneClicked);

            // load initial bvh
            if (m_motion != null)
            {
                m_controller.Motion = BvhMotion.LoadBvhFromText(m_motion.text);
                if (GraphicsSettings.renderPipelineAsset != null
                    && m_pbrAlphaBlendMaterial != null)
                {
                    m_controller.Motion.SetBoxManMaterial(Instantiate(m_pbrOpaqueMaterial));
                }
            }

            if (ArgumentChecker.TryGetFirstLoadable(out var cmd))
            {
                var _ = m_controller.LoadModelPath(cmd, MakeLoadOptions());
            }

            m_texts.Start();
        }

        private void OnDestroy()
        {
            m_controller.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                m_controller.Cancel();
            }

            m_controller.ShowBoxMan(m_showBoxMan.isOn);
            if (m_controller.TryUpdate(
                m_ui.IsTPose,
                new BlittableModelLevel
                {
                    ExternalForce = new Vector3(m_springboneExternalX.value, m_springboneExternalY.value, m_springboneExternalZ.value),
                    StopSpringBoneWriteback = m_springbonePause.isOn,
                    SupportsScalingAtRuntime = m_springboneScaling.isOn,
                },
                out var loaded
            ))
            {
                m_happy.ApplyRuntime(loaded.Instance.Vrm.Expression.Happy);
                m_angry.ApplyRuntime(loaded.Instance.Vrm.Expression.Angry);
                m_sad.ApplyRuntime(loaded.Instance.Vrm.Expression.Sad);
                m_relaxed.ApplyRuntime(loaded.Instance.Vrm.Expression.Relaxed);
                m_surprised.ApplyRuntime(loaded.Instance.Vrm.Expression.Surprised);
                m_lipAa.ApplyRuntime(loaded.Instance.Vrm.Expression.Aa);
                m_lipIh.ApplyRuntime(loaded.Instance.Vrm.Expression.Ih);
                m_lipOu.ApplyRuntime(loaded.Instance.Vrm.Expression.Ou);
                m_lipEe.ApplyRuntime(loaded.Instance.Vrm.Expression.Ee);
                m_lipOh.ApplyRuntime(loaded.Instance.Vrm.Expression.Oh);
                m_blink.ApplyRuntime(loaded.Instance.Vrm.Expression.Blink);

                var vrm = loaded.Instance;
                if (m_enableAutoExpression.isOn)
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Happy, m_autoEmotion.Happy);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Angry, m_autoEmotion.Angry);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Sad, m_autoEmotion.Sad);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, m_autoEmotion.Relaxed);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Surprised, m_autoEmotion.Surprised);
                    m_happy.m_expression.SetValueWithoutNotify(m_autoEmotion.Happy);
                    m_angry.m_expression.SetValueWithoutNotify(m_autoEmotion.Angry);
                    m_sad.m_expression.SetValueWithoutNotify(m_autoEmotion.Sad);
                    m_relaxed.m_expression.SetValueWithoutNotify(m_autoEmotion.Relaxed);
                    m_surprised.m_expression.SetValueWithoutNotify(m_autoEmotion.Surprised);
                }
                else
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Happy, m_happy.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Angry, m_angry.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Sad, m_sad.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, m_relaxed.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Surprised, m_surprised.m_expression.value);
                }

                if (m_enableLipSync.isOn)
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Aa, m_autoLipsync.Aa);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ih, m_autoLipsync.Ih);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ou, m_autoLipsync.Ou);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ee, m_autoLipsync.Ee);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Oh, m_autoLipsync.Oh);
                    m_lipAa.m_expression.SetValueWithoutNotify(m_autoLipsync.Aa);
                    m_lipIh.m_expression.SetValueWithoutNotify(m_autoLipsync.Ih);
                    m_lipOu.m_expression.SetValueWithoutNotify(m_autoLipsync.Ou);
                    m_lipEe.m_expression.SetValueWithoutNotify(m_autoLipsync.Ee);
                    m_lipOh.m_expression.SetValueWithoutNotify(m_autoLipsync.Oh);
                }
                else
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Aa, m_lipAa.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ih, m_lipIh.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ou, m_lipOu.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ee, m_lipEe.m_expression.value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Oh, m_lipOh.m_expression.value);
                }

                if (m_enableAutoBlink.isOn)
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Blink, m_autoBlink.BlinkValue);
                    m_blink.m_expression.SetValueWithoutNotify(m_autoBlink.BlinkValue);
                }
                else
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Blink, m_blink.m_expression.value);
                }

                if (m_useLookAtTarget.isOn)
                {
                    var (yaw, pitch) = vrm.Runtime.LookAt.CalculateYawPitchFromLookAtPosition(m_lookAtTarget.transform.position);
                    vrm.Runtime.LookAt.SetYawPitchManually(yaw, pitch);
                    m_yaw.value = yaw;
                    m_pitch.value = pitch;
                }
                else
                {
                    vrm.Runtime.LookAt.SetYawPitchManually(m_yaw.value, m_pitch.value);
                }

                if (vrm.TryGetBoneTransform(HumanBodyBones.Head, out var head))
                {
                    var initLocarlRotation = vrm.DefaultTransformStates[head].LocalRotation;
                    var r = head.rotation * Quaternion.Inverse(initLocarlRotation);
                    var pos = head.position
                        + (r * Vector3.forward * 0.7f)
                        + (r * Vector3.up * 0.07f)
                        ;
                    m_faceCamera.position = pos;
                    m_faceCamera.rotation = r;
                }
            }
        }

        /// <summary>
        /// for WebGL
        /// call from OpenFile.jslib
        /// </summary>
        public void FileSelected(string url)
        {
            UniGLTFLogger.Log($"FileSelected: {url}");
            StartCoroutine(m_controller.LoadCoroutine(url, MakeLoadOptions()));
        }
    }
}
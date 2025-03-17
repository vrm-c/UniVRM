using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


namespace UniVRM10.VRM10Viewer
{
    public class VRM10MainView : MonoBehaviour
    {
        [SerializeField]
        TextAsset m_motion;

        [Header("Material")]
        [SerializeField]
        Material m_pbrOpaqueMaterial = default;
        [SerializeField]
        Material m_pbrAlphaBlendMaterial = default;
        [SerializeField]
        Material m_mtoonMaterialOpaque = default;
        [SerializeField]
        Material m_mtoonMaterialAlphaBlend = default;

        VRM10ViewerController m_controller;
        Toggle m_useAsync;
        Toggle m_useSpringboneSingleton;
        Toggle m_useCustomPbrMaterial;
        Toggle m_useCustomMToonMaterial;
        RadioButtonGroup m_motionMode;
        Slider m_springboneExternalX;
        Slider m_springboneExternalY;
        Slider m_springboneExternalZ;
        Toggle m_useSpringbonePause;
        Toggle m_useSpringboneScaling;
        Toggle m_showBoxMan;

        void QueryOrAssert<T>(out T dst, VisualElement root, string name) where T : VisualElement
        {
            dst = root.Q<T>(name);
            Debug.Assert(dst != null, name);
        }
        void OnEnable()
        {
            m_controller = new VRM10ViewerController(
                (m_mtoonMaterialOpaque != null && m_mtoonMaterialAlphaBlend != null) ? new TinyMToonrMaterialImporter(m_mtoonMaterialOpaque, m_mtoonMaterialAlphaBlend) : null,
                (m_pbrOpaqueMaterial != null && m_pbrAlphaBlendMaterial != null) ? new TinyPbrMaterialImporter(m_pbrOpaqueMaterial, m_pbrAlphaBlendMaterial) : null
            );
            m_controller.OnUpdateMeta += UpdateMeta;
            m_controller.OnLoaded += OnLoaded;

            // The UXML is already instantiated by the UIDocument component
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            var openModelButton = root.Q<Button>("OpenModel");
            openModelButton.clicked += () =>
            {
                m_controller.OnOpenModelClicked(MakeLoadOptions());
            };
            QueryOrAssert(out m_useAsync, root, "UseAsync");
            QueryOrAssert(out m_useSpringboneSingleton, root, "UseSpringboneSingleton");
            QueryOrAssert(out m_useCustomPbrMaterial, root, "UseCustomPbrMaterial");
            QueryOrAssert(out m_useCustomMToonMaterial, root, "UseCustomMToonMaterial");
            // URP かつ WebGL で有効にする
            m_useCustomMToonMaterial.value = Application.platform == RuntimePlatform.WebGLPlayer && GraphicsSettings.renderPipelineAsset != null;
            QueryOrAssert(out m_motionMode, root, "MotionMode");
            QueryOrAssert(out m_springboneExternalX, root, "SpringboneExternalX");
            QueryOrAssert(out m_springboneExternalY, root, "SpringboneExternalY");
            QueryOrAssert(out m_springboneExternalZ, root, "SpringboneExternalZ");
            QueryOrAssert(out m_useSpringbonePause, root, "UeSpringbonePause");
            QueryOrAssert(out m_useSpringboneScaling, root, "UseSpringboneScaling");
            QueryOrAssert(out m_showBoxMan, root, "ShowBoxMan");

            // m_autoEmotion = gameObject.AddComponent<VRM10AutoExpression>();
            // m_autoBlink = gameObject.AddComponent<VRM10Blinker>();
            // m_autoLipsync = gameObject.AddComponent<VRM10AIUEO>();

            // m_version.text = string.Format("VRM10ViewerUI {0}", PackageVersion.VERSION);

            // m_openModel.onClick.AddListener(() => m_controller.OnOpenModelClicked(MakeLoadOptions()));
            // m_openMotion.onClick.AddListener(m_controller.OnOpenMotionClicked);
            // m_pastePose.onClick.AddListener(m_controller.OnPastePoseClicked);
            // m_resetSpringBone.onClick.AddListener(m_controller.OnResetSpringBoneClicked);
            // m_reconstructSpringBone.onClick.AddListener(m_controller.OnReconstructSpringBoneClicked);

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

            // m_texts.Start();
        }

        void OnDisable()
        {
            m_controller.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                // if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                m_controller.Cancel();
            }

            m_controller.ShowBoxMan(m_showBoxMan.value);
            if (m_controller.TryUpdate(
                m_motionMode.value == 0,
                new BlittableModelLevel
                {
                    ExternalForce = new Vector3(
                        m_springboneExternalX.value,
                        m_springboneExternalY.value,
                        m_springboneExternalZ.value),
                    StopSpringBoneWriteback = m_useSpringbonePause.value,
                    SupportsScalingAtRuntime = m_useSpringboneScaling.value,
                },
                out var loaded
            ))
            {
                // m_happy.ApplyRuntime(loaded.Instance.Vrm.Expression.Happy);
                // m_angry.ApplyRuntime(loaded.Instance.Vrm.Expression.Angry);
                // m_sad.ApplyRuntime(loaded.Instance.Vrm.Expression.Sad);
                // m_relaxed.ApplyRuntime(loaded.Instance.Vrm.Expression.Relaxed);
                // m_surprised.ApplyRuntime(loaded.Instance.Vrm.Expression.Surprised);
                // m_lipAa.ApplyRuntime(loaded.Instance.Vrm.Expression.Aa);
                // m_lipIh.ApplyRuntime(loaded.Instance.Vrm.Expression.Ih);
                // m_lipOu.ApplyRuntime(loaded.Instance.Vrm.Expression.Ou);
                // m_lipEe.ApplyRuntime(loaded.Instance.Vrm.Expression.Ee);
                // m_lipOh.ApplyRuntime(loaded.Instance.Vrm.Expression.Oh);
                // m_blink.ApplyRuntime(loaded.Instance.Vrm.Expression.Blink);

                // var vrm = loaded.Instance;
                // if (m_enableAutoExpression.isOn)
                // {
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Happy, m_autoEmotion.Happy);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Angry, m_autoEmotion.Angry);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Sad, m_autoEmotion.Sad);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, m_autoEmotion.Relaxed);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Surprised, m_autoEmotion.Surprised);
                //     m_happy.m_expression.SetValueWithoutNotify(m_autoEmotion.Happy);
                //     m_angry.m_expression.SetValueWithoutNotify(m_autoEmotion.Angry);
                //     m_sad.m_expression.SetValueWithoutNotify(m_autoEmotion.Sad);
                //     m_relaxed.m_expression.SetValueWithoutNotify(m_autoEmotion.Relaxed);
                //     m_surprised.m_expression.SetValueWithoutNotify(m_autoEmotion.Surprised);
                // }
                // else
                // {
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Happy, m_happy.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Angry, m_angry.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Sad, m_sad.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, m_relaxed.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Surprised, m_surprised.m_expression.value);
                // }

                // if (m_enableLipSync.isOn)
                // {
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Aa, m_autoLipsync.Aa);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Ih, m_autoLipsync.Ih);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Ou, m_autoLipsync.Ou);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Ee, m_autoLipsync.Ee);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Oh, m_autoLipsync.Oh);
                //     m_lipAa.m_expression.SetValueWithoutNotify(m_autoLipsync.Aa);
                //     m_lipIh.m_expression.SetValueWithoutNotify(m_autoLipsync.Ih);
                //     m_lipOu.m_expression.SetValueWithoutNotify(m_autoLipsync.Ou);
                //     m_lipEe.m_expression.SetValueWithoutNotify(m_autoLipsync.Ee);
                //     m_lipOh.m_expression.SetValueWithoutNotify(m_autoLipsync.Oh);
                // }
                // else
                // {
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Aa, m_lipAa.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Ih, m_lipIh.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Ou, m_lipOu.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Ee, m_lipEe.m_expression.value);
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Oh, m_lipOh.m_expression.value);
                // }

                // if (m_enableAutoBlink.isOn)
                // {
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Blink, m_autoBlink.BlinkValue);
                //     m_blink.m_expression.SetValueWithoutNotify(m_autoBlink.BlinkValue);
                // }
                // else
                // {
                //     vrm.Runtime.Expression.SetWeight(ExpressionKey.Blink, m_blink.m_expression.value);
                // }

                // if (m_useLookAtTarget.isOn)
                // {
                //     var (yaw, pitch) = vrm.Runtime.LookAt.CalculateYawPitchFromLookAtPosition(m_lookAtTarget.transform.position);
                //     vrm.Runtime.LookAt.SetYawPitchManually(yaw, pitch);
                //     m_yaw.value = yaw;
                //     m_pitch.value = pitch;
                // }
                // else
                // {
                //     vrm.Runtime.LookAt.SetYawPitchManually(m_yaw.value, m_pitch.value);
                // }

                // if (vrm.TryGetBoneTransform(HumanBodyBones.Head, out var head))
                // {
                //     var initLocarlRotation = vrm.DefaultTransformStates[head].LocalRotation;
                //     var r = head.rotation * Quaternion.Inverse(initLocarlRotation);
                //     var pos = head.position
                //         + (r * Vector3.forward * 0.7f)
                //         + (r * Vector3.up * 0.07f)
                //         ;
                //     m_faceCamera.position = pos;
                //     m_faceCamera.rotation = r;
                // }
            }
        }

        LoadOptions MakeLoadOptions()
        {
            return new LoadOptions()
            {
                UseAsync = m_useAsync.value,
                UseSpringboneSingelton = m_useSpringboneSingleton.value,
                UseCustomPbrMaterial = m_useCustomPbrMaterial.value,
                UseCustomMToonMaterial = m_useCustomMToonMaterial.value,
            };
        }

        void OnLoaded(Loaded loaded)
        {
            m_showBoxMan.value = false;
            // m_happy.OnLoad(loaded.Instance.Vrm.Expression.Happy);
            // m_angry.OnLoad(loaded.Instance.Vrm.Expression.Angry);
            // m_sad.OnLoad(loaded.Instance.Vrm.Expression.Sad);
            // m_relaxed.OnLoad(loaded.Instance.Vrm.Expression.Relaxed);
            // m_surprised.OnLoad(loaded.Instance.Vrm.Expression.Surprised);
            // m_lipAa.OnLoad(loaded.Instance.Vrm.Expression.Aa);
            // m_lipIh.OnLoad(loaded.Instance.Vrm.Expression.Ih);
            // m_lipOu.OnLoad(loaded.Instance.Vrm.Expression.Ou);
            // m_lipEe.OnLoad(loaded.Instance.Vrm.Expression.Ee);
            // m_lipOh.OnLoad(loaded.Instance.Vrm.Expression.Oh);
            // m_blink.OnLoad(loaded.Instance.Vrm.Expression.Blink);
        }

        public void UpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta, Migration.Vrm0Meta meta0)
        {
            // m_thumbnail.texture = thumbnail;

            if (meta != null)
            {
                m_textModelTitle.text = meta.Name;
                // m_textModelVersion.text = meta.Version;
                // m_textModelAuthor.text = meta.Authors[0];
                // m_textModelCopyright.text = meta.CopyrightInformation;
                // m_textModelContact.text = meta.ContactInformation;
                // if (meta.References != null && meta.References.Count > 0)
                // {
                //     m_textModelReference.text = meta.References[0];
                // }
                // m_textPermissionAllowed.text = meta.AvatarPermission.ToString();
                // m_textPermissionViolent.text = meta.AllowExcessivelyViolentUsage.ToString();
                // m_textPermissionSexual.text = meta.AllowExcessivelySexualUsage.ToString();
                // m_textPermissionCommercial.text = meta.CommercialUsage.ToString();
                // // m_textPermissionOther.text = meta.OtherPermissionUrl;

                // // m_textDistributionLicense.text = meta.ModificationLicense.ToString();
                // m_textDistributionOther.text = meta.OtherLicenseUrl;
            }

            if (meta0 != null)
            {
                // m_textModelTitle.text = meta0.title;
                // m_textModelVersion.text = meta0.version;
                // m_textModelAuthor.text = meta0.author;
                // m_textModelContact.text = meta0.contactInformation;
                // m_textModelReference.text = meta0.reference;
                // m_textPermissionAllowed.text = meta0.allowedUser.ToString();
                // m_textPermissionViolent.text = meta0.violentUsage.ToString();
                // m_textPermissionSexual.text = meta0.sexualUsage.ToString();
                // m_textPermissionCommercial.text = meta0.commercialUsage.ToString();
                // m_textPermissionOther.text = meta0.otherPermissionUrl;
                // // m_textDistributionLicense.text = meta0.ModificationLicense.ToString();
                // m_textDistributionOther.text = meta0.otherLicenseUrl;
            }
        }
    }
}
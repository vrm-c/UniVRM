using System;
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
        VRM10ViewerController m_controller = new();

        VisualElement m_root;
        // left
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
        Toggle m_enableAutoExpression;
        EmotionElement m_happy;
        EmotionElement m_angry;
        EmotionElement m_sad;
        EmotionElement m_relaxed;
        EmotionElement m_surprised;
        Toggle m_enableAutoLipsync;
        ExpressionElement m_lipAa;
        ExpressionElement m_lipIh;
        ExpressionElement m_lipOu;
        ExpressionElement m_lipEe;
        ExpressionElement m_lipOh;
        Toggle m_enableAutoBlink;
        ExpressionElement m_blink;
        Toggle m_enableLookatTarget;
        Slider m_yaw;
        Slider m_pitch;

        // right
        TextField m_metaName;
        TextField m_metaVersion;
        TextField m_metaAuthor;
        TextField m_metaCopyright;
        TextField m_metaContact;
        TextField m_metaReference;
        TextureElement m_metaThumbnail;
        TextField m_metaPermissionAllowed;
        TextField m_metaPermissionViolent;
        TextField m_metaPermissionSexual;
        TextField m_metaPermissionCommercial;
        TextField m_metaLicense;
        TextField m_metaDistribution;

        // runtime
        VRM10AutoExpression m_autoEmotion;
        VRM10Blinker m_autoBlink;
        VRM10AIUEO m_autoLipsync;

        void QueryOrAssert<T>(out T dst, VisualElement root, string name) where T : VisualElement
        {
            dst = root.Q<T>(name);
            Debug.Assert(dst != null, name);
        }
        void OnEnable()
        {
            // The UXML is already instantiated by the UIDocument component
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
            m_root = root;

            root.Q<Button>("OpenModel").clicked += () =>
            {
                m_controller.OnOpenModelClicked(MakeLoadOptions(), name, nameof(FileSelected));
            };

            root.Q<Button>("OpenMotion").clicked += () =>
            {
                m_controller.OnOpenMotionClicked();
            };

            root.Q<Button>("PastePose").clicked += () =>
            {
                m_controller.OnPastePoseClicked();
            };

            root.Q<Button>("ResetSpring").clicked += () =>
            {
                m_controller.OnResetSpringBoneClicked();
            };

            root.Q<Button>("ReconstructSpring").clicked += () =>
            {
                m_controller.OnReconstructSpringBoneClicked();
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
            QueryOrAssert(out m_enableAutoExpression, root, "AutoEmotions");
            m_happy = root.Q("Happy").Q<EmotionElement>().Init();
            m_angry = root.Q("Angry").Q<EmotionElement>().Init();
            m_sad = root.Q("Sad").Q<EmotionElement>().Init();
            m_relaxed = root.Q("Relaxed").Q<EmotionElement>().Init();
            m_surprised = root.Q("Surprised").Q<EmotionElement>().Init();
            QueryOrAssert(out m_enableAutoLipsync, root, "AutoLipsync");
            m_lipAa = root.Q("Aa").Q<ExpressionElement>().Init();
            m_lipIh = root.Q("Ih").Q<ExpressionElement>().Init();
            m_lipOu = root.Q("Ou").Q<ExpressionElement>().Init();
            m_lipEe = root.Q("Ee").Q<ExpressionElement>().Init();
            m_lipOh = root.Q("Oh").Q<ExpressionElement>().Init();
            QueryOrAssert(out m_enableAutoBlink, root, "AutoBlink");
            m_blink = root.Q("Blink").Q<ExpressionElement>().Init();
            QueryOrAssert(out m_enableLookatTarget, root, "LookatTarget");
            QueryOrAssert(out m_yaw, root, "Yaw");
            QueryOrAssert(out m_pitch, root, "Pitch");

            // meta
            QueryOrAssert(out m_metaName, root, "MetaName");
            QueryOrAssert(out m_metaVersion, root, "MetaVersion");
            QueryOrAssert(out m_metaAuthor, root, "MetaAuthor");
            QueryOrAssert(out m_metaCopyright, root, "MetaCopyright");
            QueryOrAssert(out m_metaContact, root, "MetaContact");
            QueryOrAssert(out m_metaReference, root, "MetaReference");
            QueryOrAssert(out m_metaThumbnail, root, "Thumbnail");
            QueryOrAssert(out m_metaPermissionAllowed, root, "PermissionAllowed");
            QueryOrAssert(out m_metaPermissionViolent, root, "PermissionViolent");
            QueryOrAssert(out m_metaPermissionSexual, root, "PermissionSexual");
            QueryOrAssert(out m_metaPermissionCommercial, root, "PermissionCommercial");
            QueryOrAssert(out m_metaLicense, root, "MetaLicense");
            QueryOrAssert(out m_metaDistribution, root, "MetaDistribution");
            root.Q<TextureElement>("FaceCameraTarget").style.backgroundImage = new StyleBackground(Background.FromRenderTexture(m_controller.m_faceCameraTarget));

            m_autoEmotion = gameObject.AddComponent<VRM10AutoExpression>();
            m_autoBlink = gameObject.AddComponent<VRM10Blinker>();
            m_autoLipsync = gameObject.AddComponent<VRM10AIUEO>();

            root.Q<Label>("Version").text = string.Format("VRM10ViewerUI {0}", PackageVersion.VERSION);

            // load initial bvh
            m_controller.Init();
            m_controller.OnUpdateMeta += UpdateMeta;
            m_controller.OnLoaded += OnLoaded;
            m_controller.Start();
            if (ArgumentChecker.TryGetFirstLoadable(out var cmd))
            {
                var _ = m_controller.LoadModelPath(cmd, MakeLoadOptions());
            }
        }

        void OnDisable()
        {
            m_controller.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_root.style.visibility = m_root.style.visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
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
                var vrm = loaded.Instance;

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

                if (m_enableAutoExpression.value)
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Happy, m_autoEmotion.Happy);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Angry, m_autoEmotion.Angry);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Sad, m_autoEmotion.Sad);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, m_autoEmotion.Relaxed);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Surprised, m_autoEmotion.Surprised);
                    m_happy.SetValueWithoutNotify(m_autoEmotion.Happy);
                    m_angry.SetValueWithoutNotify(m_autoEmotion.Angry);
                    m_sad.SetValueWithoutNotify(m_autoEmotion.Sad);
                    m_relaxed.SetValueWithoutNotify(m_autoEmotion.Relaxed);
                    m_surprised.SetValueWithoutNotify(m_autoEmotion.Surprised);
                }
                else
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Happy, m_happy.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Angry, m_angry.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Sad, m_sad.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, m_relaxed.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Surprised, m_surprised.Value);
                }

                if (m_enableAutoLipsync.value)
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Aa, m_autoLipsync.Aa);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ih, m_autoLipsync.Ih);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ou, m_autoLipsync.Ou);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ee, m_autoLipsync.Ee);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Oh, m_autoLipsync.Oh);
                    m_lipAa.SetValueWithoutNotify(m_autoLipsync.Aa);
                    m_lipIh.SetValueWithoutNotify(m_autoLipsync.Ih);
                    m_lipOu.SetValueWithoutNotify(m_autoLipsync.Ou);
                    m_lipEe.SetValueWithoutNotify(m_autoLipsync.Ee);
                    m_lipOh.SetValueWithoutNotify(m_autoLipsync.Oh);
                }
                else
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Aa, m_lipAa.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ih, m_lipIh.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ou, m_lipOu.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Ee, m_lipEe.Value);
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Oh, m_lipOh.Value);
                }

                if (m_enableAutoBlink.value)
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Blink, m_autoBlink.BlinkValue);
                    m_blink.SetValueWithoutNotify(m_autoBlink.BlinkValue);
                }
                else
                {
                    vrm.Runtime.Expression.SetWeight(ExpressionKey.Blink, m_blink.Value);
                }

                if (m_enableLookatTarget.value)
                {
                    var (yaw, pitch) = vrm.Runtime.LookAt.CalculateYawPitchFromLookAtPosition(m_controller.m_lookAtTarget.transform.position);
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
                    m_controller.m_faceCamera.position = pos;
                    m_controller.m_faceCamera.rotation = r;
                }
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

        public void UpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta, Migration.Vrm0Meta meta0)
        {
            try
            {
                m_metaThumbnail.style.backgroundImage = thumbnail;

                if (meta != null)
                {
                    m_metaName.value = meta.Name;
                    m_metaVersion.value = meta.Version;
                    m_metaAuthor.value = meta.Authors[0];
                    m_metaCopyright.value = meta.CopyrightInformation;
                    m_metaContact.value = meta.ContactInformation;
                    if (meta.References != null && meta.References.Count > 0)
                    {
                        m_metaReference.value = meta.References[0];
                    }
                    else
                    {
                        m_metaReference.value = "";
                    }

                    m_metaPermissionAllowed.value = meta.AvatarPermission.ToString();
                    m_metaPermissionViolent.value = meta.AllowExcessivelyViolentUsage.ToString();
                    m_metaPermissionSexual.value = meta.AllowExcessivelySexualUsage.ToString();
                    m_metaPermissionCommercial.value = meta.CommercialUsage.ToString();
                    m_metaLicense.value = meta.LicenseUrl;
                    m_metaDistribution.value = meta.Modification.ToString();
                }

                if (meta0 != null)
                {
                    m_metaName.value = meta0.title;
                    m_metaVersion.value = meta0.version;
                    m_metaAuthor.value = meta0.author;
                    m_metaContact.value = meta0.contactInformation;
                    m_metaReference.value = meta0.reference;

                    m_metaPermissionAllowed.value = meta0.allowedUser.ToString();
                    m_metaPermissionViolent.value = meta0.violentUsage.ToString();
                    m_metaPermissionSexual.value = meta0.sexualUsage.ToString();
                    m_metaPermissionCommercial.value = meta0.commercialUsage.ToString();
                    m_metaLicense.value = meta0.licenseType.ToString();
                    m_metaDistribution.value = "";
                }

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
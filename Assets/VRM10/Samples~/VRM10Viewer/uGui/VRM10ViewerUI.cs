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
        VRM10ViewerController m_controller = new();

        [SerializeField]
        GameObject Root = default;

        [SerializeField]
        Text m_version = default;

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
        Toggle m_useLookAtTarget = default;
        [SerializeField]
        Slider m_yaw = default;
        [SerializeField]
        Slider m_pitch = default;

        [SerializeField]
        TextFields m_texts = default;

        [SerializeField]
        UIFields m_ui = default;

        VRM10AutoExpression m_autoEmotion;
        VRM10Blinker m_autoBlink;
        VRM10AIUEO m_autoLipsync;

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
        }

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
            // URP かつ WebGL で有効にする
            m_useCustomMToonMaterial.isOn = Application.platform == RuntimePlatform.WebGLPlayer && GraphicsSettings.renderPipelineAsset != null;

            m_autoEmotion = gameObject.AddComponent<VRM10AutoExpression>();
            m_autoBlink = gameObject.AddComponent<VRM10Blinker>();
            m_autoLipsync = gameObject.AddComponent<VRM10AIUEO>();

            m_version.text = string.Format("VRM10ViewerUI {0}", PackageVersion.VERSION);

            m_openModel.onClick.AddListener(() => m_controller.OnOpenModelClicked(MakeLoadOptions(), name, nameof(FileSelected)));
            m_openMotion.onClick.AddListener(m_controller.OnOpenMotionClicked);
            m_pastePose.onClick.AddListener(m_controller.OnPastePoseClicked);
            m_resetSpringBone.onClick.AddListener(m_controller.OnResetSpringBoneClicked);
            m_reconstructSpringBone.onClick.AddListener(m_controller.OnReconstructSpringBoneClicked);

            m_texts.Start();

            m_controller.OnUpdateMeta += m_texts.UpdateMeta;
            m_controller.OnLoaded += OnLoaded;
            m_controller.Init();
            m_controller.Start();
            if (ArgumentChecker.TryGetFirstLoadable(out var cmd))
            {
                var _ = m_controller.LoadModelPath(cmd, MakeLoadOptions());
            }
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
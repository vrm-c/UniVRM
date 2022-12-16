using System;
using UniJSON;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;
using VRMShaders;
using static UniVRM10.Vrm10;
using System.Collections.Generic;

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
        Button m_pose = default;

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
        IControlRigInput m_src = default;

        IControlRigInput m_poseSrc = default;
        Vrm10ControlBone m_controlRig = default;

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

            m_src = new AnimatorControlRigInput(GameObject.FindObjectOfType<Animator>());

            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
        }

        Loaded m_loaded;

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_open.onClick.AddListener(OnOpenClicked);

            m_pose.onClick.AddListener(OnPoseClicked);

            // load initial bvh
            if (m_motion != null)
            {
                LoadMotion(m_motion.text);
            }

            string[] cmds = System.Environment.GetCommandLineArgs();
            for (int i = 1; i < cmds.Length; ++i)
            {
                if (File.Exists(cmds[i]))
                {
                    LoadModel(cmds[i]);
                }
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
            m_src = new AnimatorControlRigInput(context.Root.GetComponent<Animator>());
            m_ui.IsBvhEnabled = true;
            // hide box man
            context.Root.GetComponent<SkinnedMeshRenderer>().enabled = false;
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
                if (m_ui.IsBvhEnabled && m_src != null)
                {
                    m_loaded.UpdateControlRig(m_src);
                }
                else
                {
                    if (m_poseSrc != null)
                    {
                        m_loaded.UpdateControlRig(m_poseSrc);
                    }
                    else
                    {
                        m_loaded.TPoseControlRig();
                    }
                }
            }
        }

        void OnOpenClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", "vrm", "bvh");
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
            if (ext == ".bvh")
            {
                LoadMotion(path);
                return;
            }

            if (ext != ".vrm")
            {
                Debug.LogWarning($"{path} is not vrm");
                return;
            }

            LoadModel(path);
        }

        static bool TryGet(glTFExtensionImport extensions, string key, out UniJSON.JsonNode value)
        {
            foreach (var kv in extensions.ObjectItems())
            {
                var currentKey = kv.Key.Value.GetString();
                if (currentKey == key)
                {
                    value = kv.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        void OnPoseClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open Pose", "vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open Pose", "", "vrm");
#else
            var path = "";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var task = LoadPose(path);
        }

        static Dictionary<string, HumanBodyBones> Vrm1ToHumanoidMap = new Dictionary<string, HumanBodyBones>{
                {"hips", HumanBodyBones.Hips},
                {"spine", HumanBodyBones.Spine},
                {"chest", HumanBodyBones.Chest},
                {"neck", HumanBodyBones.Neck},
                {"head", HumanBodyBones.Head},
                {"leftShoulder", HumanBodyBones.LeftShoulder},
                {"leftUpperArm", HumanBodyBones.LeftUpperArm},
                {"leftLowerArm", HumanBodyBones.LeftLowerArm},
                {"leftHand", HumanBodyBones.LeftHand},
                {"rightShoulder", HumanBodyBones.RightShoulder},
                {"rightUpperArm", HumanBodyBones.RightUpperArm},
                {"rightLowerArm", HumanBodyBones.RightLowerArm},
                {"rightHand", HumanBodyBones.RightHand},
                {"leftUpperLeg", HumanBodyBones.LeftUpperLeg},
                {"leftLowerLeg", HumanBodyBones.LeftLowerLeg},
                {"leftFoot", HumanBodyBones.LeftFoot},
                {"leftToes", HumanBodyBones.LeftToes},
                {"rightUpperLeg", HumanBodyBones.RightUpperLeg},
                {"rightLowerLeg", HumanBodyBones.RightLowerLeg},
                {"rightFoot", HumanBodyBones.RightFoot},
                {"rightToes", HumanBodyBones.RightToes},
        };

        static Quaternion ToQuaternion(UniJSON.JsonNode value)
        {
            foreach (var kv in value.ObjectItems())
            {
                if (kv.Key.Value.GetString() == "rotation")
                {
                    Quaternion q = default;
                    var i = 0;
                    foreach (var f in kv.Value.ArrayItems())
                    {
                        switch (i++)
                        {
                            case 0:
                                q.x = f.Value.GetSingle();
                                break;
                            case 1:
                                q.y = f.Value.GetSingle();
                                break;
                            case 2:
                                q.z = f.Value.GetSingle();
                                break;
                            case 3:
                                q.w = f.Value.GetSingle();
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    return q;
                }
            }
            throw new Exception("not reach here");
        }

        UniHumanoid.Humanoid CreateHierarchy(UniGLTF.glTF gltf, UniJSON.JsonNode vrm)
        {
            var root = new GameObject("__root__");
            var nodes = new List<Transform>();
            foreach (var node in gltf.nodes)
            {
                var go = new GameObject(node.name);
                go.transform.SetParent(root.transform, false);
                if (node.matrix != null)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    // TRS
                    if (node.translation != null && node.translation.Length == 3)
                    {
                        go.transform.localPosition = node.translation.ToVector3();
                    }
                    if (node.rotation != null && node.rotation.Length == 4)
                    {
                        go.transform.localRotation = node.rotation.ToQuaternion();
                    }
                }
                nodes.Add(go.transform);
            }

            for (int i = 0; i < gltf.nodes.Count; ++i)
            {
                var gltfNode = gltf.nodes[i];
                if (gltfNode.children != null)
                {
                    var node = nodes[i];
                    foreach (var childIndex in gltfNode.children)
                    {
                        var child = nodes[childIndex];
                        child.SetParent(node, false);
                    }
                }
            }

            var humanoid = root.AddComponent<UniHumanoid.Humanoid>();
            // extensions/VRMC_vrm/humanoid/humanBones/${BONE_NAME}/node = index
            var humanBones = vrm["humanoid"]["humanBones"];
            humanoid.AssignBones(humanBones.ObjectItems().Select(kv =>
            {
                var key = kv.Key.Value.GetString();
                var humanBone = Vrm1ToHumanoidMap[key];
                var index = kv.Value["node"].Value.GetInt32();
                var node = nodes[index];
                return (humanBone, node);
            }));

            return humanoid;
        }

        static Dictionary<HumanBodyBones, Quaternion> PoseDict(UniJSON.JsonNode pose)
        {
            var dict = new Dictionary<HumanBodyBones, Quaternion>();
            foreach (var kv in pose.ObjectItems())
            {
                var key = kv.Key.Value.GetString();
                var humanBone = Vrm1ToHumanoidMap[key];
                var q = ToQuaternion(kv.Value);
                dict.Add(humanBone, q);
            }
            // extensions/VRMC_vrm_pose/${BONE_NAME}/rotation = [x, y, z, w]
            return dict;
        }

        public class ControlBoneControlRigInput : IControlRigInput
        {
            Dictionary<HumanBodyBones, Vrm10ControlBone> rigMap_;
            Transform hips_;

            public ControlBoneControlRigInput(Dictionary<HumanBodyBones, Vrm10ControlBone> bones)
            {
                rigMap_ = bones;
                hips_ = bones[HumanBodyBones.Hips].ControlBone;
            }

            public Vector3 RootPosition => hips_.localPosition;

            public bool TryGetBoneLocalRotation(HumanBodyBones bone, Quaternion parent, out Quaternion rotation)
            {
                if (!rigMap_.TryGetValue(bone, out var rig))
                {
                    rotation = default;
                    return false;
                }

                rotation = rig.NormalizedLocalRotation(parent);
                return true;
            }
        }
        async Task LoadPose(string path)
        {
            using (var gltfData = new GlbLowLevelParser(name, File.ReadAllBytes(path)).Parse())
            {
                if (gltfData.GLTF.extensions is glTFExtensionImport extensions)
                {
                    if (TryGet(extensions, "VRMC_vrm", out var vrm))
                    {
                        if (TryGet(extensions, "VRMC_vrm_pose", out var pose))
                        {
                            // 右手系のヒエラルキー
                            var humanoid = CreateHierarchy(gltfData.GLTF, vrm);
                            var rotations = humanoid.BoneMap.ToDictionary(kv => kv.Item2, kv => kv.Item1.transform.rotation);
                            // ControlRig
                            m_controlRig = Vrm10ControlBone.Build(humanoid, rotations, Handness.Right, out var rigMap);

                            // 右手系のポーズ適用
                            var poseDict = PoseDict(pose);
                            foreach (var kv in poseDict)
                            {
                                if (rigMap.TryGetValue(kv.Key, out var bone))
                                {
                                    bone.ControlBone.localRotation = kv.Value;
                                }
                            }

                            // // 右手系の VRM0 ポーズを得てポーズを作る
                            m_poseSrc = new ControlBoneControlRigInput(rigMap);
                        }
                    }
                }
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpVrm10MaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInVrm10MaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpGltfMaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInGltfMaterialDescriptorGenerator();
            }
        }

        async void LoadModel(string path)
        {
            // cleanup
            m_loaded?.Dispose();
            m_loaded = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                Debug.LogFormat("{0}", path);
                var vrm10Instance = await LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: m_useAsync.enabled ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller(),
                    materialGenerator: GetVrmMaterialDescriptorGenerator(m_useUrpMaterial.isOn),
                    vrmMetaInformationCallback: m_texts.UpdateMeta,
                    ct: cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    UnityObjectDestroyer.DestroyRuntimeOrEditor(vrm10Instance.gameObject);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (vrm10Instance == null)
                {
                    Debug.LogWarning("LoadPathAsync is null");
                    return;
                }

                var instance = vrm10Instance.GetComponent<RuntimeGltfInstance>();
                instance.ShowMeshes();
                instance.EnableUpdateWhenOffscreen();
                m_loaded = new Loaded(instance, m_target.transform);
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


        void OnDrawGizmosSelected()
        {
            // control rig
            if (Application.isPlaying)
            {
                if (m_controlRig != null)
                {
                    m_controlRig.DrawGizmo();
                }
            }
        }
    }
}

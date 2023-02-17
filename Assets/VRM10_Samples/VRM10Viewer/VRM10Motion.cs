using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Utils;
using UniHumanoid;
using UniJSON;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10Motion
    {
        public IControlRigGetter ControlRig;

        UniHumanoid.BvhImporterContext m_context;
        UniGLTF.RuntimeGltfInstance m_instance;

        public Transform Root => m_context?.Root.transform;

        public VRM10Motion(UniHumanoid.BvhImporterContext context)
        {
            m_context = context;
            ControlRig = new NormalizedRigGetter(m_context.Root.transform, m_context.Root.GetComponent<Animator>());
        }

        public VRM10Motion(UniGLTF.RuntimeGltfInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }
        }

        public void ShowBoxMan(bool showBoxMan)
        {
            if (m_context != null)
            {
                m_context.Root.GetComponent<SkinnedMeshRenderer>().enabled = showBoxMan;
            }
        }

        public static VRM10Motion LoadBvhFromText(string source, string path = "tmp.bvh")
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            return new VRM10Motion(context);
        }

        public static VRM10Motion LoadBvhFromPath(string path)
        {
            return LoadBvhFromText(File.ReadAllText(path), path);
        }

        static IEnumerable<Transform> Traverse(Transform t)
        {
            yield return t;
            foreach (Transform child in t)
            {
                foreach (var x in Traverse(child))
                {
                    yield return x;
                }
            }
        }

        #region experimental vrm-animation deserializer
        //
        // vrm-animation の簡易実装
        //
        // https://github.com/vrm-c/vrm-animation
        //
        static float ForceMeterScale(Dictionary<HumanBodyBones, Transform> map)
        {
            var positionMap = map.ToDictionary(kv => kv.Key, kv => kv.Value.position);
            var hipsHeight = positionMap[HumanBodyBones.Hips].y;

            float scaling = 1.0f;
            if (hipsHeight > 80)
            {
                // cm スケールであると見做す
                scaling = 0.01f;
            }
            return scaling;
        }

        static bool TryGet(JsonNode obj, string key, out JsonNode found)
        {
            foreach (var kv in obj.ObjectItems())
            {
                if (kv.Key.GetString() == key)
                {
                    found = kv.Value;
                    return true;
                }
            }
            found = default;
            return false;
        }

        static (HumanBodyBones, int) ToTuple(KeyValuePair<JsonNode, JsonNode> kv)
        {
            if (TryGet(kv.Value, "node", out var value))
            {
                var name = kv.Key.GetString();
                switch (name)
                {
                    case "rightThumbMetacarpal":
                        return (HumanBodyBones.RightThumbProximal, value.GetInt32());
                    case "leftThumbMetacarpal":
                        return (HumanBodyBones.LeftThumbProximal, value.GetInt32());
                    case "rightThumbProximal":
                        return (HumanBodyBones.RightThumbIntermediate, value.GetInt32());
                    case "leftThumbProximal":
                        return (HumanBodyBones.LeftThumbIntermediate, value.GetInt32());
                    default:
                        return ((HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), name, true), value.GetInt32());
                }
            }
            throw new Exception();
        }

        static Dictionary<HumanBodyBones, Transform> GetHumanMap(GltfData data, IReadOnlyList<Transform> nodes)
        {
            var humanMap = new Dictionary<HumanBodyBones, Transform>();
            if (data.GLTF.extensions is UniGLTF.glTFExtensionImport extensions)
            {
                foreach (var kv in extensions.ObjectItems())
                {
                    if (kv.Key.GetString() == "VRMC_vrm_animation")
                    {
                        var animation = kv.Value;
                        if (TryGet(animation, "humanoid", out var animation_humanoid))
                        {
                            if (TryGet(animation_humanoid, "humanBones", out var bones))
                            {
                                foreach (var kkvv in bones.ObjectItems())
                                {
                                    var (bone, index) = ToTuple(kkvv);
                                    // Debug.Log($"{bone} => {index}");
                                    humanMap.Add(bone, nodes[index]);
                                }
                            }
                        }
                    }
                }
            }
            return humanMap;
        }
        #endregion

        public static async Task<VRM10Motion> LoadVrmAnimationFromPathAsync(string path)
        {
            //
            // GetHumanoid Mapping
            //
            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new UniGLTF.ImporterContext(data))
            {
                loader.InvertAxis = Axes.X;
                loader.PositionScaling = 0.01f;
                var instance = await loader.LoadAsync(new ImmediateCaller());
                var humanMap = GetHumanMap(data, loader.Nodes);
                if (humanMap.Count == 0)
                {
                    throw new ArgumentException("fail to load VRMC_vrm_animation");
                }

                var scaling = ForceMeterScale(humanMap);
                // instance.transform.localScale = new Vector3(scaling, scaling, scaling);
                var description = AvatarDescription.Create(humanMap);

                //
                // avatar
                //
                var avatar = description.CreateAvatar(instance.Root.transform);
                avatar.name = "Avatar";
                // AvatarDescription = description;
                var animator = instance.gameObject.AddComponent<Animator>();
                animator.avatar = avatar;

                // create SkinnedMesh for bone visualize
                var renderer = SkeletonMeshUtility.CreateRenderer(animator);
                var material = new Material(Shader.Find("Standard"));
                renderer.sharedMaterial = material;
                var mesh = renderer.sharedMesh;
                mesh.name = "box-man";

                var humanoid = instance.gameObject.AddComponent<Humanoid>();
                humanoid.AssignBonesFromAnimator();
                var motion = new VRM10Motion(instance);
                motion.ControlRig = new InitRotationGetter(instance.transform, humanoid);
                return motion;
            }
        }

        public void Retarget(IControlRigSetter dst)
        {
            ControlRigUtil.Retarget(ControlRig, dst);
        }
    }
}

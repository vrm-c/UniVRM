using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniHumanoid
{
    [Obsolete("use BvhImporterContext")]
    public class ImporterContext : BvhImporterContext
    {
    }

    public class BvhImporterContext
    {
        #region Source
        String m_path;
        public String Path
        {
            get { return m_path; }
            set
            {
                if (m_path == value) return;
                m_path = value;
            }
        }
        public String Source; // source
        public Bvh Bvh;
        #endregion

        #region Imported
        public GameObject Root;
        public List<Transform> Nodes = new List<Transform>();
        public AnimationClip Animation;
        public AvatarDescription AvatarDescription;
        public Avatar Avatar;
        public Mesh Mesh;
        public Material Material;
        #endregion

        #region Load
        [Obsolete("use Load(path)")]
        public void Parse()
        {
            Parse(Path);
        }

        public void Parse(string path)
        {
            Parse(path, File.ReadAllText(path, Encoding.UTF8));
        }

        public void Parse(string path, string source)
        {
            Path = path;
            Source = source;
            Bvh = Bvh.Parse(Source);
        }

        public void Load()
        {
            //
            // build hierarchy
            //
            Root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(Path));
            var hips = BuildHierarchy(Root.transform, Bvh.Root, 1.0f);
            var skeleton = Skeleton.Estimate(hips);
            var description = AvatarDescription.Create(hips.Traverse().ToArray(), skeleton);

            //
            // scaling. reposition
            //
            var yCh = Bvh.Root.GetChannelIndex(BvhChannel.Yposition);
            var curve = Bvh.Channels[yCh];
            var hipHeight = curve.Keys[0];

            float scaling = 1.0f;
            {
                //var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                // var foot = hips.Traverse().Skip(skeleton.GetBoneIndex(HumanBodyBones.LeftFoot)).First();
                // var hipHeight = hips.position.y - foot.position.y;
                // hips height to a meter
                scaling = 1.0f / hipHeight;
                foreach (var x in Root.transform.Traverse())
                {
                    x.localPosition *= scaling;
                }

                var scaledHeight = hipHeight * scaling;
                hips.position = new Vector3(0, scaledHeight, 0); // foot to ground
            }

            //
            // create AnimationClip
            //
            Animation = BvhAnimation.CreateAnimationClip(Bvh, scaling);
            Animation.name = Root.name;
            Animation.legacy = true;
            Animation.wrapMode = WrapMode.Loop;

            var animation = Root.AddComponent<Animation>();
            animation.AddClip(Animation, Animation.name);
            animation.clip = Animation;
            animation.Play();

            //
            // avatar
            //
            Avatar = description.CreateAvatar(Root.transform);
            Avatar.name = "Avatar";
            AvatarDescription = description;
            var animator = Root.AddComponent<Animator>();
            animator.avatar = Avatar;

            Root.AddComponent<HumanPoseTransfer>();
        }

        static Transform BuildHierarchy(Transform parent, BvhNode node, float toMeter)
        {
            var go = new GameObject(node.Name);
            go.transform.localPosition = node.Offset.ToXReversedVector3() * toMeter;
            go.transform.SetParent(parent, false);

            //var gizmo = go.AddComponent<BoneGizmoDrawer>();
            //gizmo.Draw = true;

            foreach (var child in node.Children)
            {
                BuildHierarchy(go.transform, child, toMeter);
            }

            return go.transform;
        }
        #endregion

#if UNITY_EDITOR
        protected virtual string GetPrefabPath()
        {
            var dir = System.IO.Path.GetDirectoryName(Path);
            var name = System.IO.Path.GetFileNameWithoutExtension(Path);
            var prefabPath = string.Format("{0}/{1}.prefab", dir, name);
            if (!Application.isPlaying && File.Exists(prefabPath))
            {
                // already exists
                if (IsOwn(prefabPath))
                {
                    //UniGLTFLogger.LogFormat("already exist. own: {0}", prefabPath);
                }
                else
                {
                    // but unknown prefab
                    var unique = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
                    //UniGLTFLogger.LogFormat("already exist: {0} => {1}", prefabPath, unique);
                    prefabPath = unique;
                }
            }
            return prefabPath;
        }

        #region Assets
        IEnumerable<UnityEngine.Object> GetSubAssets(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path);
        }

        protected virtual bool IsOwn(string path)
        {
            foreach (var x in GetSubAssets(path))
            {
                //if (x is Transform) continue;
                if (x is GameObject) continue;
                if (x is Component) continue;
                if (AssetDatabase.IsSubAsset(x))
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerable<UnityEngine.Object> ObjectsForSubAsset()
        {
            if (Animation != null) yield return Animation;
            if (AvatarDescription != null) yield return AvatarDescription;
            if (Avatar != null) yield return Avatar;
            if (Mesh != null) yield return Mesh;
            if (Material != null) yield return Material;
        }

        public void SaveAsAsset()
        {
            var path = GetPrefabPath();
            if (File.Exists(path))
            {
                // clear SubAssets
                foreach (var x in GetSubAssets(path).Where(x => !(x is GameObject) && !(x is Component)))
                {
                    GameObject.DestroyImmediate(x, true);
                }
            }

            // Add SubAsset
            foreach (var o in ObjectsForSubAsset())
            {
                AssetDatabase.AddObjectToAsset(o, path);
            }

            // Create or update Main Asset
            UniGLTF.UniGLTFLogger.Log($"create prefab: {path}");
            PrefabUtility.SaveAsPrefabAssetAndConnect(Root, path, InteractionMode.AutomatedAction);

            AssetDatabase.ImportAsset(path);
        }
        #endregion
#endif

        public void Destroy(bool destroySubAssets)
        {
            if (Root != null) GameObject.DestroyImmediate(Root);
            if (destroySubAssets)
            {
#if UNITY_EDITOR
                foreach (var o in ObjectsForSubAsset())
                {
                    UnityEngine.Object.DestroyImmediate(o, true);
                }
#endif
            }
        }
    }
}

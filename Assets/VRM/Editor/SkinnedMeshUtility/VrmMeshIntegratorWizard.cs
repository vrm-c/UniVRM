#pragma warning disable 0414, 0649
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UniGLTF.MeshUtility;
using System.IO;
using UniGLTF.M17N;

namespace VRM
{
    public class VrmMeshIntegratorWizard : ScriptableWizard
    {
        const string ASSET_SUFFIX = ".mesh.asset";

        enum HelpMessage
        {
            Ready,
            SetTarget,
            InvalidTarget,
        }

        enum ValidationError
        {
            None,
            NoTarget,
            HasParent,
        }

        [SerializeField]
        GameObject m_root;

        [SerializeField]
        bool m_separateByBlendShape = true;

        [Header("Validation")]
        [SerializeField]
        Material[] m_uniqueMaterials;

        [Serializable]
        struct MaterialKey
        {
            public string Shader;
            public KeyValuePair<string, object>[] Properties;

            public override bool Equals(object obj)
            {
                if (!(obj is MaterialKey))
                {
                    return base.Equals(obj);
                }

                var key = (MaterialKey)obj;

                return Shader == key.Shader
                    && Properties.SequenceEqual(key.Properties)
                    ;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Serializable]
        struct MaterialList
        {
            public Material[] Materials;

            public MaterialList(Material[] list)
            {
                Materials = list;
            }
        }
        [SerializeField]
        MaterialList[] m_duplicateMaterials;

        [Serializable]
        public class ExcludeItem
        {
            public Mesh Mesh;
            public bool Exclude;
        }

        [Header("Options")]
        [SerializeField]
        List<ExcludeItem> m_excludes = new List<ExcludeItem>();

        [Header("Result")]
        [SerializeField]
        MeshMap[] integrationResults;

        public static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<VrmMeshIntegratorWizard>("MeshIntegratorWizard", "Integrate and close window", "Integrate");
        }

        private void OnEnable()
        {
            Clear(HelpMessage.Ready, ValidationError.None);
            m_root = Selection.activeGameObject;
            OnValidate();
        }

        static object GetPropertyValue(Shader shader, int i, Material m)
        {
            var propType = ShaderUtil.GetPropertyType(shader, i);
            switch (propType)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    return m.GetColor(ShaderUtil.GetPropertyName(shader, i));

                case ShaderUtil.ShaderPropertyType.Range:
                case ShaderUtil.ShaderPropertyType.Float:
                    return m.GetFloat(ShaderUtil.GetPropertyName(shader, i));

                case ShaderUtil.ShaderPropertyType.Vector:
                    return m.GetVector(ShaderUtil.GetPropertyName(shader, i));

                case ShaderUtil.ShaderPropertyType.TexEnv:
                    return m.GetTexture(ShaderUtil.GetPropertyName(shader, i));

                default:
                    throw new NotImplementedException(propType.ToString());
            }
        }

        static MaterialKey GetMaterialKey(Material m)
        {
            var key = new MaterialKey
            {
                Shader = m.shader.name,
            };

            key.Properties = Enumerable.Range(0, ShaderUtil.GetPropertyCount(m.shader))
                .Select(x => new KeyValuePair<string, object>(
                    ShaderUtil.GetPropertyName(m.shader, x),
                    GetPropertyValue(m.shader, x, m))
                    )
                .OrderBy(x => x.Key)
                .ToArray()
                    ;

            return key;
        }

        void Clear(HelpMessage help, ValidationError error)
        {
            helpString = help.Msg();
            errorString = error != ValidationError.None ? error.Msg() : null;
            m_uniqueMaterials = new Material[] { };
            m_duplicateMaterials = new MaterialList[] { };
            m_excludes.Clear();
            isValid = false;
        }

        void OnValidate()
        {
            if (m_root == null)
            {
                Clear(HelpMessage.SetTarget, ValidationError.NoTarget);
                return;
            }

            if (m_root.transform.parent != null)
            {
                Clear(HelpMessage.InvalidTarget, ValidationError.HasParent);
                return;
            }

            Clear(HelpMessage.Ready, ValidationError.None);
            isValid = true;
            m_uniqueMaterials = MeshIntegratorUtility.EnumerateSkinnedMeshRenderer(m_root.transform, MeshEnumerateOption.OnlyWithoutBlendShape)
                .SelectMany(x => x.sharedMaterials)
                .Distinct()
                .ToArray();

            m_duplicateMaterials = m_uniqueMaterials
                .GroupBy(x => GetMaterialKey(x), x => x)
                .Select(x => new MaterialList(x.ToArray()))
                .Where(x => x.Materials.Length > 1)
                .ToArray()
                ;

            var exclude_map = new Dictionary<Mesh, ExcludeItem>();
            var excludes = new List<ExcludeItem>();
            foreach (var x in m_root.GetComponentsInChildren<Renderer>())
            {
                var mesh = x.GetMesh();
                if (mesh == null)
                {
                    continue;
                }
                var item = new ExcludeItem { Mesh = mesh };
                excludes.Add(item);
                exclude_map[mesh] = item;
            }
            foreach (var x in m_excludes)
            {
                if (exclude_map.TryGetValue(x.Mesh, out ExcludeItem item))
                {
                    // update
                    item.Exclude = x.Exclude;
                }
            }
            m_excludes.Clear();
            foreach (var kv in exclude_map)
            {
                m_excludes.Add(kv.Value);
            }
        }

        void OnWizardUpdate()
        {
            // helpString = "Set target gameobject `in scene`. Prefab not supported.";
        }

        void Integrate()
        {
            var prefabPath = AssetDatabase.GetAssetPath(m_root);
            Debug.Log(prefabPath);
            var path = EditorUtility.SaveFilePanel("save prefab", Path.GetDirectoryName(prefabPath), m_root.name, "prefab");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var assetPath = UniGLTF.UnityPath.FromFullpath(path);
            if (!assetPath.IsUnderAssetsFolder)
            {
                Debug.LogWarning($"{path} is not asset path");
                return;
            }

            var excludes = m_excludes.Where(x => x.Exclude).Select(x => x.Mesh);

            // Backup Exists
            VrmPrefabUtility.BackupVrmPrefab(m_root);

            Undo.RecordObject(m_root, "Mesh Integration");
            var instance = VrmPrefabUtility.InstantiatePrefab(m_root);

            var clips = new List<BlendShapeClip>();
            var proxy = instance.GetComponent<VRMBlendShapeProxy>();
            if (proxy != null && proxy.BlendShapeAvatar != null)
            {
                clips = proxy.BlendShapeAvatar.Clips;
            }
            foreach (var clip in clips)
            {
                Undo.RecordObject(clip, "Mesh Integration");
            }

            // Execute
            var results = VRMMeshIntegratorUtility.Integrate(instance, clips, excludes, m_separateByBlendShape);
            // integrationResults = MeshIntegratorEditor.Integrate(m_root, assetPath, excludes, m_separateByBlendShape).Select(x => x.MeshMap).ToArray();
            // public static List<MeshIntegrationResult> Integrate(GameObject prefab, UniGLTF.UnityPath writeAssetPath, IEnumerable<Mesh> excludes, bool separateByBlendShape)

            // disable source renderer
            foreach (var res in results)
            {
                foreach (var renderer in res.SourceSkinnedMeshRenderers)
                {
                    Undo.RecordObject(renderer.gameObject, "Deactivate old renderer");
                    renderer.gameObject.SetActive(false);
                }

                foreach (var renderer in res.SourceMeshRenderers)
                {
                    Undo.RecordObject(renderer.gameObject, "Deactivate old renderer");
                    renderer.gameObject.SetActive(false);
                }
            }

            foreach (var result in results)
            {
                if (result.IntegratedRenderer == null) continue;

                var childAssetPath = assetPath.Parent.Child($"{result.IntegratedRenderer.gameObject.name}{ASSET_SUFFIX}");
                Debug.LogFormat("CreateAsset: {0}", childAssetPath);
                childAssetPath.CreateAsset(result.IntegratedRenderer.sharedMesh);
                Undo.RegisterCreatedObjectUndo(result.IntegratedRenderer.gameObject, "Integrate Renderers");
            }

            // Apply to Prefab
            if (UniGLTF.UnityPath.FromUnityPath(AssetDatabase.GetAssetPath(m_root)).Equals(assetPath))
            {
                VrmPrefabUtility.ApplyChangesToPrefab(instance);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(instance, assetPath.Value, out bool success);
                if (!success)
                {
                    throw new System.Exception($"PrefabUtility.SaveAsPrefabAsset: {assetPath}");
                }
            }

            // destroy source renderers
            UnityEngine.Object.DestroyImmediate(instance);
        }

        void OnWizardCreate()
        {
            Integrate();
            // close
        }

        void OnWizardOtherButton()
        {
            Integrate();
        }
    }
}

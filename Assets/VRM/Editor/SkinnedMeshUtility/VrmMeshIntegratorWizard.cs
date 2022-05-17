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

        protected override bool DrawWizardGUI()
        {
            var t = m_root.GetGameObjectType();
            EditorGUILayout.HelpBox($"{t}", MessageType.Info);
            return base.DrawWizardGUI();
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
        }

        /// 2022.05 仕様変更
        /// 
        /// * scene or prefab どっちでも動作する
        /// * backup するのではなく 変更した copy を作成する。元は変えない
        /// * 実行すると mesh, blendshape, blendShape を新規に作成する
        /// * scene のときは新しいヒエラルキーが出現する
        /// * prefab のときは新しいヒエラルキーを prefab 保存して、scene の方を削除して終了する
        /// 
        void Integrate()
        {
            String folder = "Assets";
            var prefab = m_root.GetPrefab();
            if (prefab != null)
            {
                folder = AssetDatabase.GetAssetPath(prefab);
                Debug.Log(folder);
            }

            // 新規で作成されるアセットはすべてこのフォルダの中に作る。上書きチェックはしない
            var assetFolder = EditorUtility.SaveFolderPanel("select asset save folder", Path.GetDirectoryName(folder), "VrmIntegrated");
            var unityPath = UniGLTF.UnityPath.FromFullpath(assetFolder);
            if (!unityPath.IsUnderAssetsFolder)
            {
                EditorUtility.DisplayDialog("asset folder", "Target folder must be in the `Assets` folder", "cancel");
                return;
            }
            assetFolder = unityPath.Value;

            var copy = GameObject.Instantiate(m_root);

            // 統合
            var excludes = m_excludes.Where(x => x.Exclude).Select(x => x.Mesh);
            var results = Integrate(copy, excludes, m_separateByBlendShape);

            // 統合前のMeshを非表示にして、統合した結果をヒエラルキーに追加する
            DeactivateOldRendererAndAddIntegrated(copy, results);

            // write mesh asset
            foreach (var result in results)
            {
                var childAssetPath = $"{assetFolder}/{result.IntegratedRenderer.gameObject.name}{ASSET_SUFFIX}";
                Debug.LogFormat("CreateAsset: {0}", childAssetPath);
                AssetDatabase.CreateAsset(result.IntegratedRenderer.sharedMesh, childAssetPath);
            }

            // 統合した結果を反映した BlendShapeClip を作成して置き換える
            VRMMeshIntegratorUtility.FollowBlendshapeRendererChange(results, copy, assetFolder);

            // reset firstperson
            var firstperson = copy.GetComponent<VRMFirstPerson>();
            if (firstperson != null)
            {
                firstperson.Reset();
            }

            if (prefab != null)
            {
                // prefab
                var prefabPath = $"{assetFolder}/VrmIntegrated.prefab";
                Debug.Log(prefabPath);
                PrefabUtility.SaveAsPrefabAsset(copy, prefabPath, out bool success);
                if (!success)
                {
                    throw new System.Exception($"PrefabUtility.SaveAsPrefabAsset: {prefabPath}");
                }

                // destroy scene
                UnityEngine.Object.DestroyImmediate(copy);
            }
            else
            {
                // do nothing. keep scene
            }
        }

        static List<UniGLTF.MeshUtility.MeshIntegrationResult> Integrate(GameObject root, IEnumerable<Mesh> excludes, bool separateByBlendShape)
        {
            var results = new List<UniGLTF.MeshUtility.MeshIntegrationResult>();
            if (separateByBlendShape)
            {
                results.Add(MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: MeshEnumerateOption.OnlyWithoutBlendShape, excludes: excludes));
                results.Add(MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: MeshEnumerateOption.OnlyWithBlendShape, excludes: excludes));
            }
            else
            {
                results.Add(MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: MeshEnumerateOption.All, excludes: excludes));
            }
            return results;
        }

        /// <summary>
        /// 古いMeshを disable にし、新しい統合済み mesh を追加する
        /// </summary>
        static void DeactivateOldRendererAndAddIntegrated(GameObject root, List<MeshIntegrationResult> results)
        {
            // disable source renderer
            foreach (var result in results)
            {
                foreach (var renderer in result.SourceSkinnedMeshRenderers)
                {
                    Undo.RecordObject(renderer.gameObject, "Deactivate old renderer");
                    renderer.gameObject.SetActive(false);
                }

                foreach (var renderer in result.SourceMeshRenderers)
                {
                    Undo.RecordObject(renderer.gameObject, "Deactivate old renderer");
                    renderer.gameObject.SetActive(false);
                }
            }

            // Add integrated
            foreach (var result in results)
            {
                if (result.IntegratedRenderer != null)
                {
                    result.IntegratedRenderer.transform.SetParent(root.transform, false);
                }
            }
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

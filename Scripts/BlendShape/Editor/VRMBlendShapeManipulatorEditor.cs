using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.IO;

namespace VRM
{
    [CustomEditor(typeof(VRMBlendShapeManipulator))]
    public class VRMBlendShapeManipulatorEditor : Editor
    {
        VRMBlendShapeManipulator m_target = null;
        SkinnedMeshRenderer[] m_renderers;

        void OnEnable()
        {
            m_target = (VRMBlendShapeManipulator)target;

            m_renderers = m_target.transform
                .Traverse()
                .Select(x => x.GetComponent<SkinnedMeshRenderer>())
                .Where(x => x != null)
                .ToArray()
                ;
        }

        static string EscapeFilePath(string src)
        {
            return src
                .Replace(">", "＞")
                .Replace("<", "＜")
                ;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Clear"))
            {
                ClearBlendShape();
            }

            if (GUILayout.Button("Create BlendShapeClip"))
            {
                CreateBlendShapeClip();
            }

            var clip = (BlendShapeClip)EditorGUILayout.ObjectField("Load clip", null, typeof(BlendShapeClip), false);
            if (clip != null)
            {
                ClearBlendShape();
                clip.Apply(m_target.transform, 1.0f);
            }

            EditorGUILayout.Space();

            // sliders
            foreach (var renderer in m_renderers)
            {
                var mesh = renderer.sharedMesh;
                if (mesh != null)
                {
                    var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, m_target.transform);
                    EditorGUILayout.LabelField(m_target.name+"/"+relativePath);

                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        var src = renderer.GetBlendShapeWeight(i);
                        var dst = EditorGUILayout.Slider(mesh.GetBlendShapeName(i), src, 0, 100.0f);
                        if (dst != src)
                        {
                            renderer.SetBlendShapeWeight(i, dst);
                        }
                    }
                }
            }
        }

        private void CreateBlendShapeClip()
        {
            var maxWeight = 0.0f;
            var maxWeightName = "";

            // weightのついたblendShapeを集める
            var blendShapes = m_renderers.SelectMany(x =>
            {
                var mesh = x.sharedMesh;

                var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(x.transform, m_target.transform);

                var list = new List<BlendShapeBinding>();
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        var weight = x.GetBlendShapeWeight(i);
                        if (weight == 0)
                        {
                            continue;
                        }
                        var name = mesh.GetBlendShapeName(i);
                        if (weight > maxWeight)
                        {
                            maxWeightName = name;
                            maxWeight = weight;
                        }
                        list.Add(new BlendShapeBinding
                        {
                            Index = i,
                            RelativePath = relativePath,
                            Weight = weight
                        });
                    }
                }
                return list;
            })
            .ToArray()
            ;

            var assetPath = string.Format("Assets/{0}.{1}.asset",
                m_target.name,
                maxWeightName);
#if UNITY_2018_2_OR_NEWER
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(m_target.gameObject);
#else
            var prefab = PrefabUtility.GetPrefabParent(m_target.gameObject);
#endif
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}.{2}.asset",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    maxWeightName);
            }
            assetPath = EscapeFilePath(assetPath);

            var asset = ScriptableObject.CreateInstance<BlendShapeClip>();
            asset.Values = blendShapes.ToArray();
            Debug.LogFormat("create asset: {0}", assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);

            Selection.objects = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath(assetPath, typeof(BlendShapeClip)) };
        }

        private void ClearBlendShape()
        {
            foreach (var renderer in m_renderers)
            {
                var mesh = renderer.sharedMesh;
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        renderer.SetBlendShapeWeight(i, 0);
                    }
                }
            }
        }
    }
}

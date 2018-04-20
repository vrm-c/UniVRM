using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeAvatar))]
    public class BlendShapeAvatarEditor : PreviewEditor
    {
        static String[] Presets = ((BlendShapePreset[])Enum.GetValues(typeof(BlendShapePreset)))
            .Select(x => x.ToString()).ToArray();

        BlendShapeAvatar m_target;
        void AddBlendShapeClip()
        {
            var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(m_target));
            var path = EditorUtility.SaveFilePanel(
                           "Create BlendShapeClip",
                           dir,
                           string.Format("BlendShapeClip#{0}.asset", m_target.Clips.Count),
                           "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            path = path.ToUnityRelativePath();
            Debug.LogFormat("{0}", path);
            var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            m_target.Clips.Add(clip);
            clip.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(m_target));
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.ImportAsset(path);
        }

        BlendShapeClip m_currentClip;
        BlendShapeClip CurrentClip
        {
            get { return m_currentClip; }
            set
            {
                if (m_currentClip == value) return;

                m_currentClip = value;
                //ClearBlendShape();
                Bake(m_currentClip.Values, m_currentClip.MaterialValues, 1.0f);
            }
        }

        void OnPrefabChanged()
        {
            if (m_currentClip != null)
            {
                Bake(m_currentClip.Values, m_currentClip.MaterialValues, 1.0f);
            }
        }

        protected override void OnEnable()
        {
            PrefabChanged += OnPrefabChanged;

            base.OnEnable();
            m_target = (BlendShapeAvatar)target;

            CurrentClip = m_target.Clips[0];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PrefabChanged -= OnPrefabChanged;
        }

        List<bool> m_meshFolds = new List<bool>();
        int m_preset;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // buttons
            if (m_target.Clips != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Select BlendShapeClip", EditorStyles.boldLabel);
                var preset = GUILayout.SelectionGrid(m_preset, m_target.Clips
                    .Where(x => x != null)
                    .Select(x => BlendShapeKey.CreateFrom(x).ToString()).ToArray(), 4);
                if (preset != m_preset)
                {
                    CurrentClip = m_target.Clips[preset];
                    m_preset = preset;
                }
            }

            // Add
            if (GUILayout.Button("Add BlendShapeClip"))
            {
                AddBlendShapeClip();
            }

            if (CurrentClip != null)
            {
                // clip
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("CurrentClip", EditorStyles.boldLabel);

                /*var loadClip = (BlendShapeClip)*/
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Current clip",
                    CurrentClip, typeof(BlendShapeClip), false);
                GUI.enabled = true;

                CurrentClip.Preset = (BlendShapePreset)EditorGUILayout.Popup("Preset", (int)CurrentClip.Preset, Presets);

                CurrentClip.BlendShapeName = EditorGUILayout.TextField("BlendShapeName", CurrentClip.BlendShapeName);

                var key = BlendShapeKey.CreateFrom(CurrentClip);
                if (m_target.Clips.Where(x => key.Match(x)).Count() > 1)
                {
                    EditorGUILayout.HelpBox("duplicate clip", MessageType.Error);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("BlendShapeValues", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear"))
                {
                    ClearBlendShape();
                }

                if (CurrentClip != null && GUILayout.Button("Apply"))
                {
                    string maxWeightString;
                    CurrentClip.Values = GetBindings(out maxWeightString);
                    EditorUtility.SetDirty(CurrentClip);
                }
                EditorGUILayout.EndHorizontal();

                // sliders
                bool changed = false;
                int foldIndex = 0;
                foreach (var item in PreviewSceneManager.EnumRenderItems.Where(x => x.SkinnedMeshRenderer!=null))
                {
                    var mesh = item.SkinnedMeshRenderer.sharedMesh;
                    if (mesh != null && mesh.blendShapeCount>0)
                    {
                        //var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, m_target.transform);
                        //EditorGUILayout.LabelField(m_target.name + "/" + item.Path);

                        if (foldIndex >= m_meshFolds.Count)
                        {
                            m_meshFolds.Add(false);
                        }
                        m_meshFolds[foldIndex] = EditorGUILayout.Foldout(m_meshFolds[foldIndex], item.SkinnedMeshRenderer.name);
                        if (m_meshFolds[foldIndex])
                        {
                            //EditorGUI.indentLevel += 1;
                            for (int i = 0; i < mesh.blendShapeCount; ++i)
                            {
                                var src = item.SkinnedMeshRenderer.GetBlendShapeWeight(i);
                                var dst = EditorGUILayout.Slider(mesh.GetBlendShapeName(i), src, 0, 100.0f);
                                if (dst != src)
                                {
                                    item.SkinnedMeshRenderer.SetBlendShapeWeight(i, dst);
                                    changed = true;
                                }
                            }
                            //EditorGUI.indentLevel -= 1;
                        }
                        ++foldIndex;
                    }
                }

                if (changed)
                {
                    PreviewSceneManager.Bake();
                }
            }
        }

        BlendShapeBinding[] GetBindings(out string _maxWeightName)
        {
            var maxWeight = 0.0f;
            var maxWeightName = "";
            // weightのついたblendShapeを集める
            var values = PreviewSceneManager.EnumRenderItems
                .Where(x => x.SkinnedMeshRenderer!=null)
                .SelectMany(x =>
            {
                var mesh = x.SkinnedMeshRenderer.sharedMesh;

                var relativePath = x.Path;

                var list = new List<BlendShapeBinding>();
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        var weight = x.SkinnedMeshRenderer.GetBlendShapeWeight(i);
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
            }).ToArray()
            ;
            _maxWeightName = maxWeightName;
            return values;
        }

        private void ClearBlendShape()
        {
            foreach (var item in PreviewSceneManager.EnumRenderItems.Where(x => x.SkinnedMeshRenderer!=null))
            {
                var renderer = item.SkinnedMeshRenderer;
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

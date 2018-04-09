using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VRM
{
    [DisallowMultipleComponent]
    public class VRMBlendShapeProxy : MonoBehaviour
    {
        [SerializeField]
        public BlendShapeAvatar BlendShapeAvatar;

#if UNITY_EDITOR
        public class BlendShapeSlider
        {
            VRMBlendShapeProxy m_target;
            BlendShapeKey m_key;

            public BlendShapeSlider(VRMBlendShapeProxy target, BlendShapeKey key)
            {
                m_target = target;
                m_key = key;
            }

            public void Slider()
            {
                if (m_target.BlendShapeAvatar == null)
                {
                    return;
                }

                var oldValue = m_target.GetValue(m_key);
                var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
                if (oldValue != newValue)
                {
                    m_target.SetValue(m_key, newValue);
                }
            }
        }
        List<BlendShapeSlider> m_sliders;
        public List<BlendShapeSlider> Sliders
        {
            get { return m_sliders; }
        }
        private void SetupSliders()
        {
            if (BlendShapeAvatar != null && BlendShapeAvatar.Clips != null)
            {
                m_sliders = BlendShapeAvatar.Clips
                    .Where(x => x != null)
                    .Select(x => new BlendShapeSlider(this, BlendShapeKey.CreateFrom(x)))
                    .ToList()
                    ;
            }
        }
#endif

        struct BlendShapePath
        {
            public String RelativePath;
            public int Index;
        }

        delegate void BlendShapeSetter(float value);

        class BlendShapePathHandler
        {
            public BlendShapeSetter Setter;
            float m_value;

            /*
            public void ReplaceValue(float value)
            {
                m_value = value;
            }
            */

            public void AddValue(float value)
            {
                m_value += value;
            }

            public void Apply(float value)
            {
                Setter(value);
                m_value = 0;
            }

            public void Apply()
            {
                Setter(m_value);
                m_value = 0;
            }

            public void Clear()
            {
                m_value = 0;
                Apply();
            }
        }

        class BlendShapeMerger
        {
            Dictionary<BlendShapeKey, BlendShapeClip> m_clipMap;
            Dictionary<BlendShapeKey, float> m_valueMap;
            Dictionary<BlendShapeBinding, BlendShapePathHandler> m_setterMap = new Dictionary<BlendShapeBinding, BlendShapePathHandler>();

            public BlendShapeMerger(IEnumerable<BlendShapeClip> clips, Transform root)
            {
                m_clipMap = clips.ToDictionary(x => BlendShapeKey.CreateFrom(x), x => x);
                m_valueMap = new Dictionary<BlendShapeKey, float>();
                foreach (var kv in m_clipMap)
                {
                    foreach (var binding in kv.Value.Values)
                    {

                        if (!m_setterMap.ContainsKey(binding))
                        {
                            var _target = root.Find(binding.RelativePath);
                            SkinnedMeshRenderer target = null;
                            if (_target != null)
                            {
                                target = _target.GetComponent<SkinnedMeshRenderer>();
                            }
                            if (target != null)
                            {
                                m_setterMap.Add(binding, new BlendShapePathHandler
                                {
                                    Setter = x =>
                                    {
                                        target.SetBlendShapeWeight(binding.Index, x);
                                    }
                                });
                            }
                            else
                            {
                                Debug.LogWarningFormat("{0} not found", binding.RelativePath);
                            }
                        }
                    }
                }
            }

            public void Clear()
            {
                foreach (var kv in m_setterMap)
                {
                    kv.Value.Clear();
                }
            }

            public void Restore(Dictionary<string, Material> materialMap)
            {
                foreach (var kv in m_valueMap.ToArray())
                {
                    SetValue(kv.Key, kv.Value, false, materialMap);
                }
            }

            public void Apply()
            {
                foreach (var kv in m_setterMap)
                {
                    kv.Value.Apply();
                }
            }

            public void SetValue(BlendShapeKey key, float value, bool replace, Dictionary<string, Material> materialMap)
            {
                m_valueMap[key] = value;

                BlendShapeClip clip;
                if (!m_clipMap.TryGetValue(key, out clip))
                {
                    return;
                }

                foreach (var binding in clip.Values)
                {
                    BlendShapePathHandler handler;
                    if (m_setterMap.TryGetValue(binding, out handler))
                    {
                        if (replace)
                        {
                            // 値置き換え
                            //handler.ReplaceValue();
                            handler.Apply(binding.Weight * value);
                        }
                        else
                        {
                            // 積算
                            handler.AddValue(binding.Weight * value);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("'{0}' not found", binding);
                    }
                }

                // materialの更新
                foreach(var binding in clip.MaterialValues)
                {
                    var propValue = binding.BaseValue + (binding.TargetValue - binding.BaseValue) * value;
                    materialMap[binding.MaterialName].SetColor(binding.ValueName, propValue);
                }
            }

            public float GetValue(BlendShapeKey key)
            {
                float value;
                if (!m_valueMap.TryGetValue(key, out value))
                {
                    return 0;
                }
                return value;
            }
        }
        BlendShapeMerger m_merger;
        Dictionary<string, Material> m_materialMap;
        private void Awake()
        {
            m_materialMap = new Dictionary<string, Material>();
            foreach(var x in transform.Traverse())
            {
                var renderer = x.GetComponent<Renderer>();
                if (renderer != null)
                {
                    foreach (var y in renderer.sharedMaterials.Where(y => y != null))
                    {
                        if (!string.IsNullOrEmpty(y.name))
                        {
                            if (!m_materialMap.ContainsKey(y.name))
                            {
                                m_materialMap.Add(y.name, y);
                            }
                        }
                    }
                }
            }

#if UNITY_EDITOR
            SetupSliders();
#endif
        }

        private void OnDestroy()
        {
            if (m_materialMap != null)
            {
                foreach (var x in BlendShapeAvatar.Clips)
                {
                    foreach (var y in x.MaterialValues)
                    {
                        // restore values
                        m_materialMap[y.MaterialName].SetColor(y.ValueName, y.BaseValue);
                    }
                }
            }
        }

        private void Update()
        {
            if (BlendShapeAvatar != null)
            {
                if (m_merger == null)
                {
                    m_merger = new BlendShapeMerger(BlendShapeAvatar.Clips, transform);
                }
            }
        }

        public void SetValue(BlendShapePreset key, float value, bool apply = true)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }
        public float GetValue(BlendShapePreset key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(String key, float value, bool apply = true)
        {
            SetValue(new BlendShapeKey(key), value, apply);
        }
        public float GetValue(String key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(BlendShapeKey key, float value, bool apply = true)
        {
            if (m_merger != null)
            {
                m_merger.SetValue(key, value, apply, m_materialMap);
            }
        }
        public float GetValue(BlendShapeKey key)
        {
            if (m_merger == null)
            {
                return 0;
            }
            return m_merger.GetValue(key);
        }

        public void ClearKeys()
        {
            if (m_merger != null)
            {
                m_merger.Clear();
            }
        }

        public void Restore()
        {
            if (m_merger != null)
            {
                m_merger.Restore(m_materialMap);
            }
        }

        public void Apply()
        {
            if (m_merger != null)
            {
                m_merger.Apply();
            }
        }
    }
}

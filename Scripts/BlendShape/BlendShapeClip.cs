using System;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public struct BlendShapeBinding
    {
        public String RelativePath;
        public int Index;
        public float Weight;

        public override string ToString()
        {
            return string.Format("{0}[{1}]=>{2}", RelativePath, Index, Weight);
        }
    }

    [Serializable]
    public struct MaterialValueBinding
    {
        public String MaterialName;
        public String ValueName;
        public Vector4 TargetValue;
        public Vector4 BaseValue;
    }

    [CreateAssetMenu(menuName = "VRM/BlendShapeClip")]
    public class BlendShapeClip : ScriptableObject
    {
        [SerializeField]
        GameObject m_prefab;
        public GameObject Prefab
        {
            set { m_prefab = value; }
            get {
#if UNITY_EDITOR
                if (m_prefab == null)
                {
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                }
#endif
                return m_prefab;
            }
        }

        [SerializeField]
        public string BlendShapeName = "";

        [SerializeField]
        public BlendShapePreset Preset;

        [SerializeField]
        public BlendShapeBinding[] Values = new BlendShapeBinding[] { };

        [SerializeField]
        public MaterialValueBinding[] MaterialValues = new MaterialValueBinding[] { };

        public void Apply(Transform root, float value)
        {
            if (Values != null)
            {
                foreach (var x in Values)
                {
                    var target = root.Find(x.RelativePath);
                    if (target != null)
                    {
                        var sr = target.GetComponent<SkinnedMeshRenderer>();
                        if (sr != null)
                        {
                            sr.SetBlendShapeWeight(x.Index, x.Weight * value);
                        }
                    }
                }
            }

            /*
            if (MaterialValues != null)
            {
                foreach (var x in MaterialValues)
                {
                    var target = root.Find(x.RelativePath);
                    if (target != null)
                    {
                        var sr = target.GetComponent<SkinnedMeshRenderer>();
                        if (sr != null)
                        {
                            var m = sr.sharedMaterials[x.Index];
                            var color = x.BaseValue + (x.TargetValue - x.BaseValue) * value;
                            m.SetColor(x.ValueName, color);
                        }
                    }
                }
            }
            */
        }
    }
}

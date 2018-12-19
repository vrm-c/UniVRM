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
#if UNITY_EDITOR
        /// <summary>
        /// Preview 用のObject参照
        /// </summary>
        [SerializeField]
        GameObject m_prefab;
        public GameObject Prefab
        {
            set { m_prefab = value; }
            get
            {
                if (m_prefab == null)
                {
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        // if asset is subasset of prefab
                        m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        if (m_prefab == null)
                        {
                            var parent = UniGLTF.UnityPath.FromAsset(this).Parent;
                            var prefabPath = parent.Parent.Child(parent.FileNameWithoutExtension + ".prefab");
                            m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath.Value);
                        }
                    }
                }
                return m_prefab;
            }
        }

        /// <summary>
        /// Apply BlendShape for Preview
        /// </summary>
        /// <param name="root"></param>
        /// <param name="value"></param>
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
#endif

        /// <summary>
        /// BlendShapePresetがUnknown場合の識別子
        /// </summary>
        [SerializeField]
        public string BlendShapeName = "";

        /// <summary>
        /// BlendShapePresetを識別する。Unknownの場合は、BlendShapeNameで識別する
        /// </summary>
        [SerializeField]
        public BlendShapePreset Preset;

        /// <summary>
        /// BlendShapeに対する参照(index ベース)
        /// </summary>
        /// <value></value>
        [SerializeField]
        public BlendShapeBinding[] Values = new BlendShapeBinding[] { };

        /// <summary>
        /// マテリアルに対する参照(名前ベース)
        /// </summary>
        /// <value></value>
        [SerializeField]
        public MaterialValueBinding[] MaterialValues = new MaterialValueBinding[] { };

        /// <summary>
        /// UniVRM-0.45: trueの場合、このBlendShapeClipは0と1の間の中間値を取らない。四捨五入する
        /// </summary>
        [SerializeField]
        public bool IsBinary;

        // [SerializeField]
        // public Texture2D Thumbnail;
    }
}

using MeshUtility;
using UnityEngine;


namespace UniVRM10
{
    [CreateAssetMenu(menuName = "VRM10/Expression")]
    public sealed class VRM10Expression : ScriptableObject
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
                        if (m_prefab != null) return m_prefab;

                        var parent = UnityPath.FromUnityPath(assetPath).Parent;
                        var prefabPath = parent.Parent.Child(parent.FileNameWithoutExtension + ".prefab");
                        m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath.Value);
                        if (m_prefab != null) return m_prefab;

                        var parentParent = UnityPath.FromUnityPath(assetPath).Parent.Parent;
                        var vrmPath = parent.Parent.Child(parent.FileNameWithoutExtension + ".vrm");
                        m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(vrmPath.Value);
                        if (m_prefab != null) return m_prefab;
                    }
                }
                return m_prefab;
            }
        }
#endif

        /// <summary>
        /// ExpressionPreset が Unknown 場合の識別子
        /// </summary>
        [SerializeField]
        public string ExpressionName;

        /// <summary>
        /// ExpressionPreset を識別する。 Unknown の場合は、 ExpressionName で識別する
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionPreset Preset;

        /// <summary>
        /// 対象メッシュの Expression を操作する
        /// <summary>
        [SerializeField]
        public MorphTargetBinding[] MorphTargetBindings = new MorphTargetBinding[] { };

        /// <summary>
        /// 対象マテリアルの Color を操作する
        /// <summary>
        [SerializeField]
        public MaterialColorBinding[] MaterialColorBindings = new MaterialColorBinding[] { };

        /// <summary>
        /// 対象マテリアルの UVScale+Offset を操作する
        /// <summary>
        [SerializeField]
        public MaterialUVBinding[] MaterialUVBindings = new MaterialUVBinding[] { };

        /// <summary>
        /// UniVRM-0.45: trueの場合、この Expression は0と1の間の中間値を取らない。四捨五入する
        /// </summary>
        [SerializeField]
        public bool IsBinary;

        /// <summary>
        /// この Expression と Blink(Blink, BlinkLeft, BlinkRight) が同時に有効な場合、Blink の Weight を 0 にする
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType OverrideBlink;

        /// <summary>
        /// この Expression と LookAt(LookUp, LookDown, LookLeft, LookRight) が同時に有効な場合、LookAt の Weight を 0 にする
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType OverrideLookAt;

        /// <summary>
        /// この Expression と Mouth(Aa, Ih, Ou, Ee, Oh) が同時に有効な場合、Mouth の Weight を 0 にする
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType OverrideMouth;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            if (Preset == UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
            {
                if (string.IsNullOrEmpty(ExpressionName))
                {
                    ExpressionName = "custom";
                }
            }
        }
    }
}

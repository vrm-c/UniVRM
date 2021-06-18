using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// ScriptableObject なのだけど Editor では、ヒエラルキーを参照したい。
    /// 参照して、ヒエラルキー内の Renderer を選択したりしたい。
    /// 
    /// そういうオブジェクト。
    /// </summary>
    public class PrefabRelatedScriptableObject : ScriptableObject
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

                        // 同じフォルダに prefab が居る
                        var parent = UnityPath.FromUnityPath(assetPath).Parent;
                        var prefabPath = parent.Parent.Child(parent.FileNameWithoutExtension + ".prefab");
                        m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath.Value);
                        if (m_prefab != null) return m_prefab;

                        // 同じフォルダに vrm(ScriptedImporter) が居る
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
    }
}

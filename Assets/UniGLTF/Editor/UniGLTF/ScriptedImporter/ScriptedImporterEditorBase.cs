using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public abstract class ScriptedImporterEditorBase : ScriptedImporterEditor
    {
        /// <summary>
        /// Apply されていない変更を保持する
        /// </summary>
        /// <typeparam name="SubAssetKey"></typeparam>
        /// <typeparam name="UnityEngine.Object"></typeparam>
        /// <returns></returns>
        [SerializeField]
        List<RemapEditorBase.SubAssetPair> m_editMap = new List<RemapEditorBase.SubAssetPair>();

        protected Dictionary<SubAssetKey, UnityEngine.Object> GetEditorMap()
        {
            return m_editMap.ToDictionary(x => x.Key, x => x.Object);
        }

        protected void SetEditorMap(Dictionary<SubAssetKey, UnityEngine.Object> value)
        {
            Undo.RecordObject(this, "update editorMap");
            m_editMap.Clear();
            m_editMap.AddRange(value.Select(kv => new RemapEditorBase.SubAssetPair(kv.Key, kv.Value)));
        }
    }
}

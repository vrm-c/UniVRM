using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Modules/AssetPipelineEditor/ImportSettings/AssetImporterEditor.cs
    /// 
    /// の作法に合わせる
    /// </summary>
    public abstract class RemapScriptedImporterEditorBase : ScriptedImporterEditor
    {
        protected ScriptedImporter m_importer;

        /// <summary>
        /// Apply されていない変更を保持する
        /// 
        /// * Undo
        /// 
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

        /// <summary>
        /// Revert
        /// </summary>
        protected override void ResetValues()
        {
            m_editMap.Clear();

            base.ResetValues();
        }

        public override bool HasModified()
        {
            if (m_editMap.Any())
            {
                return true;
            }
            return base.HasModified();
        }

        /// <summary>
        /// Apply
        /// </summary>
        protected override void Apply()
        {
            foreach (var kv in m_editMap)
            {
                if (kv.Object != null)
                {
                    m_importer.AddRemap(kv.ID, kv.Object);
                }
                else
                {
                    m_importer.RemoveRemap(kv.ID);
                }
            }
            m_editMap.Clear();
            AssetDatabase.WriteImportSettingsIfDirty(m_importer.assetPath);
            AssetDatabase.ImportAsset(m_importer.assetPath, ImportAssetOptions.ForceUpdate);

            base.Apply();
        }
    }
}

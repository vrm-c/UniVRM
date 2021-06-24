using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public abstract class RemapScriptedImporterEditorBase : ScriptedImporterEditor
    {
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

        public void RevertRemap()
        {
            m_editMap.Clear();
        }

        public void ApplyRemap(ScriptedImporter importer)
        {
            foreach (var kv in m_editMap)
            {
                if (kv.Object != null)
                {
                    importer.AddRemap(kv.ID, kv.Object);
                }
                else
                {
                    importer.RemoveRemap(kv.ID);
                }
            }
            m_editMap.Clear();
            AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
        }

        public void RevertApplyRemapGUI(ScriptedImporter importer)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(m_editMap.Count == 0))
            {
                if (GUILayout.Button("Revert"))
                {
                    RevertRemap();
                }
                if (GUILayout.Button("Apply"))
                {
                    ApplyRemap(importer);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

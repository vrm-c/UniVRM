using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VrmScriptedImporter))]
    public class VrmScriptedImporterEditorGUI : ScriptedImporterEditor
    {

        private bool _isOpen = true;

        public override void OnInspectorGUI()
        {
            var importer = target as VrmScriptedImporter;

            EditorGUILayout.LabelField("Extract settings");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Materials And Textures");
            GUI.enabled = !(importer.GetExternalUnityObjects<UnityEngine.Material>().Any()
                && importer.GetExternalUnityObjects<UnityEngine.Texture2D>().Any());
            if (GUILayout.Button("Extract"))
            {
                importer.ExtractMaterialsAndTextures();
            }
            GUI.enabled = !GUI.enabled;
            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<UnityEngine.Material>();
                importer.ClearExternalObjects<UnityEngine.Texture2D>();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Meta");
            GUI.enabled = !importer.GetExternalUnityObjects<UniVRM10.VRM10MetaObject>().Any();
            if (GUILayout.Button("Extract"))
            {
                importer.ExtractMeta();
            }
            GUI.enabled = !GUI.enabled;
            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<UniVRM10.VRM10MetaObject>();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Expressions");
            GUI.enabled = !(importer.GetExternalUnityObjects<UniVRM10.VRM10ExpressionAvatar>().Any()
                && importer.GetExternalUnityObjects<UniVRM10.VRM10Expression>().Any());
            if (GUILayout.Button("Extract"))
            {
                importer.ExtractExpressions();
            }
            GUI.enabled = !GUI.enabled;
            if (GUILayout.Button("Clear"))
            {
                importer.ClearExternalObjects<UniVRM10.VRM10ExpressionAvatar>();
                importer.ClearExternalObjects<UniVRM10.VRM10Expression>();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // ObjectMap
            DrawRemapGUI<UnityEngine.Material>("Material Remap", importer);
            DrawRemapGUI<UnityEngine.Texture2D>("Texture Remap", importer);
            DrawRemapGUI<UniVRM10.VRM10MetaObject>("Meta Remap", importer);
            DrawRemapGUI<UniVRM10.VRM10ExpressionAvatar>("ExpressionAvatar Remap", importer);
            DrawRemapGUI<UniVRM10.VRM10Expression>("Expression Remap", importer);

            base.OnInspectorGUI();
        }

        private void DrawRemapGUI<T>(string title, VrmScriptedImporter importer) where T : UnityEngine.Object
        {
            EditorGUILayout.Foldout(_isOpen, title);
            EditorGUI.indentLevel++;
            var objects = importer.GetExternalObjectMap().Where(x => x.Key.type == typeof(T));
            foreach (var obj in objects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(obj.Key.name);
                var asset = EditorGUILayout.ObjectField(obj.Value, obj.Key.type, true) as T;
                if (asset != obj.Value)
                {
                    importer.SetExternalUnityObject(obj.Key, asset);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
    }
}

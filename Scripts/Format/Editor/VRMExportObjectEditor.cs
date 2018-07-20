using System;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMExportObject))]
    public class VRMExportObjectEditor : Editor
    {
        SerializedProperty m_settings;
        VRMExportObject m_target;

        void OnEnable()
        {
            m_target = target as VRMExportObject;
            m_settings = serializedObject.FindProperty("Settings");
        }

        public override void OnInspectorGUI()
        {
            //
            // Editor
            //
            serializedObject.Update();

            var before = m_target.Settings.Source;

            EditorGUILayout.PropertyField(m_settings, true);
            serializedObject.ApplyModifiedProperties();

            //
            // 
            //
            var after = m_target.Settings.Source;
            if (before != after)
            {
                m_target.Settings.InitializeFrom(after as GameObject);
            }

            bool canExport = m_target.Settings.Source != null;
            foreach (var msg in m_target.Settings.CanExport())
            {
                canExport = false;
                EditorGUILayout.HelpBox(msg, MessageType.Error);
            }

            if (canExport)
            {
                if (GUILayout.Button("Export"))
                {
                    var path = EditorUtility.SaveFilePanel(
                            "Save vrm",
                            null,//Dir,
                            m_target.Settings.Source.name + ".vrm",
                            "vrm");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Export(path);
                    }
                }
            }
        }

        class DisposableInstance : IDisposable
        {
            GameObject m_go;
            public GameObject GameObject
            {
                get
                {
                    return m_go;
                }
            }

            public DisposableInstance(GameObject prefab)
            {
                m_go = GameObject.Instantiate(prefab);
            }

            public void Dispose()
            {
                if (m_go != null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(m_go);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(m_go);
                    }
                }
            }
        }

        void Export(string path)
        {
            Debug.LogFormat("export: {0}", path);

            /*
            using (var instance = new DisposableInstance(m_source.objectReferenceValue as GameObject))
            {
                var vrm = VRMExporter.Export(instance.GameObject);
                var bytes = vrm.ToGlbBytes();
                File.WriteAllBytes(path, bytes);
            }
            */
        }
    }
}

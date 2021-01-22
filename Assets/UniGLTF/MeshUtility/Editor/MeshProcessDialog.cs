using UnityEngine;
using UnityEditor;
using System.Reflection;
using MeshUtility;
using System.IO;
using System;
using System.Linq;

namespace MeshUtility
{
    public class MeshProcessDialog : EditorWindow
    {
        const string MESH_UTILITY_DICT = "UniGLTF/Mesh Utility/";

        [MenuItem(MESH_UTILITY_DICT + "MeshProcessing Wizard", priority = 30)]
        static void MeshProcessFromMenu()
        {
            var window = (MeshProcessDialog)EditorWindow.GetWindowWithRect(typeof(MeshProcessDialog), new Rect(0, 0, 500, 250));
            window.titleContent = new GUIContent ("Mesh Processing Window");       
        }
        
        enum Tabs
        {
            MeshSeparator,
            MeshIntegrator,
            StaticMeshIntegrator,
        }
        private Tabs _tab;

        private GameObject _exportTarget;
        private MethodInfo _processFunction;
        private bool _isInvokeSuccess = false;

        GUIStyle _tabButtonStyle => "LargeButton";
        GUI.ToolbarButtonSize _tabButtonSize => GUI.ToolbarButtonSize.Fixed;

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 150;
            _tab = TabBar.OnGUI(_tab, _tabButtonStyle, _tabButtonSize);

            switch (_tab)
            {
                case Tabs.MeshSeparator:
                    EditorGUILayout.TextField("Meshes containing BlendShape data will be split");
                    break;
                case Tabs.MeshIntegrator:
                    EditorGUILayout.TextField("Generate a single mesh. Meshes w/ BlendShape will be grouped into another one");
                    break;
                case Tabs.StaticMeshIntegrator:
                    EditorGUILayout.TextField("Integrate static meshes into one");
                    break;
            }

            EditorGUILayout.LabelField("ExportTarget");
            _exportTarget = (GameObject)EditorGUILayout.ObjectField(_exportTarget, typeof(GameObject), true);
            if (_exportTarget == null && MeshUtility.IsGameObjectSelected())
            {
                _exportTarget = Selection.activeObject as GameObject;
            }

            // Create Other Buttons
            {        
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                    {
                        switch (_tab)
                        {
                            case Tabs.MeshSeparator:
                                _isInvokeSuccess = InvokeWizardUpdate("MeshSeparator");
                                break;
                            case Tabs.MeshIntegrator:
                                _isInvokeSuccess = InvokeWizardUpdate("MeshIntegrator");
                                break;
                            case Tabs.StaticMeshIntegrator:
                                _isInvokeSuccess = InvokeWizardUpdate("StaticMeshIntegrator");
                                break;
                        }
                        if (_isInvokeSuccess)
                        {
                            Close();
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private bool InvokeWizardUpdate(string processFuntion)
        {
            const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            _processFunction = GetType().GetMethod(processFuntion, kInstanceInvokeFlags);
            if (_processFunction != null)
            {
                return (Boolean)_processFunction.Invoke(this, null);
            }
            else
            {
                Debug.LogError("This function has not been implemented in script");
                return false;
            }
        }

        private bool GameObjectNull()
        {
            EditorUtility.DisplayDialog("Failed", "No GameObject Selected", "ok");
            return false;
        }

        private bool MeshSeparator()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;

            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0)
            {
                MeshUtility.SeparationProcessing(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "No skinned mesh contained", "ok");
                return false;
            }
        }

        private bool MeshIntegrator()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;
            
            Component[] allComponents =  go.GetComponents(typeof(Component));
            var keyWord = "VRMMeta";

            foreach (var component in allComponents)
            {          
                if (component == null) continue;      
                var sourceString = component.ToString();
                if (sourceString.Contains(keyWord))
                {
                    EditorUtility.DisplayDialog("Failed", "Target object is VRM file, use `VRM0 -> MeshIntegrator` instead", "ok");
                    return false;
                }
            }

            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0 || go.GetComponentsInChildren<MeshFilter>().Length > 0)
            {
                MeshUtility.MeshIntegrator(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Neither skinned mesh nor static mesh contained", "ok");
                return false;
            }
        }

        private bool StaticMeshIntegrator()
        {
            if (_exportTarget == null) return GameObjectNull();
            var go = _exportTarget;
            if (go.GetComponentsInChildren<MeshFilter>().Length > 0)
            {
                MeshUtility.IntegrateSelected(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "No static mesh contained", "ok");
                return false;
            }
        }
    }
}
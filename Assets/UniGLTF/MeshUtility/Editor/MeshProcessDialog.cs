using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using MeshUtility.M17N;

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

        private enum MeshProcessingMessages
        {
            [LangMsg(Languages.ja, "ターゲットオブジェクト")]
            [LangMsg(Languages.en, "TargetObject")]
            TARGET_OBJECT,

            [LangMsg(Languages.ja, "BlendShapeを含むメッシュは分割されます")]
            [LangMsg(Languages.en, "Meshes containing BlendShape will be split")]
            MESH_SEPARATOR,

            [LangMsg(Languages.ja, "メッシュを統合する。BlendShapeを含むメッシュは独立して統合されます")]
            [LangMsg(Languages.en, "Generate a single mesh. Meshes w/ BlendShape will be grouped into another one")]
            MESH_INTEGRATOR,

            [LangMsg(Languages.ja, "静的メッシュを一つに統合する")]
            [LangMsg(Languages.en, "Integrate static meshes into one")]
            STATIC_MESH_INTEGRATOR,

            [LangMsg(Languages.ja, "GameObjectを選んでください")]
            [LangMsg(Languages.en, "Select a GameObject first")]
            NO_GAMEOBJECT_SELECTED,

            [LangMsg(Languages.ja, "GameObjectにスキンメッシュが含まれていません")]
            [LangMsg(Languages.en, "No skinned mesh is contained")]
            NO_SKINNED_MESH,

            [LangMsg(Languages.ja, "GameObjectに静的メッシュが含まれていません")]
            [LangMsg(Languages.en, "No static mesh is contained")]
            NO_STATIC_MESH,

            [LangMsg(Languages.ja, "GameObjectにスキンメッシュ・静的メッシュが含まれていません")]
            [LangMsg(Languages.en, "Skinned/Static mesh is not contained")]
            NO_MESH,

            [LangMsg(Languages.ja, "ターゲットオブジェクトはVRMモデルです。`VRM0-> MeshIntegrator`を使ってください")]
            [LangMsg(Languages.en, "Target object is VRM model, use `VRM0 -> MeshIntegrator` instead")]
            VRM_DETECTED,
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 150;
            // lang
            Getter.OnGuiSelectLang();

            _tab = TabBar.OnGUI(_tab, _tabButtonStyle, _tabButtonSize);

            switch (_tab)
            {
                case Tabs.MeshSeparator:
                    EditorGUILayout.TextField(MeshProcessingMessages.MESH_SEPARATOR.Msg());
                    break;
                case Tabs.MeshIntegrator:
                    EditorGUILayout.TextField(MeshProcessingMessages.MESH_INTEGRATOR.Msg());
                    break;
                case Tabs.StaticMeshIntegrator:
                    EditorGUILayout.TextField(MeshProcessingMessages.STATIC_MESH_INTEGRATOR.Msg());
                    break;
            }

            EditorGUILayout.LabelField(MeshProcessingMessages.TARGET_OBJECT.Msg());
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
            EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
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
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_SKINNED_MESH.Msg(), "ok");
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
                    EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.VRM_DETECTED.Msg(), "ok");
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
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_MESH.Msg(), "ok");
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
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_STATIC_MESH.Msg(), "ok");
                return false;
            }
        }
    }
}
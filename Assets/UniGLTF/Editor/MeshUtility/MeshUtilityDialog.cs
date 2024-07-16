using UnityEngine;
using UnityEditor;
using UniGLTF.M17N;
using System.Collections.Generic;
using System.Linq;
using System;


namespace UniGLTF.MeshUtility
{
    public class MeshUtilityDialog : EditorWindow
    {
        public const string MENU_NAME = "glTF MeshUtility";
        protected const string ASSET_SUFFIX = ".mesh.asset";
        public static void OpenWindow()
        {
            var window =
                (MeshUtilityDialog)EditorWindow.GetWindow(typeof(MeshUtilityDialog));
            window.titleContent = new GUIContent(MENU_NAME);
            window.Show();
        }

        protected enum Tabs
        {
            Freeze,
            IntegrateSplit,
        }
        protected Tabs _tab;
        protected GameObject _exportTarget;

        GltfMeshUtility _meshUtil;
        protected virtual GltfMeshUtility MeshUtility
        {
            get
            {
                if (_meshUtil == null)
                {
                    _meshUtil = new GltfMeshUtility();
                }
                return _meshUtil;
            }
        }
        MeshIntegrationTab _integrationTab;
        protected virtual MeshIntegrationTab MeshIntegration
        {
            get
            {
                if (_integrationTab == null)
                {
                    _integrationTab = new MeshIntegrationTab(this, MeshUtility);
                }
                return _integrationTab;
            }
        }

        protected List<Validation> _validations = new List<Validation>();
        protected virtual void Validate()
        {
            _validations.Clear();
            if (_exportTarget == null)
            {
                _validations.Add(Validation.Error("set target GameObject"));
                return;
            }
        }
        bool IsValid => !_validations.Any(v => !v.CanExport);

        MeshInfo[] integrationResults;

        Vector2 _scrollPos;

        void OnEnable()
        {
        }

        protected virtual void DialogMessage()
        {
            EditorGUILayout.HelpBox(MeshUtilityMessages.MESH_UTILITY.Msg(), MessageType.Info);
        }

        private void OnGUI()
        {
            var modified = false;
            EditorGUIUtility.labelWidth = 200;
            LanguageGetter.OnGuiSelectLang();

            DialogMessage();

            var exportTarget = (GameObject)EditorGUILayout.ObjectField(
                MeshUtilityMessages.TARGET_OBJECT.Msg(),
                _exportTarget, typeof(GameObject), true);
            if (exportTarget != _exportTarget)
            {
                _exportTarget = exportTarget;
                MeshIntegration.UpdateMeshIntegrationList(_exportTarget);
                modified = true;
            }
            if (_exportTarget == null)
            {
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // GameObject or Prefab ?
            switch (_exportTarget.GetPrefabType())
            {
                case UnityExtensions.PrefabType.PrefabAsset:
                    EditorGUILayout.HelpBox(MeshUtilityMessages.PREFAB_ASSET.Msg(), MessageType.Warning);
                    break;

                case UnityExtensions.PrefabType.PrefabInstance:
                    EditorGUILayout.HelpBox(MeshUtilityMessages.PREFAB_INSTANCE.Msg(), MessageType.Warning);
                    break;
            }

            // tab bar
            _tab = TabBar.OnGUI(_tab, "LargeButton", GUI.ToolbarButtonSize.Fixed);

            foreach (var validation in _validations)
            {
                validation.DrawGUI();
            }

            switch (_tab)
            {
                case Tabs.Freeze:
                    {
                        if (MeshFreezeGui())
                        {
                            modified = true;
                        }
                        break;
                    }

                case Tabs.IntegrateSplit:
                    {
                        if (MeshIntegrateGui())
                        {
                            modified = true;
                        }
                        break;
                    }

                    // TODO:
                    // Mesh統合のオプション
                    // case Tabs.BoneMeshEraser:
                    //     {
                    //         // TODO: FirstPerson 処理と統合する
                    //         EditorGUILayout.HelpBox(MeshUtilityMessages.BONE_MESH_ERASER.Msg(), MessageType.Info);
                    //         // if (_boneMeshEraserEditor)
                    //         // {
                    //         //     _boneMeshEraserEditor.OnInspectorGUI();
                    //         // }
                    //         // if (TabBoneMeshRemover.TryExecutable(_exportTarget, _skinnedMeshRenderer, out string msg))
                    //         // {
                    //         //     processed = TabBoneMeshRemover.OnGUI(_exportTarget, _skinnedMeshRenderer, _eraseBones);
                    //         // }
                    //         // else
                    //         // {
                    //         //     EditorGUILayout.HelpBox(msg, MessageType.Error);
                    //         // }
                    //         break;
                    //     }
            }
            EditorGUILayout.EndScrollView();

            if (modified)
            {
                Validate();
            }

            GUI.enabled = IsValid;
            var pressed = GUILayout.Button("Process", GUILayout.MinWidth(100));
            GUI.enabled = true;
            if (pressed)
            {
                if (_exportTarget.GetPrefabType() == UnityExtensions.PrefabType.PrefabAsset)
                {
                    /// [prefab]
                    /// 
                    /// * prefab から instance を作る
                    /// * instance に対して 焼き付け, 統合, 分離 を実行する
                    ///   * instance のヒエラルキーが改変され、mesh 等のアセットは改変版が作成される(元は変わらない)
                    /// * instance を asset に保存してから prefab を削除して終了する
                    /// 
                    UnityPath assetFolder = default;
                    try
                    {
                        assetFolder = PrefabContext.GetOutFolder(_exportTarget);
                    }
                    catch (Exception)
                    {
                        EditorUtility.DisplayDialog("asset folder", "Target folder must be in the Assets or writable Packages folder", "cancel");
                        return;
                    }

                    using (var context = new PrefabContext(_exportTarget, assetFolder))
                    {
                        try
                        {
                            // prefab が instantiate されていた場合に
                            // Mesh統合設定を instantiate に置き換える
                            var groupCopy = MeshUtility.CopyInstantiate(_exportTarget, context.Instance);

                            var (results, created) = MeshUtility.Process(context.Instance, groupCopy);

                            // TODO: this should be replaced export and reimport ?
                            WriteAssets(context.AssetFolder, context.Instance, results);
                            WritePrefab(context.AssetFolder, context.Instance);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Debug.LogException(ex, context.Instance);
                            context.Keep = true;
#endif
                        }
                    }
                }
                else
                {
                    using (var context = new UndoContext("MeshUtility", _exportTarget))
                    {
                        var (results, created) = MeshUtility.Process(_exportTarget, MeshUtility.MeshIntegrationGroups);
                        MeshUtility.Clear(results);

                        foreach (var go in created)
                        {
                            // 処理後の mesh をアタッチした Renderer.gameobject
                            Undo.RegisterCreatedObjectUndo(go, "MeshUtility");
                        }
                    }
                }

                // TODO: Show Result ?
                _exportTarget = null;
            }
        }

        Mesh WriteAndReload(Mesh src, string assetPath)
        {
            UniGLTFLogger.Log($"CreateAsset: {assetPath}");
            AssetDatabase.CreateAsset(src, assetPath);
            var unityPath = UnityPath.FromUnityPath(assetPath);
            unityPath.ImportAsset();
            var mesh = unityPath.LoadAsset<Mesh>();
            return mesh;
        }

        /// <summary>
        /// Write Mesh
        /// </summary>
        protected virtual void WriteAssets(string assetFolder, GameObject instance, List<MeshIntegrationResult> results)
        {
            foreach (var result in results)
            {
                if (result.Integrated != null)
                {
                    var childAssetPath = $"{assetFolder}/{result.Integrated.IntegratedRenderer.gameObject.name}{ASSET_SUFFIX}";
                    result.Integrated.IntegratedRenderer.sharedMesh = WriteAndReload(
                        result.Integrated.IntegratedRenderer.sharedMesh, childAssetPath);
                }
                if (result.IntegratedNoBlendShape != null)
                {
                    var childAssetPath = $"{assetFolder}/{result.IntegratedNoBlendShape.IntegratedRenderer.gameObject.name}{ASSET_SUFFIX}";
                    result.IntegratedNoBlendShape.IntegratedRenderer.sharedMesh = WriteAndReload(
                        result.IntegratedNoBlendShape.IntegratedRenderer.sharedMesh, childAssetPath);
                }
            }

            MeshUtility.Clear(results);
        }

        /// <summary>
        /// Write Prefab
        /// </summary>
        protected virtual string WritePrefab(string assetFolder, GameObject instance)
        {
            var prefabPath = $"{assetFolder}/Integrated.prefab";
            UniGLTFLogger.Log(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool success);
            if (!success)
            {
                throw new Exception($"PrefabUtility.SaveAsPrefabAsset: {prefabPath}");
            }
            return prefabPath;
        }

        protected bool ToggleIsModified(string label, ref bool value)
        {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue == value)
            {
                return false;
            }
            value = newValue;
            return true;
        }

        bool MeshFreezeGui()
        {
            return ToggleIsModified("BlendShapeRotationScaling", ref MeshUtility.FreezeBlendShapeRotationAndScaling);
        }

        protected virtual bool MeshIntegrateGui()
        {
            var split = ToggleIsModified("Separate by BlendShape", ref MeshUtility.SplitByBlendShape);
            var p = position;
            var last = GUILayoutUtility.GetLastRect();
            var y = last.y + last.height;
            var rect = new Rect
            {
                x = last.x,
                y = y,
                width = p.width,
                height = p.height - y
                // process button の高さ
                - 30
            };
            var mod = MeshIntegration.OnGui(rect);
            return split || mod;
        }
    }
}
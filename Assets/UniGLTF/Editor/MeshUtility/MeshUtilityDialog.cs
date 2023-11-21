using UnityEngine;
using UnityEditor;
using UniGLTF.M17N;
using System.Collections.Generic;
using System.Linq;
using System.IO;


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
            // Clear(HelpMessage.Ready, ValidationError.None);
            // OnValidate();
        }

        // void Clear(HelpMessage help, ValidationError error)
        // {
        //     helpString = help.Msg();
        //     errorString = error != ValidationError.None ? error.Msg() : null;
        //     m_uniqueMaterials = new Material[] { };
        //     m_duplicateMaterials = new MaterialList[] { };
        //     m_excludes.Clear();
        //     isValid = false;
        // }

        // void OnValidate()
        // {
        //     isValid = false;
        //     if (m_root == null)
        //     {
        //         Clear(HelpMessage.SetTarget, ValidationError.NoTarget);
        //         return;
        //     }

        //     if (m_root.GetGameObjectType() != GameObjectType.AssetPrefab)
        //     {
        //         Clear(HelpMessage.SetTarget, ValidationError.NotPrefab);
        //         return;
        //     }

        //     if (m_root.transform.parent != null)
        //     {
        //         Clear(HelpMessage.InvalidTarget, ValidationError.HasParent);
        //         return;
        //     }

        //     var backup = m_excludes.ToArray();
        //     Clear(HelpMessage.Ready, ValidationError.None);
        //     isValid = true;
        //     m_uniqueMaterials = MeshIntegratorUtility.EnumerateSkinnedMeshRenderer(m_root.transform, MeshEnumerateOption.OnlyWithoutBlendShape)
        //         .SelectMany(x => x.sharedMaterials)
        //         .Distinct()
        //         .ToArray();

        //     m_duplicateMaterials = m_uniqueMaterials
        //         .GroupBy(x => GetMaterialKey(x), x => x)
        //         .Select(x => new MaterialList(x.ToArray()))
        //         .Where(x => x.Materials.Length > 1)
        //         .ToArray()
        //         ;

        //     UpdateExcludes(backup);
        // }

        // void UpdateExcludes(ExcludeItem[] backup)
        // {
        //     var exclude_map = new Dictionary<Mesh, ExcludeItem>();
        //     var excludes = new List<ExcludeItem>();
        //     foreach (var x in m_root.GetComponentsInChildren<Renderer>())
        //     {
        //         var mesh = x.GetMesh();
        //         if (mesh == null)
        //         {
        //             continue;
        //         }
        //         if (exclude_map.ContainsKey(mesh))
        //         {
        //             continue;
        //         }

        //         var item = new ExcludeItem
        //         {
        //             Mesh = mesh,
        //         };
        //         var found = backup.FirstOrDefault(y => y.Mesh == mesh);
        //         if (found != null)
        //         {
        //             item.Exclude = found.Exclude;
        //         }
        //         excludes.Add(item);
        //         exclude_map[mesh] = item;
        //     }
        //     m_excludes.AddRange(excludes);
        // }

        /// <summary>
        /// Scene と Prefab で挙動をスイッチする。
        /// 
        /// - Scene: ヒエラルキーを操作する。Asset の 書き出しはしない。UNDO はする。TODO: 明示的な Asset の書き出し。
        /// - Prefab: 対象をコピーして処理する。Undo は実装しない。結果を Asset として書き出し、処理後にコピーは削除する。
        /// 
        /// </summary>
        bool TargetIsPrefab => _exportTarget != null && _exportTarget.scene.name == null;

        protected virtual void DialogMessage()
        {
            EditorGUILayout.HelpBox(MeshUtilityMessages.MESH_UTILITY.Msg(), MessageType.Info);
        }

        private void OnGUI()
        {
            var modified = false;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
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

            // GameObject or Prefab ?
            if (TargetIsPrefab)
            {
                EditorGUILayout.HelpBox(MeshUtilityMessages.PREFAB_TARGET.Msg(), MessageType.Warning);
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
                if (TargetIsPrefab)
                {
                    /// [prefab]
                    /// 
                    /// * backup するのではなく 変更した copy を作成する。元は変えない
                    ///   * copy 先の統合前の renderer を disable で残さず destroy する
                    /// * 実行すると mesh, blendshape, blendShape を新規に作成する
                    /// * 新しいヒエラルキーを prefab に保存してから削除して終了する

                    // 出力フォルダを決める
                    var folder = "Assets";
                    var prefab = _exportTarget.GetPrefab();
                    if (prefab != null)
                    {
                        folder = AssetDatabase.GetAssetPath(prefab);
                        // Debug.Log(folder);
                    }
                    // 新規で作成されるアセットはすべてこのフォルダの中に作る。上書きチェックはしない
                    var assetFolder = EditorUtility.SaveFolderPanel("select asset save folder", Path.GetDirectoryName(folder), "VrmIntegrated");
                    var unityPath = UniGLTF.UnityPath.FromFullpath(assetFolder);
                    if (!unityPath.IsUnderWritableFolder)
                    {
                        EditorUtility.DisplayDialog("asset folder", "Target folder must be in the Assets or writable Packages folder", "cancel");
                        return;
                    }
                    assetFolder = unityPath.Value;

                    var copy = GameObject.Instantiate(_exportTarget);

                    var (results, created) = MeshUtility.Process(copy);

                    WriteAssets(copy, assetFolder, results);

                    // destroy scene
                    UnityEngine.Object.DestroyImmediate(copy);
                }
                else
                {
                    Undo.RegisterFullObjectHierarchyUndo(_exportTarget, "MeshUtility");
                    var (results, created) = MeshUtility.Process(_exportTarget);
                    foreach (var go in created)
                    {
                        Undo.RegisterCreatedObjectUndo(go, "MeshUtility");
                    }
                }

                // TODO: Show Result ?
                _exportTarget = null;
            }
        }

        void WriteAssets(GameObject copy, string assetFolder, List<MeshIntegrationResult> results)
        {
            //
            // write mesh asset
            foreach (var result in results)
            {
                var childAssetPath = $"{assetFolder}/{result.Integrated.IntegratedRenderer.gameObject.name}{ASSET_SUFFIX}";
                Debug.LogFormat("CreateAsset: {0}", childAssetPath);
                AssetDatabase.CreateAsset(result.Integrated.IntegratedRenderer.sharedMesh, childAssetPath);
            }

            // 統合した結果をヒエラルキーに追加する
            foreach (var result in results)
            {
                if (result.Integrated.IntegratedRenderer != null)
                {
                    result.Integrated.IntegratedRenderer.transform.SetParent(copy.transform, false);
                }
            }

            // 統合した結果を反映した BlendShapeClip を作成して置き換える
            // var clips = VRMMeshIntegratorUtility.FollowBlendshapeRendererChange(results, copy, assetFolder);

            // 用が済んだ 統合前 の renderer を削除する
            foreach (var result in results)
            {
                foreach (var renderer in result.SourceMeshRenderers)
                {
                    GameObject.DestroyImmediate(renderer);
                }
                foreach (var renderer in result.SourceSkinnedMeshRenderers)
                {
                    GameObject.DestroyImmediate(renderer);
                }
            }

            // reset firstperson
            // var firstperson = copy.GetComponent<VRMFirstPerson>();
            // if (firstperson != null)
            // {
            //     firstperson.Reset();
            // }

            // prefab
            var prefabPath = $"{assetFolder}/VrmIntegrated.prefab";
            Debug.Log(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(copy, prefabPath, out bool success);
            if (!success)
            {
                throw new System.Exception($"PrefabUtility.SaveAsPrefabAsset: {prefabPath}");
            }

            // var prefabReference = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            // foreach (var clip in clips)
            // {
            //     var so = new SerializedObject(clip);
            //     so.Update();
            //     // clip.Prefab = copy;
            //     var prop = so.FindProperty("m_prefab");
            //     prop.objectReferenceValue = prefabReference;
            //     so.ApplyModifiedProperties();
            // }
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
            var forceUniqueName = ToggleIsModified("ForceUniqueName", ref MeshUtility.ForceUniqueName);
            var blendShape = ToggleIsModified("BlendShape", ref MeshUtility.FreezeBlendShape);
            var scale = ToggleIsModified("Scale", ref MeshUtility.FreezeScaling);
            var rotation = ToggleIsModified("Rotation", ref MeshUtility.FreezeRotation);
            return forceUniqueName || blendShape || scale || rotation;
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
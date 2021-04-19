using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public delegate IEnumerable<Validation> Validator(GameObject root);

    /// <summary>
    /// Exportダイアログの共通機能
    /// 
    /// * ExportRoot 管理
    /// * Invalidate 管理
    /// 
    /// </summary>
    public class ExporterDialogState : IDisposable
    {
        public void Dispose()
        {
            ExportRoot = null;
            ExportRootChanged = null;
        }

        #region ExportRoot管理
        (GameObject GameObject, bool IsPrefab) m_root;
        public event Action<GameObject> ExportRootChanged;
        void RaiseExportRootChanged()
        {
            var tmp = ExportRootChanged;
            if (tmp == null) return;
            tmp(m_root.GameObject);
        }
        public GameObject ExportRoot
        {
            get { return m_root.GameObject; }
            set
            {
                string assetPath = default;
                var isPrefab = false;
                if (value != null && AssetDatabase.IsMainAsset(value))
                {
                    assetPath = AssetDatabase.GetAssetPath(value);
                    try
                    {
                        var prefab = PrefabUtility.LoadPrefabContents(assetPath);
                        value = prefab;
                        isPrefab = true;
                    }
                    catch (ArgumentException)
                    {
                        // Debug.LogWarning(ex);
                    }
                }
                if (m_root.GameObject == value)
                {
                    return;
                }
                if (m_root.IsPrefab)
                {
#if VRM_DEVELOP
                    Debug.Log($"PrefabUtility.UnloadPrefabContents({m_root.GameObject})");
#endif
                    PrefabUtility.UnloadPrefabContents(m_root.GameObject);
                }
                m_root = (value, isPrefab);
                m_requireValidation = true;
                RaiseExportRootChanged();
            }
        }
        #endregion

        #region Validation管理
        bool m_requireValidation = true;
        public void Invalidate()
        {
            // UpdateRoot(ExportRoot);
            m_requireValidation = true;
        }

        List<Validation> m_validations = new List<Validation>();
        public IReadOnlyList<Validation> Validations => m_validations;

        /// <summary>
        /// EditorWindow.OnGUI から
        /// 
        /// if (Event.current.type == EventType.Layout)
        /// {
        ///     m_state.Validate(ValidatorFactory());
        /// }
        /// 
        /// IEnumerable<MeshUtility.Validator> ValidatorFactory()
        /// {
        ///     yield break;
        /// }
        ///
        /// のように呼び出してね
        /// </summary>
        public void Validate(IEnumerable<Validator> validators)
        {
            if (m_requireValidation)
            {
                m_validations.Clear();
                m_requireValidation = false;
                foreach (var validator in validators)
                {
                    foreach (var validation in validator(ExportRoot))
                    {
                        try
                        {
                            m_validations.Add(validation);
                            if (validation.ErrorLevel == ErrorLevels.Critical)
                            {
                                // 打ち切り
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            // ERROR
                            m_validations.Add(Validation.Critical(ex.Message));
                            return;
                        }
                    }
                }
            }
        }
        #endregion
    }
}

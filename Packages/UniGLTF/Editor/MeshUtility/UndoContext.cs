using System;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    // Instantiate
    class UndoContext : IDisposable
    {
        public UndoContext(string undoName, GameObject go)
        {
            Undo.RegisterFullObjectHierarchyUndo(go, undoName);
            if (go.GetPrefabType() == UnityExtensions.PrefabType.PrefabInstance)
            {
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        public void Dispose()
        {
            // 特に何もしない
            // Undo すると元に戻ってしまう

            // TODO: あれば一時オブジェクトの破棄
        }
    }
}

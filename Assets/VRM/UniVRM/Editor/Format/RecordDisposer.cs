using System;
using UnityEditor;

namespace VRM
{
    /// <summary>
    /// UndoをGroupを開始して、DisposeでUndoする。
    /// using で使うのを想定。
    /// using ブロック内で Undo されるべき操作をする。
    /// </summary>
    public struct RecordDisposer : IDisposable
    {
        int _group;
        public RecordDisposer(UnityEngine.Object[] objects, string msg)
        {
            Undo.IncrementCurrentGroup();
            _group = Undo.GetCurrentGroup();
            Undo.RecordObjects(objects, msg);
        }

        public void Dispose()
        {
            Undo.RevertAllDownToGroup(_group);
        }
    }
}

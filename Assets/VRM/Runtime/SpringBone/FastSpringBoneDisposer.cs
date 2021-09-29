using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// FastSpringBoneに関連して、特定のGameObjectと紐付いたIDisposableの破棄を担当するクラス
    /// </summary>
    public sealed class FastSpringBoneDisposer : MonoBehaviour
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
        
        private void OnDestroy()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
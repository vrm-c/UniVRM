using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF.SpringBoneJobs
{
    /// <summary>
    /// FastSpringBoneに関連して、特定のGameObjectと紐付いたIDisposableの破棄を担当するクラス
    /// </summary>
    public sealed class FastSpringBoneDisposer : MonoBehaviour
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        class Disposable : IDisposable
        {
            Action m_onDispose;
            Disposable(Action action)
            {
                m_onDispose = action;
            }
            public static IDisposable Create(Action action)
            {
                return new Disposable(action);
            }
            public void Dispose()
            {
                m_onDispose();
            }
        }

        public FastSpringBoneDisposer Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
            return this;
        }

        public FastSpringBoneDisposer AddAction(Action action)
        {
            _disposables.Add(Disposable.Create(action));
            return this;
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
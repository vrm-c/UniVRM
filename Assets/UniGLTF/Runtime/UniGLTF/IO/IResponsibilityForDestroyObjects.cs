using System;

namespace UniGLTF
{
    public delegate void TakeResponsibilityForDestroyObjectFunc(SubAssetKey key, UnityEngine.Object obj);

    /// <summary>
    /// UnityObjectを破棄する責務。
    /// 
    /// この interface を実装するクラスは、利用後に破棄すべき UnityObject を保持する可能性があるので
    /// Dispose により解放すること。
    /// 
    /// [Runtime] TransferOwnership により、破棄責任を RuntimeGltfInstance に移譲する。
    ///   RuntimeGltfInstance.OnDestroy でこれを破棄する。
    /// [Editor] TransferOwnership により、Asset化して破棄しない
    ///   DestroyするとAssetが消えてしまう。
    /// 
    /// </summary>
    public interface IResponsibilityForDestroyObjects : IDisposable
    {
        void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take);
    }
}

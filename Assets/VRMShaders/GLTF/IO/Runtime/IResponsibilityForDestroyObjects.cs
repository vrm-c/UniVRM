using System;

namespace VRMShaders
{
    public delegate void TakeResponsibilityForDestroyObjectFunc(SubAssetKey key, UnityEngine.Object obj);

    /// <summary>
    /// UnityObjectを破棄する責務。
    /// 
    /// この interface を実装するクラスは、利用後に破棄すべき UnityObject を保持する可能性があるので
    /// Dispose により解放すること。
    /// 
    /// TransferOwnership により、破棄責任を移譲することができる。
    /// 
    /// </summary>
    public interface IResponsibilityForDestroyObjects : IDisposable
    {
        void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take);
    }
}

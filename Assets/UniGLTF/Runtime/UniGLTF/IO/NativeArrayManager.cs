using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;

namespace UniGLTF
{
    /// <summary>
    /// 特定のコンテキスト(GltfDataなど)に関連する、NativeArrayの作成を代行し、
    /// まとめてDisposeできるようにする。
    /// 
    /// 例えば indexバッファー NativeArray<uint> の元が NativeArray<ushort> である場合に
    /// 新規に NativeArray を作成し Dispose 対象として管理する必要がある。
    /// また、Sparse や base64 encoding など単純なバイト列のスライスで済まない場合も同様である。
    /// 
    /// </summary>
    public class NativeArrayManager : IDisposable
    {
        List<IDisposable> m_disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var disposable in m_disposables)
            {
                disposable.Dispose();
            }
            m_disposables.Clear();
        }


        /// <summary>
        /// NativeArrayを新規作成し、Dispose管理する。
        /// 個別にDisposeする必要が無い。
        /// </summary>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public NativeArray<T> CreateNativeArray<T>(int size) where T : struct
        {
            var array = new NativeArray<T>(size, Allocator.Persistent);
            m_disposables.Add(array);
            return array;
        }

        public NativeArray<T> CreateNativeArray<T>(ArraySegment<T> data) where T : struct
        {
            var array = CreateNativeArray<T>(data.Count);
            // TODO: remove ToArray
            array.CopyFrom(data.ToArray());
            return array;
        }

        public NativeArray<T> CreateNativeArray<T>(T[] data) where T : struct
        {
            var array = CreateNativeArray<T>(data.Length);
            array.CopyFrom(data);
            return array;
        }
    }
}

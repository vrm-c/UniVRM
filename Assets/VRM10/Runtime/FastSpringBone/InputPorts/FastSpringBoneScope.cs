using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10.FastSpringBones.System
{
    /// <summary>
    /// 1キャラに対応するFastSpringBoneのスコープ
    /// </summary>
    public class FastSpringBoneScope : IDisposable
    {
        public IReadOnlyList<FastSpringBoneSpring> Springs { get; }

        public FastSpringBoneScope(IReadOnlyList<FastSpringBoneSpring> springs)
        {
            Springs = springs;
            
            FastSpringBoneService.Instance.BufferCombiner.Register(this);
        }

        public void Dispose()
        {
            FastSpringBoneService.Instance.BufferCombiner.Unregister(this);
        }
    }
}
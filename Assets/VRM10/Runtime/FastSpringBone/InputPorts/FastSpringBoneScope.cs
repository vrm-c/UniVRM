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
        private readonly FastSpringBoneService _fastSpringBoneService;
        
        public IReadOnlyList<FastSpringBoneSpring> Springs { get; }

        public FastSpringBoneScope(IReadOnlyList<FastSpringBoneSpring> springs)
        {
            Springs = springs;
            
            _fastSpringBoneService = FastSpringBoneService.Instance;
            _fastSpringBoneService.BufferCombiner.Register(this);
        }

        public void Dispose()
        {
            _fastSpringBoneService.BufferCombiner.Unregister(this);
        }
    }
}
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
        private readonly FastSpringBoneService _service;
        private readonly FastSpringBoneBuffer _buffer;
        
        public IReadOnlyList<FastSpringBoneSpring> Springs { get; }

        public FastSpringBoneScope(IReadOnlyList<FastSpringBoneSpring> springs)
        {
            Springs = springs;
            _service = FastSpringBoneService.Instance;
            _buffer = new FastSpringBoneBuffer(springs);
            
            _service.BufferCombiner.Register(_buffer);
        }

        public void Dispose()
        {
            _service.BufferCombiner.Unregister(_buffer);
            _buffer.Dispose();
        }
    }
}
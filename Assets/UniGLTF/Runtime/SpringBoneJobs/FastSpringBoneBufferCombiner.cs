using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniGLTF.SpringBoneJobs.InputPorts;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs
{
    /// <summary>
    /// CombinedBuffer 構築を管理する
    /// 
    /// - FastSpringBoneBuffer(Vrm-1.0 一体分の情報)を登録・削除する。
    /// - 再構築する。登録・削除後に反映するために呼び出す。
    /// - (TODO) FastSpringBoneBuffer一体分の回転初期化を実行する。
    /// 
    /// </summary>
    public sealed class FastSpringBoneBufferCombiner : IDisposable
    {
        private FastSpringBoneCombinedBuffer _combinedBuffer;
        public FastSpringBoneCombinedBuffer Combined => _combinedBuffer;
        private readonly LinkedList<FastSpringBoneBuffer> _buffers = new LinkedList<FastSpringBoneBuffer>();
        private bool _isDirty;
        public bool HasBuffer => _buffers.Count > 0 && _combinedBuffer != null;

        public void Register(FastSpringBoneBuffer buffer)
        {
            _buffers.AddLast(buffer);
            _isDirty = true;
        }

        public void Unregister(FastSpringBoneBuffer buffer)
        {
            _buffers.Remove(buffer);
            _isDirty = true;
        }

        /// <summary>
        /// 変更があったならばバッファを再構築する
        /// </summary>
        public JobHandle ReconstructIfDirty(JobHandle handle)
        {
            if (_isDirty)
            {
                var result = ReconstructBuffers(handle);
                _isDirty = false;
                return result;
            }

            return handle;
        }

        /// <summary>
        /// バッファを再構築する
        /// </summary>
        private JobHandle ReconstructBuffers(JobHandle handle)
        {
            Profiler.BeginSample("FastSpringBone.ReconstructBuffers");

            Profiler.BeginSample("FastSpringBone.ReconstructBuffers.DisposeBuffers");
            if (_combinedBuffer is FastSpringBoneCombinedBuffer combined)
            {
                Profiler.BeginSample("FastSpringBone.ReconstructBuffers.SaveToSourceBuffer");
                combined.SaveToSourceBuffer();
                Profiler.EndSample();

                // TODO: Dispose せずに再利用？
                combined.Dispose();
            }
            Profiler.EndSample();

            handle = FastSpringBoneCombinedBuffer.Create(handle, _buffers, out _combinedBuffer);

            Profiler.EndSample();

            return handle;
        }

        /// <summary>
        /// 各Jointのローカルローテーションを初期回転に戻す。spring reset
        /// </summary>
        public void InitializeJointsLocalRotation(FastSpringBoneBuffer model)
        {
            if (_combinedBuffer is FastSpringBoneCombinedBuffer combined)
            {
                combined.InitializeJointsLocalRotation(model);
            }
        }

        public void Dispose()
        {
            if (_combinedBuffer is FastSpringBoneCombinedBuffer combined)
            {
                combined.Dispose();
                _combinedBuffer = null;
            }
        }

        public void DrawGizmos()
        {
            if (_combinedBuffer is FastSpringBoneCombinedBuffer combined)
            {
                combined.DrawGizmos();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Profiling;
using UniGLTF.SpringBoneJobs.InputPorts;
using UnityEngine;
using Unity.Collections;
using System.Linq;

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
        private Queue<(bool isAdd, FastSpringBoneBuffer buffer)> _request = new();
        public bool HasBuffer => _buffers.Count > 0 && _combinedBuffer != null;

        public void Register(FastSpringBoneBuffer buffer)
        {
            _request.Enqueue((true, buffer));
        }

        public void Unregister(FastSpringBoneBuffer buffer)
        {
            _request.Enqueue((false, buffer));
        }

        /// <summary>
        /// 変更があったならばバッファを再構築する
        /// </summary>
        public JobHandle ReconstructIfDirty(JobHandle handle)
        {
            if (_request.Count == 0)
            {
                return handle;
            }

            if (_combinedBuffer is FastSpringBoneCombinedBuffer combined)
            {
                // index が変わる前に シミュレーションの状態を保存する。
                // 状態の保存場所が BlittableJoint から CurrentTails に移動しているのでここでやる。
                var logicsIndex = 0;
                foreach (var buffer in _buffers)
                {
                    if (_request.Any(x => !x.isAdd && x.buffer == buffer))
                    {
                        // 削除するので skip
                        continue;
                    }
                    buffer.BackupCurrentTails(combined.CurrentTails, combined.NextTails, logicsIndex);
                    logicsIndex += buffer.Logics.Length;
                }
            }

            // buffer 増減
            while (_request.Count > 0)
            {
                var (isAdd, buffer) = _request.Dequeue();
                if (isAdd)
                {
                    // 速度 0 にする
                    _buffers.AddLast(buffer);
                }
                else
                {
                    _buffers.Remove(buffer);
                }
            }

            // 再構築
            return ReconstructBuffers(handle);
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
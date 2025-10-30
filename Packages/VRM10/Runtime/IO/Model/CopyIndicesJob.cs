using Unity.Collections;
using Unity.Jobs;

#if ENABLE_VRM10_BURST
using Unity.Burst;
#endif

namespace UniVRM10
{
    /// <summary>
    /// インデックス配列を、オフセットを加えながら複製するJob郡
    /// MEMO: ushortを考慮することをやめればかなりシンプルに書ける
    /// </summary>
    internal struct CopyIndicesJobs
    {
        /// <summary>
        /// unsigned int -> unsigned int
        /// </summary>
#if ENABLE_VRM10_BURST
        [BurstCompile]
#endif
        public struct UInt2UInt : IJobParallelFor
        {
            private readonly uint _vertexOffset;

            [ReadOnly] private readonly NativeSlice<uint> _source;
            [WriteOnly] private NativeSlice<uint> _destination;

            public UInt2UInt(uint vertexOffset, NativeSlice<uint> source, NativeSlice<uint> destination)
            {
                _vertexOffset = vertexOffset;
                _source = source;
                _destination = destination;
            }

            public void Execute(int index)
            {
                _destination[index] = _source[index] + _vertexOffset;
            }
        }

        /// <summary>
        /// unsigned short -> unsigned int
        /// </summary>
#if ENABLE_VRM10_BURST
        [BurstCompile]
#endif
        public struct Ushort2Uint : IJobParallelFor
        {
            private readonly uint _vertexOffset;

            [ReadOnly] private readonly NativeSlice<ushort> _source;
            [WriteOnly] private NativeSlice<uint> _destination;

            public Ushort2Uint(uint vertexOffset, NativeSlice<ushort> source, NativeSlice<uint> destination)
            {
                _vertexOffset = vertexOffset;
                _source = source;
                _destination = destination;
            }

            public void Execute(int index)
            {
                _destination[index] = _source[index] + _vertexOffset;
            }
        }

        /// <summary>
        /// unsigned short -> unsigned short
        /// </summary>
#if ENABLE_VRM10_BURST
        [BurstCompile]
#endif
        public struct Ushort2Ushort : IJobParallelFor
        {
            private readonly ushort _vertexOffset;

            [ReadOnly] private readonly NativeSlice<ushort> _source;
            [WriteOnly] private NativeSlice<ushort> _destination;

            public Ushort2Ushort(ushort vertexOffset, NativeSlice<ushort> source, NativeSlice<ushort> destination)
            {
                _vertexOffset = vertexOffset;
                _source = source;
                _destination = destination;
            }

            public void Execute(int index)
            {
                _destination[index] = (ushort)(_source[index] + _vertexOffset);
            }
        }

        /// <summary>
        /// unsigned int -> unsigned short
        /// </summary>
#if ENABLE_VRM10_BURST
        [BurstCompile]
#endif
        public struct Uint2Ushort : IJobParallelFor
        {
            private readonly ushort _vertexOffset;

            [ReadOnly] private readonly NativeSlice<uint> _source;
            [WriteOnly] private NativeSlice<ushort> _destination;

            public Uint2Ushort(ushort vertexOffset, NativeSlice<uint> source, NativeSlice<ushort> destination)
            {
                _vertexOffset = vertexOffset;
                _source = source;
                _destination = destination;
            }

            public void Execute(int index)
            {
                _destination[index] = (ushort)(_source[index] + _vertexOffset);
            }
        }
    }
}
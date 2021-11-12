using UniGLTF;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

#if ENABLE_VRM10_BURST
using Unity.Burst;
#endif

namespace UniVRM10
{
    #if ENABLE_VRM10_BURST
    [BurstCompile]
    #endif
    internal struct InterleaveMeshVerticesJob : IJobParallelFor
    {
        [WriteOnly] private NativeArray<MeshVertex> _vertices;
        
        [ReadOnly] private readonly NativeArray<Vector3> _positions;
        
        // default値を許容する
        [ReadOnly, NativeDisableContainerSafetyRestriction] private readonly NativeArray<Vector3> _normals;
        [ReadOnly, NativeDisableContainerSafetyRestriction] private readonly NativeArray<Vector2> _texCoords;
        [ReadOnly, NativeDisableContainerSafetyRestriction] private readonly NativeArray<Color> _colors;
        [ReadOnly, NativeDisableContainerSafetyRestriction] private readonly NativeArray<Vector4> _weights;
        [ReadOnly, NativeDisableContainerSafetyRestriction] private readonly NativeArray<SkinJoints> _joints;

        public InterleaveMeshVerticesJob(
            NativeArray<MeshVertex> vertices,
            NativeArray<Vector3> positions,
            NativeArray<Vector3> normals = default,
            NativeArray<Vector2> texCoords = default,
            NativeArray<Color> colors = default,
            NativeArray<Vector4> weights = default,
            NativeArray<SkinJoints> joints = default)
        {
            _vertices = vertices;
            _positions = positions;
            _normals = normals;
            _texCoords = texCoords;
            _colors = colors;
            _weights = weights;
            _joints = joints;
        }

        public void Execute(int index)
        {
            _vertices[index] = new MeshVertex(
                _positions[index],
                _normals.IsCreated ? _normals[index] : Vector3.zero,
                _texCoords.IsCreated ? _texCoords[index] : Vector2.zero,
                _colors.IsCreated ? _colors[index] : Color.white,
                _joints.IsCreated ? _joints[index].Joint0 : (ushort)0,
                _joints.IsCreated ? _joints[index].Joint1 : (ushort)0,
                _joints.IsCreated ? _joints[index].Joint2 : (ushort)0,
                _joints.IsCreated ? _joints[index].Joint3 : (ushort)0,
                _weights.IsCreated ? _weights[index].x : 0,
                _weights.IsCreated ? _weights[index].y : 0,
                _weights.IsCreated ? _weights[index].z : 0,
                _weights.IsCreated ? _weights[index].w : 0
            );
        }
    }
}
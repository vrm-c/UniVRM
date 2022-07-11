using System;
using System.Collections.Generic;
using UniGLTF;

namespace UniVRM10
{
    public class UnNormalizedException : Exception
    {

    }

    /// <summary>
    /// x, y, z => -x, y, -z
    /// </summary>
    public static class RotateY180
    {
        public static void Rotate(glTFNode node)
        {
            if (node.matrix != null && node.matrix.Length == 16)
            {
                throw new NotImplementedException("matrix not implemented !");
            }
            else
            {
                if (node.translation != null && node.translation.Length == 3)
                {
                    // rotate 180 degrees around the Y axis
                    var t = node.translation;
                    t[0] = -t[0];
                    t[2] = -t[2];
                }
                if (node.rotation != null && node.rotation.Length == 4)
                {
                    if (node.rotation[0] == 0 && node.rotation[1] == 0 && node.rotation[2] == 0 && node.rotation[3] == 1)
                    {
                        // indentity
                    }
                    else
                    {
                        throw new UnNormalizedException();
                    }
                }
                if (node.scale != null && node.scale.Length == 3)
                {
                    // do nothing
                }
            }
        }

        static void ReverseVector3Array(GltfData data, int accessorIndex, HashSet<int> used)
        {
            if (accessorIndex == -1)
            {
                return;
            }

            if (!used.Add(accessorIndex))
            {
                return;
            }

            var accessor = data.GLTF.accessors[accessorIndex];
            var bufferViewIndex = -1;
            if (accessor.bufferView.HasValue)
            {
                bufferViewIndex = accessor.bufferView.Value;
            }
            else if (accessor.sparse?.values != null && accessor.sparse.values.bufferView != -1)
            {
                bufferViewIndex = accessor.sparse.values.bufferView;
            }

            if (bufferViewIndex != -1)
            {
                var buffer = data.GetBytesFromBufferView(bufferViewIndex);
                var span = buffer.Reinterpret<UnityEngine.Vector3>(1);
                for (int i = 0; i < span.Length; ++i)
                {
                    span[i] = span[i].RotateY180();
                }
            }
        }

        /// <summary>
        /// シーンをY軸で180度回転する
        /// </summary>
        /// <param name="gltf"></param>
        public static void Rotate(GltfData data)
        {
            foreach (var node in data.GLTF.nodes)
            {
                Rotate(node);
            }

            // mesh の回転のみでよい
            var used = new HashSet<int>();
            foreach (var mesh in data.GLTF.meshes)
            {
                foreach (var prim in mesh.primitives)
                {
                    ReverseVector3Array(data, prim.attributes.POSITION, used);
                    ReverseVector3Array(data, prim.attributes.NORMAL, used);
                    foreach (var target in prim.targets)
                    {
                        ReverseVector3Array(data, target.POSITION, used);
                        ReverseVector3Array(data, target.NORMAL, used);
                    }
                }
            }

            foreach (var skin in data.GLTF.skins)
            {
                if (used.Add(skin.inverseBindMatrices))
                {
                    var accessor = data.GLTF.accessors[skin.inverseBindMatrices];
                    var buffer = data.GetBytesFromBufferView(accessor.bufferView.Value);
                    var span = buffer.Reinterpret<UnityEngine.Matrix4x4>(1);
                    for (int i = 0; i < span.Length; ++i)
                    {
                        span[i] = span[i].RotateY180();
                    }
                }
            }
        }
    }
}

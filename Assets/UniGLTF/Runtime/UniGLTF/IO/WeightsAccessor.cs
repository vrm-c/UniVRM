using System;
using UnityEngine;

namespace UniGLTF
{
    public static class WeightsAccessor
    {
        /// <summary>
        /// WEIGHTS_0 の byte4 もしくは ushort4 もしくは float4 に対するアクセスを提供する
        /// </summary>
        public delegate (float x, float y, float z, float w) Getter(int index);

        public static (Getter, int) GetAccessor(glTF gltf, int accessorIndex)
        {
            var gltfAccessor = gltf.accessors[accessorIndex];
            switch (gltfAccessor.componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        var array = gltf.GetArrayFromAccessor<Byte4>(accessorIndex);
                        Getter getter = (i) =>
                            {
                                var value = array[i];
                                var inv = 1.0f / byte.MaxValue;
                                return (value.x*inv, value.y*inv, value.z*inv, value.w*inv);
                            };
                        return (getter, array.Length);
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        var array = gltf.GetArrayFromAccessor<UShort4>(accessorIndex);
                        Getter getter = (i) =>
                            {
                                var value = array[i];
                                var inv = 1.0f / ushort.MaxValue;
                                return (value.x*inv, value.y*inv, value.z*inv, value.w*inv);
                            };
                        return (getter, array.Length);
                    }

                case glComponentType.FLOAT:
                    {
                        var array = gltf.GetArrayFromAccessor<Vector4>(accessorIndex);
                        Getter getter = (i) =>
                            {
                                var value = array[i];
                                return (value.x, value.y, value.z, value.w);
                            };
                        return (getter, array.Length);                       
                    }
            }

            throw new NotImplementedException($"WEIGHTS_0 not support {gltfAccessor.componentType}");        }
    }
}

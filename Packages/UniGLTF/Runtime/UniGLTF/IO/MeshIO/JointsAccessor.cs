using System;

namespace UniGLTF
{
    /// <summary>
    /// JOINTS_0 の byte4 もしくは ushort4 に対するアクセスを提供する
    /// </summary>
    public static class JointsAccessor
    {
        public delegate (ushort x, ushort y, ushort z, ushort w) Getter(int index);

        public static (Getter, int) GetAccessor(GltfData data, int accessorIndex)
        {
            var gltfAccessor = data.GLTF.accessors[accessorIndex];
            switch (gltfAccessor.componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        var array = data.GetArrayFromAccessor<Byte4>(accessorIndex);
                        Getter getter = (i) =>
                            {
                                var value = array[i];
                                return (value.x, value.y, value.z, value.w);
                            };
                        return (getter, array.Length);
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        var array = data.GetArrayFromAccessor<UShort4>(accessorIndex);
                        Getter getter = (i) =>
                            {
                                var value = array[i];
                                return (value.x, value.y, value.z, value.w);
                            };
                        return (getter, array.Length);
                    }
            }

            throw new NotImplementedException($"JOINTS_0 not support {gltfAccessor.componentType}");
        }
    }
}

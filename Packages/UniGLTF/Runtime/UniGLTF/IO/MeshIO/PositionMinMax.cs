using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#accessors-bounds
    /// 
    /// Animation input and vertex position attribute accessors MUST have accessor.min and accessor.max defined. For all other accessors, these properties are optional.
    /// </summary>
    public static class AccessorsBounds
    {
        public static void UpdatePositionAccessorsBounds(glTFAccessor accessor, Vector3[] positions)
        {
            if (positions.Length == 0)
            {
                return;
            }
            var minX = positions[0].x;
            var minY = positions[0].y;
            var minZ = positions[0].z;
            var maxX = positions[0].x;
            var maxY = positions[0].y;
            var maxZ = positions[0].z;
            foreach (var position in positions)
            {
                minX = Mathf.Min(minX, position.x);
                minY = Mathf.Min(minY, position.y);
                minZ = Mathf.Min(minZ, position.z);
                maxX = Mathf.Max(maxX, position.x);
                maxY = Mathf.Max(maxY, position.y);
                maxZ = Mathf.Max(maxZ, position.z);
            }
            accessor.min = new float[] { minX, minY, minZ };
            accessor.max = new float[] { maxX, maxY, maxZ };
        }
    }
}

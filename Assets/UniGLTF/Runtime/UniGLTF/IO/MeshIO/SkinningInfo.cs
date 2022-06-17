namespace UniGLTF
{
    class SkinningInfo
    {
        public JointsAccessor.Getter Joints;
        public WeightsAccessor.Getter Weights;

        public static SkinningInfo Create(GltfData data, glTFMesh mesh, glTFPrimitives primitives)
        {
            var positions = data.GLTF.accessors[primitives.attributes.POSITION];
            return new SkinningInfo
            {
                Joints = primitives.GetJoints(data, positions.count),
                Weights = primitives.GetWeights(data, positions.count),
            };
        }

        private static (float x, float y, float z, float w) NormalizeBoneWeight(
            (float x, float y, float z, float w) src)
        {
            var sum = src.x + src.y + src.z + src.w;
            if (sum == 0)
            {
                return src;
            }

            var f = 1.0f / sum;
            src.x *= f;
            src.y *= f;
            src.z *= f;
            src.w *= f;
            return src;
        }

        public SkinnedMeshVertex GetSkinnedVertex(int i)
        {
            if (Joints == null)
            {
                return default;
            }
            var joints = Joints?.Invoke(i) ?? (0, 0, 0, 0);
            var weights = Weights != null ? NormalizeBoneWeight(Weights(i)) : (0, 0, 0, 0);
            return new SkinnedMeshVertex(
                joints.x,
                joints.y,
                joints.z,
                joints.w,
                weights.x,
                weights.y,
                weights.z,
                weights.w);
        }
    }
}

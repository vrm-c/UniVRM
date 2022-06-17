namespace UniGLTF
{
    class SkinningInfo
    {
        public JointsAccessor.Getter Joints;
        public WeightsAccessor.Getter Weights;
        public bool AssignBoneWeight { get; private set; }

        public static SkinningInfo Create(GltfData data, glTFMesh mesh, glTFPrimitives primitives)
        {
            // var hasMorphTarget = HasMorphTarget(mesh);
            var hasMorphTarget = false;

            var positions = data.GLTF.accessors[primitives.attributes.POSITION];
            var skinning = new SkinningInfo
            {
                Joints = primitives.GetJoints(data, positions.count),
                Weights = primitives.GetWeights(data, positions.count),
            };

            if (skinning.Joints != null)
            {
                // use SkinnedMeshRenderer
                return skinning;
            }
            else if (!hasMorphTarget)
            {
                // use MeshRenderer
                return skinning;
            }
            else
            {
                // use SkinnedMeshRenderer without boneWeight.
                // https://github.com/vrm-c/UniVRM/issues/1675                
                return new SkinningInfo
                {
                    AssignBoneWeight = true,
                    Joints = _ => (0, 0, 0, 0),
                    Weights = _ => (1, 0, 0, 0), // assign weight 1
                };
            }
        }

        static bool HasMorphTarget(glTFMesh mesh)
        {
            foreach (var prim in mesh.primitives)
            {
                if (prim.targets != null && prim.targets.Count > 0)
                {
                    return true;
                }
            }
            return false;
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

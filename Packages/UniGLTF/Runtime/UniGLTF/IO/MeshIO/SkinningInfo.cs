namespace UniGLTF
{
    class SkinningInfo
    {
        public JointsAccessor.Getter Joints;
        public WeightsAccessor.Getter Weights;

        /// <summary>
        /// gltfMesh に morphTarget が有る場合に、Unity では boneWeight の有無と無関係に UnityEngine.SkinnedMeshRenderer を使います。
        /// そのため `boneWeight が無い` UnityEngine.SkinnedMeshRenderer となる場合があります。
        /// boneWeight の無い SkinnedMeshRenderer の rootBone は、
        /// 
        /// * boundingBox の中心
        /// * boneWeight の無いボーンに対するスキニング
        /// 
        /// が兼用になるためメッシュが正しく描画されない場合があります。
        /// この問題の対策として、全頂点に対して boneWeight = 1, boneJoint = 0 を付与します。
        /// この boneWeight を利用するために AddComponent<SkinnedMeshRenderer> する段階で、
        /// 
        /// * mesh.bindMatrices
        /// * SkinnedMeshRenderer.bones = new Transform[]{ renderer.transform };
        /// 
        /// が必要です。この変数はその指示です。
        /// </summary>
        public bool ShouldSetRendererNodeAsBone { get; private set; }

        public static SkinningInfo Create(GltfData data, glTFMesh mesh, glTFPrimitives primitives)
        {
            var hasMorphTarget = HasMorphTarget(mesh);

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
                    ShouldSetRendererNodeAsBone = true,
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

        public MeshVertex2? GetSkinnedVertex(int i)
        {
            if (Joints == null)
            {
                return default;
            }
            var joints = Joints?.Invoke(i) ?? (0, 0, 0, 0);
            var weights = Weights != null ? NormalizeBoneWeight(Weights(i)) : (0, 0, 0, 0);
            return new MeshVertex2(
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

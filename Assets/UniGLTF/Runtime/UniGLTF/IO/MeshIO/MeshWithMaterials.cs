using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF
{
    public class MeshWithMaterials
    {
        public Mesh Mesh;
        public Material[] Materials;

        /// <summary>
        /// BoneWeight が無い && BlendShape が有るの場合に、BoneWeightを付与する。
        /// 付与した場合に true になる。
        /// </summary>
        public bool AssignBoneWeight = false;

        // 複数のノードから参照されうる
        public List<Renderer> Renderers = new List<Renderer>(); // SkinnedMeshRenderer or MeshRenderer
    }
}

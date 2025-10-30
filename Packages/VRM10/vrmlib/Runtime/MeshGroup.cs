using System.Collections.Generic;
using System.Text;

namespace VrmLib
{
    public class MeshGroup : GltfId
    {
        public readonly string Name;

        public readonly List<Mesh> Meshes = new List<Mesh>();

        public Skin Skin;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            if (Skin != null)
            {
                sb.Append("(skinned)");
            }
            var isFirst = true;
            foreach (var mesh in Meshes)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(mesh);
            }
            return sb.ToString();
        }

        public MeshGroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// VRM-0.X 様式の共有頂点バッファを持っているか？
        /// 
        /// * vrm-1.0 の Export では共有頂点バッファを使用しない
        /// * UniVRMの vrm-0.x => vrm-1.0 へのマイグレーション処理では、頂点バッファの様式変更はしない
        /// 
        /// マイグレーションしたときのみtrueになる想定
        /// </summary>
        public bool HasSharedBuffer => Meshes.Count == 1 && Meshes[0].Submeshes.Count > 1;
    }
}

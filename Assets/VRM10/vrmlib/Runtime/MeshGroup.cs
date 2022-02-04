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
    }
}

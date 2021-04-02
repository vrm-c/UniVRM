using System;
using System.Linq;

namespace VrmLib
{
    /// <summary>
    /// Modelを安全に変更できるようにラップする
    /// </summary>
    public class ModelModifier
    {
        /// <summary>
        /// 直接データを変更する場合は整合性に注意
        /// </summary>
        public Model Model;

        public ModelModifier(Model model)
        {
            Model = model;
        }

        /// <summary>
        /// Meshを置き換える。
        ///
        /// src=null, dst!=null で追加。
        /// src!=null, dst=null で削除。
        /// </summary>
        /// <param name = "src">置き換え元</param>
        /// <param name = "dst">置き換え先</param>
        public void MeshReplace(MeshGroup src, MeshGroup dst)
        {
            // replace: Meshes
            if (src != null)
            {
                Model.MeshGroups.RemoveAll(x => x == src);
            }
            if (dst != null && !Model.MeshGroups.Contains(dst))
            {
                Model.MeshGroups.Add(dst);
            }

            // replace: Node.Mesh
            foreach (var node in Model.Nodes)
            {
                if (src != null && src == node.MeshGroup)
                {
                    node.MeshGroup = dst;
                }
            }
        }

        #region Node
        public void NodeAdd(Node node, Node parent = null)
        {
            if (parent is null)
            {
                parent = Model.Root;
            }
            parent.Add(node);
            if (Model.Nodes.Contains(node))
            {
                throw new ArgumentException($"Nodes contain {node}");
            }
            Model.Nodes.Add(node);
        }

        public void NodeRemove(Node remove)
        {
            foreach (var node in Model.Nodes)
            {
                if (node.Parent == remove)
                {
                    remove.Remove(node);
                }
                if (remove.Parent == node)
                {
                    node.Remove(remove);
                }
            }
            if (Model.Root.Children.Contains(remove))
            {
                Model.Root.Remove(remove);
            }

            Model.Nodes.Remove(remove);
        }

        /// <summary>
        /// Nodeを置き換える。参照を置換する。
        /// </summary>
        public void NodeReplace(Node src, Node dst)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }
            if (dst == null)
            {
                throw new ArgumentNullException();
            }

            // add dst same parent
            src.Parent.Add(dst, ChildMatrixMode.KeepWorld);

            // remove all child
            foreach (var child in src.Children.ToArray())
            {
                dst.Add(child, ChildMatrixMode.KeepWorld);
            }

            // remove from parent
            src.Parent.Remove(src);
            Model.Nodes.Remove(src);

            // remove from skinning
            foreach (var skin in Model.Skins)
            {
                skin.Replace(src, dst);
            }

            // fix animation reference
            foreach (var animation in Model.Animations)
            {
                if (animation.NodeMap.TryGetValue(src, out NodeAnimation nodeAnimation))
                {
                    animation.NodeMap.Remove(src);
                    animation.NodeMap.Add(dst, nodeAnimation);
                }
            }

            if (Model.Nodes.Contains(dst))
            {
                throw new Exception("already exists");
            }
            Model.Nodes.Add(dst);

            // TODO: SpringBone
        }
        #endregion
    }
}
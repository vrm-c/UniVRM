using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public static class ModelExtensions
    {
        public static void ApplyRotationAndScale(this Model model)
        {
            // worldPositionを記録する
            var m_positionMap = model.Nodes.ToDictionary(x => x, x => x.Translation);

            // 回転・拡縮を除去する
            // 木構造の根元から実行する
            // Rootは編集対象外
            foreach (var node in model.Root.Traverse().Skip(1))
            {
                // 回転・拡縮を除去
                if (m_positionMap.TryGetValue(node, out Vector3 pos))
                {
                    var t = Matrix4x4.CreateTranslation(pos);
                    node.SetMatrix(t, false);
                }
            }
        }

        /// <summary>
        // [Debug向け]secondaryを除去
        /// </summary>
        public static void RemoveSecondary(this Model model)
        {
            var secondary = model.Nodes
            .FirstOrDefault(x =>
                (x.Name == "secondary" || x.Name == "SpringBone")
                && x.Parent == model.Root
                && x.Children.Count == 0)
            ;
            if (secondary != null)
            {
                var mod = new ModelModifier(model);
                mod.NodeRemove(secondary);
            }
        }

        static void CheckIndex<T>(List<T> list, string name) where T : GltfId
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].GltfIndex.HasValue)
                {
                    if (list[i].GltfIndex.Value == i)
                    {
                        Console.WriteLine($"{name}[{i}] => OK");
                    }
                    else
                    {
                        Console.WriteLine($"{name}[{i}] => {list[i].GltfIndex}");
                    }
                }
                else
                {
                    Console.WriteLine($"{name}[{i}] => null");
                }
            }
        }

        public static void CheckIndex(this Model model)
        {
            CheckIndex(model.Nodes, nameof(model.Nodes));
            CheckIndex(model.Skins, nameof(model.Skins));
            CheckIndex(model.MeshGroups, nameof(model.MeshGroups));
            CheckIndex(model.Animations, nameof(model.Animations));
        }
    }
}

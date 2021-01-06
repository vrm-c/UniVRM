using System;
using System.Linq;

namespace VrmLib
{
    public static class ModelExtensionsForValidation
    {
        public static void Validate(this Model model, Node node, string message)
        {
            if (node is null)
            {
                throw new ArgumentNullException(message);
            }
            if (!model.Nodes.Contains(node))
            {
                throw new ArgumentException($"{message}: node found in nodes");
            }
        }

        public static void Validate(this Model model)
        {
            foreach (var node in model.Root.Traverse().Skip(1))
            {
                model.Validate(node, "nodes must Contains node");
            }

            foreach (var skin in model.Skins)
            {
                foreach (var joint in skin.Joints)
                {
                    model.Validate(joint, "nodes must Contatins joint");
                }
            }

            if (model.Vrm != null)
            {
                if (model.Vrm.ExpressionManager != null)
                {
                    foreach (var b in model.Vrm.ExpressionManager.ExpressionList)
                    {
                        foreach (var v in b.MorphTargetBinds)
                        {
                            model.Validate(v.Node, "MorphTargetBindValue.Node is null");
                        }
                    }
                }

                if (model.Vrm.FirstPerson != null)
                {
                    foreach (var a in model.Vrm.FirstPerson.Annotations)
                    {
                        model.Validate(a.Node, "FirstPersonMeshAnnotation.Node is null");
                    }
                }

                var humanDict = model.Root.Traverse()
                    .Where(x => x.HumanoidBone.HasValue)
                    .ToDictionary(x => x.HumanoidBone.Value, x => x);

                foreach (var required in new[]{
                    HumanoidBones.hips,
                })
                {
                    if (!humanDict.ContainsKey(required))
                    {
                        throw new Exception($"no {required}");
                    }
                }
            }
        }
    }
}

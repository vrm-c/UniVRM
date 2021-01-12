using System;
using System.Collections.Generic;
using System.Linq;
using VrmLib;

namespace UniVRM10
{
    public static class FirstPersonAdapter
    {
        public static VrmLib.FirstPersonMeshType FromGltf(this UniGLTF.Extensions.VRMC_vrm.FirstPersonType src)
        {
            switch (src)
            {
                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.auto: return FirstPersonMeshType.Auto;
                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.both: return FirstPersonMeshType.Both;
                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.firstPersonOnly: return FirstPersonMeshType.FirstPersonOnly;
                case UniGLTF.Extensions.VRMC_vrm.FirstPersonType.thirdPersonOnly: return FirstPersonMeshType.ThirdPersonOnly;
            }

            throw new NotImplementedException();
        }

        public static FirstPerson FromGltf(this UniGLTF.Extensions.VRMC_vrm.FirstPerson fp, List<Node> nodes)
        {
            var self = new FirstPerson();

            if (fp.MeshAnnotations != null)
            {
                self.Annotations.AddRange(fp.MeshAnnotations
                    .Select(x => new FirstPersonMeshAnnotation(nodes[x.Node.Value], x.FirstPersonType.FromGltf())));
            }
            return self;
        }
        public static UniGLTF.Extensions.VRMC_vrm.FirstPerson ToGltf(this FirstPerson self, List<Node> nodes)
        {
            if (self == null)
            {
                return null;
            }

            var firstPerson = new UniGLTF.Extensions.VRMC_vrm.FirstPerson
            {

            };

            foreach (var x in self.Annotations)
            {
                firstPerson.MeshAnnotations.Add(new UniGLTF.Extensions.VRMC_vrm.MeshAnnotation
                {
                    Node = nodes.IndexOfThrow(x.Node),
                    FirstPersonType = EnumUtil.Cast<UniGLTF.Extensions.VRMC_vrm.FirstPersonType>(x.FirstPersonFlag),
                });
            }
            return firstPerson;
        }
    }
}

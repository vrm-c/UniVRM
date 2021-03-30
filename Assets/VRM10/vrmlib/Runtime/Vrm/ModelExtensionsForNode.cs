using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace VrmLib
{
    public static class ModelExtensionsForNode
    {
        class NodeUsage
        {
            public bool HasMesh;
            public int WeightUsed;
            public HumanoidBones? HumanBone;
            public bool TreeHasHumanBone;

            /// <summary>
            /// 子階層に消さずに残すBoneが含まれるか
            /// </summary>
            public bool TreeHasUsedBone;

            public bool SpringUse;

            public override string ToString()
            {
                if (HumanBone.HasValue)
                {
                    return $"{HumanBone.Value}";
                }
                else
                {
                    return $"{Used}";
                }
            }

            public bool Used
            {
                get
                {
                    if (HasMesh) return true;
                    if (WeightUsed > 0) return true;
                    if (HumanBone.HasValue && HumanBone.Value != HumanoidBones.unknown) return true;
                    if (SpringUse) return true;
                    if (TreeHasHumanBone) return true;
                    if (TreeHasUsedBone) return true;
                    return false;
                }
            }
        }
   }
}
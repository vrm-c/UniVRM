using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.NativeWrappers;

namespace VRM.FastSpringBones.Registries
{
    /// <summary>
    /// 今生きているRootBoneの一覧を返すクラス
    /// </summary>
    public sealed class RootBoneRegistry : Registry<NativePointer<BlittableRootBone>>
    {
    }
}
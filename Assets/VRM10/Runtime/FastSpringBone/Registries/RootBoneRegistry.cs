using UniVRM10.FastSpringBones.Blittables;
using UniVRM10.FastSpringBones.NativeWrappers;

namespace UniVRM10.FastSpringBones.Registries
{
    /// <summary>
    /// 今生きているRootBoneの一覧を返すクラス
    /// </summary>
    public sealed class RootBoneRegistry : Registry<NativePointer<BlittableRootBone>>
    {
    }
}
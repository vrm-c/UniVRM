namespace UniVRM10
{
    /// <summary>
    /// This is an interface to customize springboneruntime of VRM-1.0 placed in the scene.
    /// see Vrm10Instance.MakeRuntime.
    /// </summary>
    public interface IVrm10SpringBoneRuntimeProvider
    {
        IVrm10SpringBoneRuntime CreateSpringBoneRuntime();
    }
}
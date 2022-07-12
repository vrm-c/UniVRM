using System;

namespace UniGLTF
{
    /// <summary>
    /// https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#meshes
    /// 
    /// See Implementation Note about `mesh.extras.targetNames`
    /// </summary>
    [Flags]
    public enum BlendShapeTargetNameLocationFlags
    {
        None = 0,
        Mesh = 1,
        Primitives = 2,

        Both = Mesh | Primitives,
    }
}

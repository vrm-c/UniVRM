using System;
using System.Linq;
using System.Reflection;
using UniJSON;


namespace UniGLTF
{
    #region Base
    public class PartialExtensionBase<T>
    {
    }

    [ItemJsonSchema(ValueType = ValueNodeType.Object)]
    //[JsonSchema(MinProperties = 1)]
    public partial class ExtensionsBase<T> : PartialExtensionBase<T>
    {
    }

    //[JsonSchema(MinProperties = 1)]
    public partial class ExtraBase<T> : PartialExtensionBase<T>
    {
    }
    #endregion

    [Serializable]
    public partial class glTF_extensions : ExtensionsBase<glTF_extensions> { }

    [Serializable]
    public partial class gltf_extras : ExtraBase<gltf_extras> { }
}

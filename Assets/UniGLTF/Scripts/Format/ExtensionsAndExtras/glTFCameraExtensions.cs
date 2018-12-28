using System;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    [ItemJsonSchema(ValueType = ValueNodeType.Object)]
    public partial class glTFOrthographic_extensions : ExtensionsBase<glTFOrthographic_extensions> { }

    [Serializable]
    public partial class glTFOrthographic_extras : ExtraBase<glTFOrthographic_extras> { }

    [Serializable]
    [ItemJsonSchema(ValueType = ValueNodeType.Object)]
    public partial class glTFPerspective_extensions : ExtensionsBase<glTFPerspective_extensions> { }

    [Serializable]
    public partial class glTFPerspective_extras : ExtraBase<glTFPerspective_extras> { }

    [Serializable]
    [ItemJsonSchema(ValueType = ValueNodeType.Object)]
    public partial class glTFCamera_extensions : ExtensionsBase<glTFCamera_extensions> { }

    [Serializable]
    public partial class glTFCamera_extras : ExtraBase<glTFCamera_extras> { }
}

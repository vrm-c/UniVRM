using System;
using System.Collections.Generic;
using UniJSON;


namespace UniGLTF
{
    /// <summary>
    /// https://github.com/KhronosGroup/glTF/issues/1036
    /// </summary>
    [Serializable]
    public partial class glTFMesh_extras : ExtraBase<glTFMesh_extras>
    {
        [JsonSchema(Required = true, MinItems = 1)]
        public List<string> targetNames = new List<string>();
    }
}

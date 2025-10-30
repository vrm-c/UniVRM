using System;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFSkin
    {
        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int inverseBindMatrices = -1;

        [JsonSchema(Required = true, MinItems = 1)]
        [ItemJsonSchema(Minimum = 0)]
        public int[] joints;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int skeleton = -1;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
        public string name;
    }
}

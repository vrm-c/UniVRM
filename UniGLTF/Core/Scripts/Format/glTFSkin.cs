using System;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFSkin : JsonSerializableBase
    {
        [JsonSchema(Minimum = 0)]
        public int inverseBindMatrices = -1;

        [JsonSchema(Required = true, MinItems = 1)]
        [ItemJsonSchema(Minimum = 0)]
        public int[] joints;

        [JsonSchema(Minimum = 0)]
        public int skeleton = -1;

        // empty schemas
        public object extensions;
        public object extras;
        public string name;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => inverseBindMatrices);
            f.KeyValue(() => joints);
            if (skeleton >= 0)
            {
                f.KeyValue(() => skeleton);
            }
        }
    }
}

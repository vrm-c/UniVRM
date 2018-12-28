using System;
using System.Collections.Generic;
using UniJSON;


namespace UniGLTF
{
    /// <summary>
    /// https://github.com/KhronosGroup/glTF/issues/1036
    /// </summary>
    [Serializable]
    public partial class glTFPrimitives_extras : ExtraBase<glTFPrimitives_extras>
    {
        [JsonSchema(Required = true, MinItems = 1)]
        public List<string> targetNames = new List<string>();

        [JsonSerializeMembers]
        void PrimitiveMembers(GLTFJsonFormatter f)
        {
            if (targetNames.Count > 0)
            {
                f.Key("targetNames");
                f.BeginList();
                foreach (var x in targetNames)
                {
                    f.Value(x);
                }
                f.EndList();
            }
        }
    }
}

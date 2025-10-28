using System;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFAttributes
    {
        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int POSITION = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int NORMAL = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int TANGENT = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int TEXCOORD_0 = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int TEXCOORD_1 = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int COLOR_0 = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int JOINTS_0 = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int WEIGHTS_0 = -1;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as glTFAttributes;
            if (rhs == null)
            {
                return base.Equals(obj);
            }

            return POSITION == rhs.POSITION
                && NORMAL == rhs.NORMAL
                && TANGENT == rhs.TANGENT
                && TEXCOORD_0 == rhs.TEXCOORD_0
                && TEXCOORD_1 == rhs.TEXCOORD_1
                && COLOR_0 == rhs.COLOR_0
                && JOINTS_0 == rhs.JOINTS_0
                && WEIGHTS_0 == rhs.WEIGHTS_0
                ;
        }
    }

    [Serializable]
    public class gltfMorphTarget
    {
        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int POSITION = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int NORMAL = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int TANGENT = -1;
    }

    /// <summary>
    /// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/mesh.primitive.schema.json
    /// </summary>
    [Serializable]
    public class glTFPrimitives
    {
        public enum Mode
        {
            POINTS = 0,
            LINES = 1,
            LINE_LOOP = 2,
            LINE_STRIP = 3,
            TRIANGLES = 4,
            TRIANGLE_STRIP = 5,
            TRIANGLE_FAN = 6,
        }

        [JsonSchema(EnumValues = new object[] { 0, 1, 2, 3, 4, 5, 6 })]
        public int mode;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int indices = -1;

        [JsonSchema(Required = true, SkipSchemaComparison = true)]
        public glTFAttributes attributes;

        [JsonSchema(Minimum = 0)]
        public int? material;

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        [ItemJsonSchema(SkipSchemaComparison = true)]
        public List<gltfMorphTarget> targets = new List<gltfMorphTarget>();

        public glTFExtension extras;

        [JsonSchema(SkipSchemaComparison = true)]
        public glTFExtension extensions;
    }

    [Serializable]
    public class glTFMesh
    {
        public string name;

        [JsonSchema(Required = true, MinItems = 1)]
        public List<glTFPrimitives> primitives = new List<glTFPrimitives>();

        [JsonSchema(MinItems = 1)]
        public float[] weights;

        [JsonSchema(SkipSchemaComparison = true)]
        public glTFExtension extras;

        // empty schemas
        public glTFExtension extensions;

        public glTFMesh()
        {
        }

        public glTFMesh(string _name)
        {
            name = _name;
        }
    }
}

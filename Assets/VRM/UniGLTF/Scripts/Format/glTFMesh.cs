using System;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTFAttributes : JsonSerializableBase
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
                && COLOR_0 == rhs.COLOR_0
                && JOINTS_0 == rhs.JOINTS_0
                && WEIGHTS_0 == rhs.WEIGHTS_0
                ;
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => POSITION);
            if (NORMAL != -1) f.KeyValue(() => NORMAL);
            if (TANGENT != -1) f.KeyValue(() => TANGENT);
            if (TEXCOORD_0 != -1) f.KeyValue(() => TEXCOORD_0);
            if (COLOR_0 != -1) f.KeyValue(() => COLOR_0);
            if (JOINTS_0 != -1) f.KeyValue(() => JOINTS_0);
            if (WEIGHTS_0 != -1) f.KeyValue(() => WEIGHTS_0);
        }
    }

    [Serializable]
    public class gltfMorphTarget : JsonSerializableBase
    {
        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int POSITION = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int NORMAL = -1;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int TANGENT = -1;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => POSITION);
            if (NORMAL >= 0) f.KeyValue(() => NORMAL);
            if (TANGENT >= 0) f.KeyValue(() => TANGENT);
        }
    }

    /// <summary>
    /// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/mesh.primitive.schema.json
    /// </summary>
    [Serializable]
    public class glTFPrimitives : JsonSerializableBase
    {
        [JsonSchema(EnumValues = new object[] { 0, 1, 2, 3, 4, 5, 6 })]
        public int mode;

        [JsonSchema(Minimum = 0, ExplicitIgnorableValue = -1)]
        public int indices = -1;

        [JsonSchema(Required = true, SkipSchemaComparison = true)]
        public glTFAttributes attributes;

        public bool HasVertexColor
        {
            get
            {
                return attributes.COLOR_0 != -1;
            }
        }

        [JsonSchema(Minimum = 0)]
        public int material;

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        [ItemJsonSchema(SkipSchemaComparison = true)]
        public List<gltfMorphTarget> targets = new List<gltfMorphTarget>();

        public glTFPrimitives_extras extras = new glTFPrimitives_extras();

        [JsonSchema(SkipSchemaComparison = true)]
        public glTFPrimitives_extensions extensions = null;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => mode);
            f.KeyValue(() => indices);
            f.Key("attributes"); f.GLTFValue(attributes);
            f.KeyValue(() => material);
            if (targets != null && targets.Count > 0)
            {
                f.Key("targets"); f.GLTFValue(targets);
            }
            if (extras.targetNames.Count > 0)
            {
                f.Key("extras"); f.GLTFValue(extras);
            }
        }
    }

    [Serializable]
    public class glTFMesh : JsonSerializableBase
    {
        public string name;

        [JsonSchema(Required = true, MinItems = 1)]
        public List<glTFPrimitives> primitives = new List<glTFPrimitives>();

        [JsonSchema(MinItems = 1)]
        public float[] weights;

        [JsonSchema(SkipSchemaComparison = true)]
        public glTFMesh_extras extras = null;

        // empty schemas
        public object extensions;

        public glTFMesh()
        {
        }

        public glTFMesh(string _name)
        {
            name = _name;
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => name);
            f.Key("primitives"); f.GLTFValue(primitives);
            if (weights != null && weights.Length > 0)
            {
                f.KeyValue(() => weights);
            }
        }
    }
}

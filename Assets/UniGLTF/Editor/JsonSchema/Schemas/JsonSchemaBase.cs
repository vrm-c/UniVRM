using System;
using System.IO;
using System.Text;

namespace UniGLTF.JsonSchema.Schemas
{
    public abstract class JsonSchemaBase
    {
        public readonly string JsonPath;

        public string ClassName => JsonPath
                .Replace(".", "__")
                .Replace("[]", "_ITEM")
                .Replace("{}", "_PROP")
                ;

        public readonly JsonSchemaType JsonSchemaType;
        public string Title;
        public readonly string Description;

        /// HardCoding
        public string HardCode;

        public JsonSchemaBase(in JsonSchemaSource source)
        {
            JsonPath = source.JsonPath;
            JsonSchemaType = source.type;
            Title = source.title;
            Description = source.description;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            sb.Append(JsonSchemaType);
            sb.Append("]");
            if (!string.IsNullOrEmpty(Title))
            {
                sb.Append($" \"{Title}\"");
            }
            return sb.ToString();
        }

        public bool IsArrayItem;

        /// <summary>
        /// CSharpの型
        /// </summary>
        /// <value></value>
        public abstract string ValueType { get; }

        /// <summary>
        /// Use or not GenerateDeserializer and GenerateSerializer
        /// </summary>
        public abstract bool IsInline { get; }

        /// <summary>
        /// Deserializer の呼び出し
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public abstract string GenerateDeserializerCall(string callName, string argName);

        /// <summary>
        /// Deserializer の実装
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="callName"></param>
        public virtual void GenerateDeserializer(TraverseContext writer, string callName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serialize 時に出力するか否か
        /// 
        /// * null や -1 などの無効な値のキーをスキップするために使う
        /// 
        /// </summary>
        /// <param name="argName"></param>
        /// <returns></returns>
        public abstract string CreateSerializationCondition(string argName);

        /// <summary>
        /// Serializer の呼び出し
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public abstract string GenerateSerializerCall(string callName, string argName);

        /// <summary>
        /// Serializer 実装
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="callName"></param>
        public virtual void GenerateSerializer(TraverseContext writer, string callName)
        {
            throw new NotImplementedException();
        }
    }
}

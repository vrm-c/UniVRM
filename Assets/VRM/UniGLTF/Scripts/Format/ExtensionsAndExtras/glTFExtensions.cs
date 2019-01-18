using System;
using System.Linq;
using System.Reflection;
using UniJSON;


namespace UniGLTF
{
    #region Base
    public class JsonSerializeMembersAttribute : Attribute { }

    public class PartialExtensionBase<T> : JsonSerializableBase
    {
        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            foreach (var method in this.GetType().GetMethods(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.GetCustomAttributes(typeof(JsonSerializeMembersAttribute), true).Any())
                {
                    method.Invoke(this, new[] { f });
                }
            }
        }

        public int __count
        {
            get
            {
                return typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttributes(typeof(JsonSerializeMembersAttribute), true).Any())
                    .Count();
            }
        }
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

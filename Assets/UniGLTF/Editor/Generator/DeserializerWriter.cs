using System.IO;
using UniGLTF.JsonSchema;
using UniGLTF.JsonSchema.Schemas;

namespace GenerateUniGLTFSerialization
{
    public static class DeserializerWriter
    {
        const string Begin = @"// This file is generated from JsonSchema. Don't modify this source code.
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.Extensions.$0 {

public static class GltfDeserializer
{
    public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From($0.ExtensionName);

public static bool TryGet(UniGLTF.glTFExtension src, out $0 extension)
{
    if(src is UniGLTF.glTFExtensionImport extensions)
    {
        foreach(var kv in extensions.ObjectItems())
        {
            if(kv.Key.GetUtf8String() == ExtensionNameUtf8)
            {
                extension = Deserialize(kv.Value);
                return true;
            }
        }
    }

    extension = default;
    return false;
}

";

        const string End = @"
} // GltfDeserializer
} // UniGLTF 
";

        public static void Write(TextWriter w, JsonSchemaSource root, string rootName)
        {
            w.Write(Begin.Replace("$0", rootName));
            root.Create(true, rootName).GenerateDeserializer(new TraverseContext(w), "Deserialize");
            w.Write(End);
        }
    }
}

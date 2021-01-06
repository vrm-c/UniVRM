using System.IO;
using UniGLTF.JsonSchema;
using UniGLTF.JsonSchema.Schemas;

namespace GenerateUniGLTFSerialization
{
    public static class SerializerWriter
    {
        const string Begin = @"// This file is generated from JsonSchema. Don't modify this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;

namespace UniGLTF.Extensions.$0 {

    static public class GltfSerializer
    {

        public static void SerializeTo(ref UniGLTF.glTFExtension dst, $0 extension)
        {
            if (dst is glTFExtensionImport)
            {
                throw new NotImplementedException();
            }

            if (!(dst is glTFExtensionExport extensions))
            {
                extensions = new glTFExtensionExport();
                dst = extensions;
            }

            var f = new JsonFormatter();
            Serialize(f, extension);
            extensions.Add($0.ExtensionName, f.GetStoreBytes());
        }

";

        const string End = @"
    } // class
} // namespace
";

        public static void Write(TextWriter w, JsonSchemaSource root, string rootName)
        {
            w.Write(Begin.Replace("$0", rootName));
            root.Create(true, rootName).GenerateSerializer(new TraverseContext(w), "Serialize");
            w.Write(End);
        }
    }
}

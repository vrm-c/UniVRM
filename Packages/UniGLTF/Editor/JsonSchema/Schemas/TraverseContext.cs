using System.IO;
using System.Collections.Generic;

namespace UniGLTF.JsonSchema.Schemas
{
    public class TraverseContext
    {
        public readonly TextWriter Writer;

        public readonly HashSet<string> Used = new HashSet<string>();

        public TraverseContext(TextWriter writer)
        {
            Writer = writer;
        }

        public void Write(string s)
        {
            Writer.Write(s);
        }
    }
}

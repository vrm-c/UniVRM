using System;
using System.IO;
using System.Linq;
using UniGLTF.JsonSchema;

namespace GenerateUniGLTFSerialization
{
    public class Generator
    {
        static void ClearFolder(DirectoryInfo dir)
        {
            Console.WriteLine($"clear: {dir}");

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo child in dir.GetDirectories())
            {
                child.Delete(true);
            }
        }

        static string CleanupTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return title;
            }
            var splitted = title.Split().ToList();
            if (splitted.Last() == "extension")
            {
                splitted.RemoveAt(splitted.Count - 1);
            }
            return string.Join("", splitted
                .Where(x => x.Length > 0)
                .Select(x => x.Substring(0, 1).ToUpper() + x.Substring(1)));
        }

        static string GetStem(string filename)
        {
            return filename.Split('.').First();
        }

        public static void GenerateTo(JsonSchemaSource root, DirectoryInfo dir, bool clearFolder)
        {
            // clear or create folder
            if (dir.Exists)
            {
                if (dir.EnumerateFileSystemInfos().Any())
                {
                    if (!clearFolder)
                    {
                        Console.WriteLine($"{dir} is not empty.");
                        return;
                    }

                    // clear
                    ClearFolder(dir);
                }
            }
            else
            {
                Console.WriteLine($"create: {dir}");
                dir.Create();
            }

            foreach (var s in root.Traverse())
            {
                // title を掃除
                s.title = CleanupTitle(s.title);
            }

            {
                var dst = Path.Combine(dir.FullName, "Format.g.cs");
                Console.WriteLine(dst);
                using (var w = new StringWriter())
                {
                    FormatWriter.Write(w, root, GetStem(root.FilePath.Name));
                    File.WriteAllText(dst, w.ToString().Replace("\r\n", "\n"));
                }
            }
            {
                var dst = Path.Combine(dir.FullName, "Deserializer.g.cs");
                Console.WriteLine(dst);
                using (var w = new StringWriter())
                {
                    DeserializerWriter.Write(w, root, GetStem(root.FilePath.Name));
                    File.WriteAllText(dst, w.ToString().Replace("\r\n", "\n"));
                }
            }
            {
                var dst = Path.Combine(dir.FullName, "Serializer.g.cs");
                Console.WriteLine(dst);
                using (var w = new StringWriter())
                {
                    SerializerWriter.Write(w, root, GetStem(root.FilePath.Name));
                    File.WriteAllText(dst, w.ToString().Replace("\r\n", "\n"));
                }
            }
        }
    }
}

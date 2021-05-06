using System;
using System.IO;
using System.Linq;
using System.Text;
using UniGLTF.JsonSchema;

namespace GenerateUniGLTFSerialization
{
    public class Generator
    {
        static void DeleteAllInDirectory(DirectoryInfo dir)
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

        static void CleanDirectory(DirectoryInfo dir)
        {
            // clear or create folder
            if (dir.Exists)
            {
                if (dir.EnumerateFileSystemInfos().Any())
                {
                    DeleteAllInDirectory(dir);
                }
            }
            else
            {
                Console.WriteLine($"create: {dir}");
                dir.Create();
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

        static void WriteAllTextForce(string path, string contents)
        {
            if (string.IsNullOrEmpty(path)) return;
            
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir)) return;
            
            var dirInfo = new DirectoryInfo(dir);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            File.WriteAllText(path, contents, Encoding.UTF8);
        }
        
        public static void GenerateTo(JsonSchemaSource root, DirectoryInfo formatDir, DirectoryInfo serializerDir)
        {
            CleanDirectory(formatDir);
            CleanDirectory(serializerDir);
            
            foreach (var s in root.Traverse())
            {
                // title を掃除
                s.title = CleanupTitle(s.title);
            }

            {
                var dst = Path.Combine(formatDir.FullName, "Format.g.cs");
                Console.WriteLine(dst);
                using (var w = new StringWriter())
                {
                    FormatWriter.Write(w, root, GetStem(root.FilePath.Name));
                    WriteAllTextForce(dst, w.ToString().Replace("\r\n", "\n"));
                }
            }
            {
                var dst = Path.Combine(serializerDir.FullName, "Deserializer.g.cs");
                Console.WriteLine(dst);
                using (var w = new StringWriter())
                {
                    DeserializerWriter.Write(w, root, GetStem(root.FilePath.Name));
                    WriteAllTextForce(dst, w.ToString().Replace("\r\n", "\n"));
                }
            }
            {
                var dst = Path.Combine(serializerDir.FullName, "Serializer.g.cs");
                Console.WriteLine(dst);
                using (var w = new StringWriter())
                {
                    SerializerWriter.Write(w, root, GetStem(root.FilePath.Name));
                    WriteAllTextForce(dst, w.ToString().Replace("\r\n", "\n"));
                }
            }
        }
    }
}

using System.IO;
using System.Text;


namespace UniJSONPRofiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText(args[0], Encoding.UTF8);
            var parsed = UniJSON.JsonParser.Parse(json);
        }
    }
}

using System.IO;
using System.Text;

#if UNIJSON_PROFILING
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
#endif
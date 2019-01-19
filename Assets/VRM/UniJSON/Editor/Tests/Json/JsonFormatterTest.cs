using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Text;

namespace UniJSON
{
    public class JsonFormatterTests
    {
        [Test]
        public void IndentTest()
        {
            var formatter = new JsonFormatter(2);
            formatter.BeginMap();
            formatter.Key("a"); formatter.Value(1);
            formatter.EndMap();

            //var json = formatter.ToString();
        }

        [Test]
        public void NullTest()
        {
            var bytes = Encoding.UTF8.GetBytes("null");
            var json = new JsonFormatter();
            json.Null();
            Assert.True(json.GetStoreBytes().ToEnumerable().SequenceEqual(bytes));
        }

        [Test]
        public void BooleanTest()
        {
            {
                var bytes = Encoding.UTF8.GetBytes("true");
                var json = new JsonFormatter();
                json.Value(true);
                Assert.True(json.GetStoreBytes().ToEnumerable().SequenceEqual(bytes));
            }
            {
                var bytes = Encoding.UTF8.GetBytes("false");
                var json = new JsonFormatter();
                json.Value(false);
                Assert.True(json.GetStoreBytes().ToEnumerable().SequenceEqual(bytes));
            }
        }

        [Test]
        public void ReUseFormatter()
        {
            IFormatter f = new JsonFormatter();
            f.Value(1);

            f.Clear();
            // fail
            f.Value(2);
        }
    }
}

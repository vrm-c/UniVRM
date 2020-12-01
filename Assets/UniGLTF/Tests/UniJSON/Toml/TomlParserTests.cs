using NUnit.Framework;


/*
namespace UniJSON.Toml
{
    class TomlParserTests
    {
        [Test]
        public void BareKeyTests()
        {
            {
                var result = TomlParser.Parse(@"
value = 1
");
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["value"].GetInt32());
            }
        }

        [Test]
        public void DottedKeyTests()
        {
            {
                var result = TomlParser.Parse(@"
value.value2 = 1
");
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["value"]["value2"].GetInt32());
            }
        }

        [Test]
        public void DuplicatedKey()
        {
            {
                Assert.Catch(() => TomlParser.Parse(@"
value = 1
value = 2
"));
            }
        }

        [Test]
        public void QuotedKeyTests()
        {
            {
                var result = TomlParser.Parse(@"
""value"" = 1
");
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["value"].GetInt32());
            }

            {
                var result = TomlParser.Parse(@"
""[key=value]"" = 1
");
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["value"].GetInt32());
            }
        }

        [Test]
        public void TableTests()
        { 
            {
                var result = @"
[table]
value = 1
".ParseAsToml();
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["table"]["value"].GetInt32());
            }

            {
                var result = @"
[table.table2]
value = 1
".ParseAsToml();
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["table"]["table2"]["value"].GetInt32());
            }
        }

        [Test]
        public void TomlExample()
        {
            var result = @"
# This is a TOML document.

title = ""TOML Example""

[owner]
name = ""Tom Preston-Werner""
dob = 1979 - 05 - 27T07: 32:00 - 08:00 # First class dates

[database]
server = ""192.168.1.1""
ports = [8001, 8001, 8002]
connection_max = 5000
enabled = true

[servers]

# Indentation (tabs and/or spaces) is allowed but not required
  [servers.alpha]
  ip = ""10.0.0.1""
  dc = ""eqdc10""

  [servers.beta]
  ip = ""10.0.0.2""
  dc = ""eqdc10""

[clients]
data = [ [""gamma"", ""delta""], [1, 2] ]

# Line breaks are OK when inside arrays
hosts = [
  ""alpha"",
  ""omega""
]
".ParseAsToml();
            Assert.AreEqual("TOML Example", result["title"].GetString());
        }
    }
}
*/

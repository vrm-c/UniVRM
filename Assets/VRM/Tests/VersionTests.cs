using NUnit.Framework;

namespace VRM
{
	public class UniVRMVersionTests
	{
		[Test]
		[TestCase(UniGLTF.PackageVersion.VERSION, false)]
		[TestCase("0.199", true)]
		[TestCase("0.199.0", true)]
		[TestCase("1.0.0", true)]
		public void IsNewerTest(string newer, bool isNewer)
		{
			Assert.AreEqual(isNewer, UniGLTF.PackageVersion.IsNewer(newer));
		}

		[Test]
		[TestCase("0.50", "0.50", false)]
		[TestCase("0.50", "0.51.0", false)]
		[TestCase("0.51.0", "0.50", true)]
		[TestCase("0.51.0", "0.51.0", false)]
		[TestCase("0.51.1", "0.51.0", true)]
		[TestCase("0.51.0", "0.51.0-a", false)]
		[TestCase("0.51.0-b", "0.51.0-a", true)]
		[TestCase("1.0.0-a", "0.51.0", true)]
		[TestCase("1.0.0", "0.51.0", true)]
		public void IsNewerTest(string newer, string older, bool isNewer)
		{
			Assert.AreEqual(isNewer, UniGLTF.PackageVersion.IsNewer(newer, older));
		}

		[Test]
		[TestCase("0.50", true, 0, 50, 0, "")]
		[TestCase("0.51.0", true, 0, 51, 0, "")]
		[TestCase("0.51.1", true, 0, 51, 1, "")]
		[TestCase("0.51.2-a", true, 0, 51, 2, "a")]
		[TestCase("0.51.10-a1", true, 0, 51, 10, "a1")]
		[TestCase("aaaaa", false, 0, 0, 0, "")]
		public void ParseVersionTest(string version, bool canBeParsed, int major, int minor, int patch, string pre)
		{
			UniGLTF.PackageVersion.Version v;
			var res = UniGLTF.PackageVersion.ParseVersion(version, out v);
			Assert.AreEqual(canBeParsed, res);
			if (res)
			{
				Assert.AreEqual(major, v.Major);
				Assert.AreEqual(minor, v.Minor);
				Assert.AreEqual(patch, v.Patch);
				Assert.AreEqual(pre, v.Pre);
			}
		}
	}
}

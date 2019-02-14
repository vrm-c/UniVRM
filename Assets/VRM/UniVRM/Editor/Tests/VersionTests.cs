using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;
using UnityEngine;

namespace VRM
{
	public class UniVRMVersionTests
	{
		[Test]
		[TestCase(VRMVersion.VERSION, false)]
		[TestCase("0.99", true)]
		[TestCase("0.99.0", true)]
		[TestCase("1.0.0", true)]
		public void IsNewweTest(string newer, bool isNewer)
		{
			Assert.AreEqual(isNewer, VRMVersion.IsNewer(newer));
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
		public void IsNewweTest(string newer, string older, bool isNewer)
		{
			Assert.AreEqual(isNewer, VRMVersion.IsNewer(newer, older));
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
			VRMVersion.Version v;
			var res = VRMVersion.ParseVersion(version, out v);
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

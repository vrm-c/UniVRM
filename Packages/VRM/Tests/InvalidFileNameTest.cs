using NUnit.Framework;
using System.Linq;
using UniGLTF;

namespace VRM
{
    public class InvalidFileNameTest
    {
        [Test]
        [TestCase("VRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMVRMV", true)]
        [TestCase("VRMFormatVRMFormatVRMFormatVRMFormatVRMFormatVRMFormatVRMFormat", false)]
        [TestCase("UniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRM", true)]
        [TestCase("UniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniVRMUniV", false)]
        [TestCase("AliciaAliciaAliciaAliciaAliciaAliciaAliciaAliciaAliciaAliciaAliciaAlicia", true)]
        public void DetectFileNameLength(string fileName, bool isIllegal)
        {
            var result = NameValidator.IsFileNameLengthTooLong(fileName);
            Assert.AreEqual(result, isIllegal);
        }

        [Test]
        [TestCase("\u0000\u0042\u0062", true)]
        [TestCase("\u0045\u0046\u0047\u0065\u0068\u0036", false)]
        [TestCase("\u0043\u0045\u0047\u007F", true)]
        [TestCase("\u0000\u0042\u0062", true)]
        [TestCase("\u003A\u0039\u005C\u0060\u0074", false)]
        [TestCase("\u005D\u006F\u001C\u007A\u0036\u0049", true)]
        public void DetectControlCharacters(string fileName, bool isIllegal)
        {
            var result = fileName.Any(x => char.IsControl(x));
            Assert.AreEqual(result, isIllegal);
        }
    }
}

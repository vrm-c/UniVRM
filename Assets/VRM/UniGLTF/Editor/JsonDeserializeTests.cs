using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class JsonDeserializeTests
    {
        static T deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        [Test]
        public void PrimitivesExtensionsTest()
        {
            {
                var r = deserialize<glTFPrimitives_extensions>("");
                Assert.AreEqual(null, r);
            }

            {
                var r = deserialize<glTFPrimitives_extensions>("{}");
                Assert.NotNull(r);
                // This is a curious behaviour of JsonUtility.
                // TODO: We should replace a library which treats JSON from JsonUtility
                //Assert.Null(r.KHR_draco_mesh_compression);
            }

            {
                var r = deserialize<glTFPrimitives_extensions>("{\"KHR_draco_mesh_compression\":{}}");
                Assert.NotNull(r);
                //Assert.NotNull(r.KHR_draco_mesh_compression);
            }
        }
    }
}

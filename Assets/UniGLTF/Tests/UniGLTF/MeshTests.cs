using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UniGLTF
{
    public class MeshTests
    {
        [Test]
        public void AccessorTest()
        {
            byte[] bytes = default;
            using(var ms = new MemoryStream())
            using(var w = new BinaryWriter(ms))
            {
                w.Write(1.0f);
                w.Write(2.0f);
                w.Write(3.0f);
                w.Write(4.0f);
                w.Write(5.0f);
                w.Write(6.0f);
                w.Write(7.0f);
                w.Write(8.0f);
                bytes = ms.ToArray();
            }
            var storage = new SimpleStorage(new ArraySegment<byte>(bytes));

            var gltf = new glTF
            {
                buffers=new List<glTFBuffer>
                {
                    new glTFBuffer
                    {
                    }
                },
                bufferViews = new List<glTFBufferView>
                {
                    new glTFBufferView{
                        buffer=0,
                        byteLength=32,
                        byteOffset=0,                        
                    }
                },
                accessors = new List<glTFAccessor>
                {
                    new glTFAccessor{
                        bufferView = 0,
                        componentType=glComponentType.FLOAT,
                        count=2,
                        byteOffset=0,
                        type="VEC4",
                    }
                }
            };
            gltf.buffers[0].OpenStorage(storage);

            var (getter, len) = WeightsAccessor.GetAccessor(gltf, 0);
            Assert.AreEqual((1.0f, 2.0f, 3.0f, 4.0f), getter(0));
            Assert.AreEqual((5.0f, 6.0f, 7.0f, 8.0f), getter(1));
        }
    }
}

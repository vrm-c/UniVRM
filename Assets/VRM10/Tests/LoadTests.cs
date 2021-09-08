using NUnit.Framework;

namespace UniVRM10.Test
{
    public class LoadTests
    {
        [Test]
        public void EmptyThumbnailName()
        {
            Assert.True(Vrm10Data.TryParseOrMigrate(TestAsset.AliciaPath, true, out Vrm10Data vrm));

            var index = vrm.VrmExtension.Meta.ThumbnailImage.Value;

            // empty thumbnail name
            vrm.Data.GLTF.images[index].name = null;

            using (var loader = new Vrm10Importer(vrm))
            {
                loader.LoadAsync(new VRMShaders.ImmediateCaller()).Wait();
            }
        }
    }
}

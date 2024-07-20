using NUnit.Framework;
using UniGLTF;

namespace UniVRM10.Test
{
    public class LoadTests
    {
        [Test]
        public void EmptyThumbnailName()
        {
            using (var data = new GlbFileParser(TestAsset.AliciaPath).Parse())
            using (var migrated = Vrm10Data.Migrate(data, out var vrm1Data, out var migration))
            {
                // Vrm10Data.ParseOrMigrate(TestAsset.AliciaPath, true, out Vrm10Data vrm, out MigrationData migration))
                Assert.NotNull(vrm1Data);

                var index = vrm1Data.VrmExtension.Meta.ThumbnailImage.Value;

                // empty thumbnail name
                vrm1Data.Data.GLTF.images[index].name = null;

                using (var loader = new Vrm10Importer(vrm1Data))
                {
                    loader.LoadAsync(new ImmediateCaller()).Wait();
                }
            }
        }
    }
}

using NUnit.Framework;

namespace VRM10.MToon10.Tests
{
    public sealed class MigrationTests
    {
        [Test]
        public void MigrateToonyAndShift()
        {
            var delta = 0.001f;

            // 0.x default
            Assert.AreEqual(0.95f,MToon10Migrator.MigrateToShadingToony(0.9f, 0f), delta);
            Assert.AreEqual(-0.05f,MToon10Migrator.MigrateToShadingShift(0.9f, 0f), delta);

            // lambert
            Assert.AreEqual(0.5f, MToon10Migrator.MigrateToShadingToony(0, 0), delta);
            Assert.AreEqual(-0.5f, MToon10Migrator.MigrateToShadingShift(0, 0), delta);

            // half lambert
            Assert.AreEqual(0.0f, MToon10Migrator.MigrateToShadingToony(0, -1), delta);
            Assert.AreEqual(0.0f, MToon10Migrator.MigrateToShadingShift(0, -1), delta);

            // random
            Assert.AreEqual(0.79f, MToon10Migrator.MigrateToShadingToony(0.7f, -0.4f), delta);
            Assert.AreEqual(0.19f, MToon10Migrator.MigrateToShadingShift(0.7f, -0.4f), delta);
        }

        [Test]
        public void MigrateGiIntensity()
        {
            // normal
            Assert.AreEqual(0f, MToon10Migrator.MigrateToGiEqualization(1f));

            // equalized
            Assert.AreEqual(1f, MToon10Migrator.MigrateToGiEqualization(0f));

            // intermediate
            Assert.AreEqual(0.25f, MToon10Migrator.MigrateToGiEqualization(0.75f));
            Assert.AreEqual(0.333f, MToon10Migrator.MigrateToGiEqualization(0.667f));
            Assert.AreEqual(0.125f, MToon10Migrator.MigrateToGiEqualization(0.875f));
            Assert.AreEqual(0.75f, MToon10Migrator.MigrateToGiEqualization(0.25f));
        }
    }
}
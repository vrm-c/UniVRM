using System;


namespace UniHumanoid
{
    public static class BvhImporter
    {
        [Obsolete("use BvhImporter.Parse(path), then BvhImporter.Load()")]
        public static void Import(BvhImporterContext context)
        {
            context.Parse(context.Path);
            context.Load();
        }
    }
}

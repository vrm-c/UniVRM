using System.IO;
using UnityEngine;

namespace UniVRM10
{
    public static class TestAsset
    {
        public static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        public static Vrm10Instance LoadAlicia()
        {
            Vrm10Data.TryParseOrMigrate(AliciaPath, true, out Vrm10Data vrm);
            using (var loader = new Vrm10Importer(vrm))
            {
                var task = loader.LoadAsync(new VRMShaders.ImmediateCaller());
                task.Wait();

                var instance = task.Result;

                return instance.GetComponent<Vrm10Instance>();
            }
        }
    }
}

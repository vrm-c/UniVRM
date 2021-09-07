using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UniVRM10.Test
{
    public class ExpressionTests
    {
        static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        static VRM10Controller Load()
        {
            Vrm10Data.TryParseOrMigrate(AliciaPath, true, out Vrm10Data vrm);
            using (var loader = new Vrm10Importer(vrm))
            {
                var task = loader.LoadAsync(new VRMShaders.ImmediateCaller());
                task.Wait();

                var instance = task.Result;

                return instance.GetComponent<VRM10Controller>();
            }

        }

        [Test]
        public void KeyTest()
        {
            var controller = Load();

            var src = controller.Vrm.Expression.Aa.MaterialColorBindings.ToList();

            var renderers = controller.GetComponentsInChildren<Renderer>();
            var name = renderers[0].sharedMaterials[0].name;

            // add duplicate key
            src.Add(new MaterialColorBinding
            {
                BindType = UniGLTF.Extensions.VRMC_vrm.MaterialColorType.color,
                MaterialName = name,
                TargetValue = default,
            });
            src.Add(new MaterialColorBinding
            {
                BindType = UniGLTF.Extensions.VRMC_vrm.MaterialColorType.color,
                MaterialName = name,
                TargetValue = default,
            });
            controller.Vrm.Expression.Aa.MaterialColorBindings = src.ToArray();

            var r = new VRM10ControllerRuntime(controller);
        }
    }
}

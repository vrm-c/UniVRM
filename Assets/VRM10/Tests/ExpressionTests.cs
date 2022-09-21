using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UniVRM10.Test
{
    public class ExpressionTests
    {
        [Test]
        public void DuplicatedMaterialColorBindings()
        {
            var controller = TestAsset.LoadAlicia();

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

            // ok if no exception
            var r = new Vrm10Runtime(controller, ControlRigGenerationOption.None);
        }

        [Test]
        public void DuplicatedMaterialUVBindings()
        {
            var controller = TestAsset.LoadAlicia();

            var renderers = controller.GetComponentsInChildren<Renderer>();
            var name = renderers[0].sharedMaterials[0].name;

            // add duplicate key
            var src = controller.Vrm.Expression.Aa.MaterialUVBindings.ToList();
            src.Add(new MaterialUVBinding
            {
                MaterialName = name,
            });
            src.Add(new MaterialUVBinding
            {
                MaterialName = name,
            });
            controller.Vrm.Expression.Aa.MaterialUVBindings = src.ToArray();

            // ok if no exception
            var r = new Vrm10Runtime(controller, ControlRigGenerationOption.None);
        }
    }
}

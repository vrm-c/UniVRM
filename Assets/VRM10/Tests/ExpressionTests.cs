using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

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
            var r = controller.MakeRuntime(useControlRig: false);
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
            var r = controller.MakeRuntime(useControlRig: false);
        }

        [Test]
        public void MaterialUVBindings()
        {
            var controller = TestAsset.LoadAlicia();

            var renderers = controller.GetComponentsInChildren<Renderer>();
            var r = renderers[0];
            var m = r.sharedMaterials[0];

            // make expression
            controller.Vrm.Expression.Neutral = null;
            controller.Vrm.Expression.Aa = null;
            controller.Vrm.Expression.Ih = null;
            controller.Vrm.Expression.Ou = null;
            controller.Vrm.Expression.Ee = null;
            controller.Vrm.Expression.Oh = null;
            controller.Vrm.Expression.Blink = null;
            controller.Vrm.Expression.BlinkLeft = null;
            controller.Vrm.Expression.BlinkRight = null;
            controller.Vrm.Expression.Happy = null;
            controller.Vrm.Expression.Angry = null;
            controller.Vrm.Expression.Sad = null;
            controller.Vrm.Expression.Relaxed = null;
            controller.Vrm.Expression.Surprised = null;

            controller.Vrm.LookAt.LookAtType = UniGLTF.Extensions.VRMC_vrm.LookAtType.expression;
            controller.Vrm.LookAt.HorizontalInner = new CurveMapper(90, 10);
            controller.Vrm.LookAt.HorizontalOuter = new CurveMapper(90, 10);
            controller.Vrm.LookAt.VerticalDown = new CurveMapper(90, 10);
            controller.Vrm.LookAt.VerticalUp = new CurveMapper(90, 10);
            var left = new Vector2(0.33f, 0);
            AddUvOffset(ref controller.Vrm.Expression.LookLeft.MaterialUVBindings, m.name, left);
            var right = new Vector2(-0.33f, 0);
            AddUvOffset(ref controller.Vrm.Expression.LookRight.MaterialUVBindings, m.name, right);
            var up = new Vector2(0, 0.33f);
            AddUvOffset(ref controller.Vrm.Expression.LookUp.MaterialUVBindings, m.name, up);
            var down = new Vector2(0, -0.33f);
            AddUvOffset(ref controller.Vrm.Expression.LookDown.MaterialUVBindings, m.name, down);

            controller.Vrm.Expression.LookLeft.IsBinary = true;
            controller.Vrm.Expression.LookRight.IsBinary = true;
            controller.Vrm.Expression.LookUp.IsBinary = true;
            controller.Vrm.Expression.LookDown.IsBinary = true;

            // recreate expression
            controller.DisposeRuntime();

            {
                // left
                var yaw = -Mathf.PI * 0.5f;
                var pitch = 0;
                controller.Runtime.Expression.Process(new LookAtEyeDirection(yaw, pitch));
                Assert.That(m.mainTextureOffset, Is.EqualTo(left).Using(Vector2ComparerWithEqualsOperator.Instance));
            }
            {
                // right
                var yaw = Mathf.PI * 0.5f;
                var pitch = 0;
                controller.Runtime.Expression.Process(new LookAtEyeDirection(yaw, pitch));
                Assert.That(m.mainTextureOffset, Is.EqualTo(right).Using(Vector2ComparerWithEqualsOperator.Instance));
            }

            {
                // up
                var yaw = 0;
                var pitch = Mathf.PI * 0.5f;
                controller.Runtime.Expression.Process(new LookAtEyeDirection(yaw, pitch));
                Assert.That(m.mainTextureOffset, Is.EqualTo(up).Using(Vector2ComparerWithEqualsOperator.Instance));
            }

            {
                // down
                var yaw = 0;
                var pitch = -Mathf.PI * 0.5f;
                controller.Runtime.Expression.Process(new LookAtEyeDirection(yaw, pitch));
                Assert.That(m.mainTextureOffset, Is.EqualTo(down).Using(Vector2ComparerWithEqualsOperator.Instance));
            }

            {
                // left + up
                var yaw = -Mathf.PI * 0.5f;
                var pitch = Mathf.PI * 0.5f; ;
                controller.Runtime.Expression.Process(new LookAtEyeDirection(yaw, pitch));
                Assert.That(m.mainTextureOffset, Is.EqualTo(left + up).Using(Vector2ComparerWithEqualsOperator.Instance));
            }
        }

        void AddUvOffset(ref MaterialUVBinding[] array, string materialName, in Vector2 offset)
        {
            array = AddBinding(array, new MaterialUVBinding
            {
                MaterialName = materialName,
                Offset = offset,
            });

        }

        private MaterialUVBinding[] AddBinding(MaterialUVBinding[] bindings, MaterialUVBinding binding)
        {
            var list = bindings.ToList();
            list.Add(binding);
            return list.ToArray();
        }
    }
}

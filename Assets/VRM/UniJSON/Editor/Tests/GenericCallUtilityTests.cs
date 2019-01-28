using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System;


namespace UniJSON
{
    public class GenericCallUtilityTests
    {
        class Sample
        {
            public int Value
            {
                get;
                private set;
            }

            public void Set(int value)
            {
                Value = value;
            }
        }



        [Test]
        public void GenericCallUtilityTestsSimplePasses()
        {
            var s = new Sample();

            var mi = s.GetType().GetMethod("Set");

            var invoke = (Action<Sample, int>)GenericInvokeCallFactory.Create<Sample, int>(mi);
            invoke(s, 1);
            Assert.AreEqual(1, s.Value);

            var exp = (Action<Sample, int>)GenericExpressionCallFactory.Create<Sample, int>(mi);
            exp(s, 2);
            Assert.AreEqual(2, s.Value);
        }
    }
}

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

            public int Get(int _)
            {
                return Value;
            }
        }



        [Test]
        public void GenericCallUtilityTestsSimplePasses()
        {
            var s = new Sample();


            {
                var mi = s.GetType().GetMethod("Set");
                var action = GenericInvokeCallFactory.OpenAction<Sample, int>(mi);
                action(s, 1);
                Assert.AreEqual(1, s.Value);
            }

            {
                var mi = s.GetType().GetMethod("Get");
                var func = GenericInvokeCallFactory.OpenFunc<Sample, int, int>(mi);
                /*var value =*/ func(s, 1);
                Assert.AreEqual(1, s.Value);
            }

            {
                var mi = s.GetType().GetMethod("Set");
                var action = GenericExpressionCallFactory.Create<Sample, int>(mi);
                action(s, 2);
                Assert.AreEqual(2, s.Value);
            }
        }
    }
}

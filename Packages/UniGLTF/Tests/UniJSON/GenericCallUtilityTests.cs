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
    }
}

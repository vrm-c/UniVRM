using System;
using UnityEngine;

namespace UniVRM10
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class EnumFlagsAttribute : PropertyAttribute { }
}

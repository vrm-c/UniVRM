using System;
using System.Collections.Generic;

namespace UniVRM10
{
    public interface IVrmAnimation : IDisposable
    {
        (INormalizedPoseProvider, ITPoseProvider) ControlRig { get; }
        IReadOnlyDictionary<ExpressionKey, Func<float>> ExpressionMap { get; }
        public void ShowBoxMan(bool enable);
    }
}

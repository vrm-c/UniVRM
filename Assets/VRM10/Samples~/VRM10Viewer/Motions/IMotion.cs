using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public interface IMotion : IDisposable
    {
        (INormalizedPoseProvider, ITPoseProvider) ControlRig { get; }
        IDictionary<ExpressionKey, Transform> ExpressionMap { get; }

        void ShowBoxMan(bool enable);
    }
}

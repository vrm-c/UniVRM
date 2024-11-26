using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10.ClothWarp
{
    public struct SpringConstraint
    {
        public int _p0;

        public int _p1;

        // 初期長さ
        public float _rest;

        public SpringConstraint(int p0, int p1, float d)
        {
            _p0 = p0;
            _p1 = p1;
            _rest = d;
        }

        /// <summary>
        ///  フックの法則
        /// </summary>
        /// <returns></returns>
        public void Resolve(FrameTime time, float hookean, List<ClothWarpNode> list)
        {
            var p0 = list[_p0];
            var p1 = list[_p1];
            var d = Vector3.Distance(p0.State.Current, p1.State.Current);
            var f = (d - _rest) * hookean;
            var dx = (p1.State.Current - p0.State.Current).normalized * f / time.SqDt;
            p0.Force += dx;
            p1.Force -= dx;
        }

        // public void DrawGizmo(List<ClothWarp> list)
        // {
        //     Gizmos.DrawLine(list[_p0].State.Current, list[_p1].State.Current);
        // }
    }
}
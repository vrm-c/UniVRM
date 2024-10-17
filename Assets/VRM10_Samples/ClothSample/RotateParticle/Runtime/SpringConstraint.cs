using System;
using UnityEngine;

namespace RotateParticle
{
    public class SpringConstraint
    {
        RotateParticle _p0;

        RotateParticle _p1;

        // 初期長さ
        float _rest;

        public SpringConstraint(RotateParticle p0, RotateParticle p1)
        {
            _p0 = p0;
            _p1 = p1;
            _rest = Vector3.Distance(p0.State.Current, p1.State.Current);
        }

        /// <summary>
        ///  フックの法則
        /// </summary>
        /// <returns></returns>
        public void Resolve(FrameTime time, float hookean)
        {
            var d = Vector3.Distance(_p0.State.Current, _p1.State.Current);
            var f = (d - _rest) * hookean;
            var dx = (_p1.State.Current - _p0.State.Current).normalized * f / time.SqDt;

            _p0.Force += dx;
            _p1.Force -= dx;
        }

        public void DrawGizmo()
        {
            Gizmos.DrawLine(_p0.State.Current, _p1.State.Current);
        }
    }
}
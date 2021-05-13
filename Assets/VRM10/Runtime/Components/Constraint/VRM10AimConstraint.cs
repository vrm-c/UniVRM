using System;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// WIP
    /// 
    /// Slerp(thisRot * offsetRot(Aim x Up), sourceRot, weight)
    /// 
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10AimConstraint : VRM10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Quaternion DestinationOffset = Quaternion.identity;

        public Quaternion ParentRotation => transform.parent == null ? Quaternion.identity : transform.parent.rotation;

        public class AimLogic
        {
            public readonly Quaternion InitialLocalRotation;
            public AimLogic(Quaternion initialLocalRotation)
            {
                InitialLocalRotation = initialLocalRotation;
            }
        }
        public AimLogic Logic { get; private set; }


        void Start()
        {
            if (Source == null)
            {
                enabled = false;
                return;
            }

            if (Logic == null)
            {
                Logic = new AimLogic(transform.localRotation);
            }
        }

        static (float, int, float, int) CalcYawPitch(Matrix4x4 m, Vector3 target)
        {
            var zaxis = Vector3.Project(target, m.GetColumn(2));
            var yaxis = Vector3.Project(target, m.GetColumn(1));
            var xaxis = Vector3.Project(target, m.GetColumn(0));

            var xDot = Vector3.Dot(xaxis, m.GetColumn(0)) > 0;
            var yDot = Vector3.Dot(yaxis, m.GetColumn(1)) > 0;
            var zDot = Vector3.Dot(zaxis, m.GetColumn(2)) > 0;

            // xz
            var yaw = (float)System.Math.Atan2(xaxis.magnitude, zaxis.magnitude) * Mathf.Rad2Deg;
            var yawQuadrant = -1;
            if (xDot && zDot)
            {
                // 1st(0-90)
                yawQuadrant = 0;
            }
            else if (xDot && !zDot)
            {
                // 2nd(90-180)
                yawQuadrant = 1;
            }
            else if (!xDot && !zDot)
            {
                // 3rd
                yawQuadrant = 2;
            }
            else if (!xDot && zDot)
            {
                // 4th
                yawQuadrant = 3;
            }
            else
            {
                throw new NotImplementedException();
            }

            // xy
            var pitch = (float)System.Math.Atan2(yaxis.magnitude, (xaxis + zaxis).magnitude) * Mathf.Rad2Deg;
            var pitchQuadrant = -1;
            if (yDot && zDot)
            {
                // 1st
                pitchQuadrant = 0;
            }
            else if (yDot & !zDot)
            {
                // 2nd
                pitchQuadrant = 1;
            }
            else if (!yDot & !zDot)
            {
                // 3rd
                pitchQuadrant = 2;
            }
            else if (!yDot & zDot)
            {
                // 4th
                pitchQuadrant = 3;
            }

            return (yaw, yawQuadrant, pitch, pitchQuadrant);
        }

        /// <summary>
        /// TargetのUpdateよりも先か後かはその時による。
        /// 厳密に制御するのは無理。
        /// </summary>
        public override void Process()
        {
            if (Logic == null)
            {
                return;
            }

            var m = Matrix4x4.TRS(transform.position, ParentRotation * Logic.InitialLocalRotation, Vector3.one);
            m.CalcYawPitch(Source.position, out Yaw, out Pitch);
            // Delta = Quaternion.Euler(0, Yaw, 0) * Quaternion.Euler(Pitch, 0, 0);
            Delta = m.YawPitchRotation(Yaw, Pitch);
            transform.rotation = ParentRotation * Logic.InitialLocalRotation * Delta;
        }

        public float Yaw;
        public float Pitch;
        public Quaternion Delta;
    }
}

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

        // [SerializeField]
        // [Range(0, 10.0f)]
        // float Weight = 1.0f;

        /// <summary>
        /// Forward
        /// </summary>
        [SerializeField]
        public Vector3 AimVector = Vector3.forward;

        [SerializeField]
        public Vector3 UpVector = Vector3.up;

        [SerializeField]
        public Vector3 RightVector;

        Quaternion m_selfInitial;
        Matrix4x4 m_coords;

        void Start()
        {
            if (Source == null)
            {
                return;
            }

            m_selfInitial = transform.rotation;

            // 正規直交座標を作る
            // Y x Z => X
            AimVector.Normalize();
            UpVector.Normalize();
            RightVector = Vector3.Cross(UpVector, AimVector).normalized;
            // 直交するように再計算
            UpVector = Vector3.Cross(AimVector, RightVector).normalized;
            m_coords = new Matrix4x4(
                new Vector4(RightVector.x, RightVector.y, RightVector.z, 0),
                new Vector4(UpVector.x, UpVector.y, UpVector.z, 0),
                new Vector4(AimVector.x, AimVector.y, AimVector.z, 0),
                new Vector4(0, 0, 0, 1)
            );
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
            if (Source == null)
            {
                return;
            }

            var localPosition = transform.worldToLocalMatrix.MultiplyPoint(Source.position);
            // var (yaw, pitch) = CalcYawPitch(m_coords, localPosition);
        }

        void OnDrawGizmos()
        {
            if (Source == null)
            {
                return;
            }

            var localPosition = transform.worldToLocalMatrix.MultiplyPoint(Source.position);
            var (yaw, yaw_, pitch, pitch_) = CalcYawPitch(m_coords, localPosition);
            switch (yaw_)
            {
                case 0: break;
                case 1: yaw = 180 - yaw; break;
                case 2: yaw = 180 + yaw; break;
                case 3: yaw = 360 - yaw; break;
            }
            switch (pitch_)
            {
                case 0: pitch = -pitch; break;
                case 1: pitch = -pitch; break;
                case 2: break;
                case 3: break;
            }
            // Debug.Log($"{yaw}({yaw_}), {pitch}({pitch_})");
            // var rot = Quaternion.Euler(pitch, yaw, 0);
            var rot = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
            var p = rot * Vector3.forward;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, p * 5);
        }
    }
}

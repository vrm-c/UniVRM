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

        public Quaternion ParentRotation => transform.parent == null ? Quaternion.identity : transform.parent.rotation;

        [SerializeField]
        public Quaternion DestinationOffset = Quaternion.identity;

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

            var zAxis = (Source.position - transform.position).normalized;
            var xAxis = Vector3.Cross(Vector3.up, zAxis);
            var yAxis = Vector3.Cross(zAxis, xAxis);
            var m = new Matrix4x4(xAxis, yAxis, zAxis, new Vector4(0, 0, 0, 1));
            Delta = Quaternion.Inverse(ParentRotation * Logic.InitialLocalRotation * DestinationOffset) * m.rotation;
            transform.rotation = ParentRotation * Logic.InitialLocalRotation * Delta;
        }

        public float Yaw;
        public float Pitch;
        public Quaternion Delta;
    }
}

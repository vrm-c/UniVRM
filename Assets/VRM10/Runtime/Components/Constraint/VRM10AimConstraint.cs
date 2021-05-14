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

        // [SerializeField]
        // public Quaternion DestinationOffset = Quaternion.identity;

        // public class AimLogic
        // {
        //     public readonly Quaternion InitialLocalRotation;
        //     public AimLogic(Quaternion initialLocalRotation)
        //     {
        //         InitialLocalRotation = initialLocalRotation;
        //     }
        // }
        // public AimLogic Logic { get; private set; }


        void Start()
        {
            if (Source == null)
            {
                enabled = false;
                return;
            }

            // if (Logic == null)
            // {
            //     Logic = new AimLogic(transform.localRotation);
            // }
        }

        /// <summary>
        /// TargetのUpdateよりも先か後かはその時による。
        /// 厳密に制御するのは無理。
        /// </summary>
        public override void Process()
        {
            // if (Logic == null)
            // {
            //     return;
            // }

            var m = Matrix4x4.Rotate(ParentRotation);
            m.CalcYawPitch(Source.position - transform.position, out Yaw, out Pitch);
            Delta = Quaternion.Euler(0, Yaw, 0) * Quaternion.Euler(-Pitch, 0, 0);
            transform.rotation = ParentRotation * Delta;
        }

        public float Yaw;
        public float Pitch;
        public Quaternion Delta;
    }
}

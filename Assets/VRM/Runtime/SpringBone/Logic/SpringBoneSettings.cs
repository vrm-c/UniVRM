using UnityEngine;

namespace VRM.SpringBone
{
    readonly struct SpringBoneSettings
    {
        public readonly float StiffnessForce;
        public readonly float DragForce;
        public readonly Vector3 GravityDir;
        public readonly float GravityPower;
        public readonly float HitRadius;

        public SpringBoneSettings(float stiffnessForce, float dragForce, Vector3 gravityDir, float gravityPower, float hitRadius) =>
            (StiffnessForce, DragForce, GravityDir, GravityPower, HitRadius) = (stiffnessForce, dragForce, gravityDir, gravityPower, hitRadius);
    }
}
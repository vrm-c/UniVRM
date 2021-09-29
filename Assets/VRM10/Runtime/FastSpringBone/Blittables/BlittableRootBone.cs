using UnityEngine;

namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// 1本の毛束を示すBlittable型
    /// </summary>
    public unsafe readonly struct BlittableRootBone
    {
        private readonly float _gravityPower;
        private readonly Vector3 _gravityDir;
        private readonly float _dragForce;
        private readonly float _stiffnessForce;
        private readonly BlittablePoints* _blittablePoints;

        public void DrawGizmos()
        {
            for (var i = 0; i < _blittablePoints->Count; i++)
            {
                var point = (*_blittablePoints)[i];
                Gizmos.DrawWireSphere(point.CurrentPosition, 0.05f);
            }
            for (var i = 0; i < _blittablePoints->Count - 1; i++)
            {
                var point1 = (*_blittablePoints)[i];
                var point2 = (*_blittablePoints)[i + 1];
                Gizmos.DrawLine(point1.CurrentPosition, point2.CurrentPosition);
            }
        }

        public BlittableRootBone(
            float gravityPower,
            Vector3 gravityDir,
            float dragForce,
            float stiffnessForce,
            BlittablePoints* blittablePoints)
        {
            _gravityPower = gravityPower;
            _gravityDir = gravityDir;
            _dragForce = dragForce;
            _stiffnessForce = stiffnessForce;
            _blittablePoints = blittablePoints;
        }
        
        public void Update(float deltaTime)
        {
            var stiffness = _stiffnessForce * deltaTime;
            var external = _gravityDir * (_gravityPower * deltaTime);
            for (var i = 0; i < _blittablePoints->Count; i++)
            {
                var point = (*_blittablePoints)[i];

                // Pointを更新
                point.Update(
                    stiffness,
                    _dragForce,
                    external
                );

                (*_blittablePoints)[i] = point;
            }
        }
    }
}
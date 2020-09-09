using UnityEngine;

namespace MeshUtility.Gizmo
{
    public class BoneInfo
    {
        private readonly Transform _head;
        private readonly Transform _tail;
        private readonly Vector3 _headLocalForward;
        private readonly Vector3 _headLocalUp;

        public HumanBodyBones HeadBone { get; private set; }
        public HumanBodyBones TailBone { get; private set; }

        public GameObject HeadObject { get { return _head.gameObject; } }

        public BoneInfo(Transform head, Transform tail, HumanBodyBones headBone, HumanBodyBones tailBone)
        {
            _head = head;
            _tail = tail;
            _headLocalForward = (_head != null && _tail != null) ?
                _head.InverseTransformPoint(_tail.position) :
                new Vector3(0, 0, 0.1f);
            _headLocalUp = CalculateLocalUpVector(_headLocalForward);

            HeadBone = headBone;
            TailBone = tailBone;
        }

        public BoneInfo(Transform head, Vector3 headLocalDirection, HumanBodyBones headBone)
        {
            _head = head;
            _tail = null;
            _headLocalForward = headLocalDirection;
            _headLocalUp = CalculateLocalUpVector(_headLocalForward);

            HeadBone = headBone;
        }

        public override string ToString()
        {
            return $"{_head}";
        }

        public Vector3 GetHeadPosition()
        {
            if (_head == null) return Vector3.zero;

            return _head.position;
        }

        public Vector3 GetTailPosition()
        {
            if (_tail == null)
            {
                return _head.TransformPoint(_headLocalForward);
            }
            else
            {
                return _tail.position;
            }
        }

        public Vector3 GetUpVector()
        {
            if (_head == null) return Vector3.zero;

            return _head.TransformVector(_headLocalUp);
        }

        private static Vector3 CalculateLocalUpVector(Vector3 localForward)
        {
            var dotX = Mathf.Abs(Vector3.Dot(localForward, new Vector3(1, 0, 0)));
            var dotY = Mathf.Abs(Vector3.Dot(localForward, new Vector3(0, 1, 0)));
            var dotZ = Mathf.Abs(Vector3.Dot(localForward, new Vector3(0, 0, 1)));
            if (dotX > dotY && dotX > dotZ)
            {
                return new Vector3(0, 1, 0);
            }
            if (dotY > dotX && dotY > dotZ)
            {
                return new Vector3(0, 0, 1);
            }
            else
            {
                return new Vector3(0, 1, 0);
            }
        }

    }
}
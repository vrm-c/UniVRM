using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public class BoneSelector : IDisposable
    {
        List<BoneInfo> _bones;
        Animator _currentAnimator;
        public Animator CurrentAnimator
        {
            set
            {
                if (_currentAnimator == value)
                {
                    return;
                }
                if (value == null || value.avatar == null || !value.isHuman)
                {
                    _currentAnimator = null;
                    return;
                }

                _currentAnimator = value;
                _bones = BoneInfo.GetHumanoidBones(value);
            }
        }

        Camera _sceneViewCamera;
        CommandBuffer _commandBuffer;
        const CameraEvent _cameraEvent = CameraEvent.AfterForwardAlpha;

        BoneInfo _selectedBoneInfo;
        public BoneInfo SelectedBoneInfo => _selectedBoneInfo;

        BoneInfo _hoverBoneInfo;
        public BoneInfo HoverBoneInfo => _hoverBoneInfo;

        public BoneSelector(Camera camera)
        {
            _sceneViewCamera = camera;

            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = "bones";
            _sceneViewCamera.AddCommandBuffer(_cameraEvent, _commandBuffer);
        }

        public void Dispose()
        {
            CurrentAnimator = null;
            if (_commandBuffer != null)
            {
                _sceneViewCamera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
                _commandBuffer.Dispose();
                _commandBuffer = null;
            }
        }

        public void SetTarget(GameObject activeGameObject)
        {
            CurrentAnimator = activeGameObject?.GetComponentInParent<Animator>();
        }

        struct HitResult
        {
            public Vector3 pos1;
            public Vector3 pos2;
            public float s;

            public float Distance()
            {
                return Vector3.Distance(pos1, pos2);
            }
        }

        Dictionary<BoneInfo, float> _hitBones = new Dictionary<BoneInfo, float>();
        public GameObject IntersectBone(Ray ray, bool isHover = false)
        {
            if (_bones == null)
            {
                return null;
            }

            _hitBones.Clear();
            foreach (var boneInfo in _bones)
            {
                var direction = boneInfo.GetTailPosition() - boneInfo.GetHeadPosition();
                HitResult hitResult;
                if (GetClosestPosition(boneInfo.GetHeadPosition(), direction, ray, out hitResult))
                {
                    var range = (boneInfo.GetTailPosition() - boneInfo.GetHeadPosition()).magnitude * 0.1f;
                    if (range > hitResult.Distance())
                    {
                        _hitBones.Add(boneInfo, hitResult.s);
                    }
                }
            }

            // clear
            if (isHover)
            {
                return _IntersectBone(ref _hoverBoneInfo);
            }
            else
            {
                return _IntersectBone(ref _selectedBoneInfo);
            }
        }

        GameObject _IntersectBone(ref BoneInfo info)
        {
            info = null;
            if (!_hitBones.Any())
            {
                return null;
            }

            var min = _hitBones.Aggregate((result, next) => result.Value < next.Value ? result : next);
            // Debug.Log("Hit!! = " + min.Key.HeadBone);
            info = min.Key;
            return info.HeadObject;
        }
        private bool GetClosestPosition(Vector3 origin, Vector3 targetDirection, Ray ray, out HitResult hitResult)
        {
            Vector3 deltaPos3 = origin - ray.origin;
            Vector4 deltaPos4 = new Vector4(deltaPos3.x, deltaPos3.y, deltaPos3.z, 0.0f);

            Vector3 normal = Vector3.Cross(targetDirection, ray.direction);

            if (normal.magnitude < 0.001f)
            {
                hitResult = default;
                return false;
            }

            Matrix4x4 mat = Matrix4x4.identity;
            mat.SetColumn(0, new Vector4(ray.direction.x, ray.direction.y, ray.direction.z, 0.0f));
            mat.SetColumn(1, new Vector4(-targetDirection.x, -targetDirection.y, -targetDirection.z, 0.0f));
            mat.SetColumn(2, new Vector4(normal.x, normal.y, normal.z, 0.0f));
            mat = mat.inverse;

            var s = Vector4.Dot(mat.GetRow(0), deltaPos4);
            var t = Vector4.Dot(mat.GetRow(1), deltaPos4);

            if (s < 0.0f)
            {
                hitResult = default;
                return false;
            }

            if (s < 0.0f) s = 0.0f;
            //if (s > 1.0f) s = 1.0f;
            if (t < 0.0f) t = 0.0f;
            if (t > 1.0f) t = 1.0f;

            var pos1 = ray.direction * s + ray.origin;
            var pos2 = targetDirection * t + origin;
            hitResult = new HitResult()
            {
                pos1 = pos1,
                pos2 = pos2,
                s = s
            };

            return true;
        }

        public void Draw()
        {
            if (_bones == null)
            {
                return;
            }

            if (_commandBuffer != null)
            {
                _commandBuffer.Clear();
                _commandBuffer.DrawBones(_bones);

                if (_selectedBoneInfo != null)
                {
                    _commandBuffer.DrawBone(_selectedBoneInfo, BoneInfoDrawer.SelectedMaterial);
                }
                if (_hoverBoneInfo != null && _selectedBoneInfo != _hoverBoneInfo)
                {
                    _commandBuffer.DrawBone(_hoverBoneInfo, BoneInfoDrawer.HoverMaterial);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class BoneMeshRemoverValidator
    {
        private Animator _cAnimator = null;
        private Transform _cEraseRoot = null;
        private SkinnedMeshRenderer _pSkinnedMesh;
        private Animator _pAnimator;
        private Transform _pEraseRoot;

        public void Validate(SkinnedMeshRenderer _skinnedMesh, List<BoneMeshEraser.EraseBone> _eraseBones)
        {
            // any better way we can detect component change?
            if (_skinnedMesh != _pSkinnedMesh || _cAnimator != _pAnimator || _cEraseRoot != _pEraseRoot)
            {
                BoneMeshEraserValidate(_skinnedMesh, _eraseBones);
            }
            _pSkinnedMesh = _skinnedMesh;
            _pAnimator = _cAnimator;
            _pEraseRoot = _cEraseRoot;
        }

        void BoneMeshEraserValidate(SkinnedMeshRenderer skinnedMeshRenderer, List<BoneMeshEraser.EraseBone> eraseBones)
        {
            eraseBones.Clear();
            if (skinnedMeshRenderer == null)
            {
                return;
            }

            if (_cEraseRoot == null)
            {
                if (_cAnimator != null)
                {
                    _cEraseRoot = _cAnimator.GetBoneTransform(HumanBodyBones.Head);
                    //Debug.LogFormat("head: {0}", EraseRoot);
                }
            }

            eraseBones.AddRange(skinnedMeshRenderer.bones.Select(x =>
            {
                var eb = new BoneMeshEraser.EraseBone
                {
                    Bone = x,
                };

                if (_cEraseRoot != null)
                {
                    // 首の子孫を消去
                    if (eb.Bone.Ancestor().Any(y => y == _cEraseRoot))
                    {
                        //Debug.LogFormat("erase {0}", x);
                        eb.Erase = true;
                    }
                }

                return eb;
            }));
        }
    }
}

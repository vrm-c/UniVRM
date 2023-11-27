using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    public static class BoneNormalizer
    {
        public static (GameObject, Dictionary<Transform, Transform>) CreateNormalizedHierarchy(GameObject go,
            bool removeScaling = true,
            bool removeRotation = true)
        {
            var boneMap = new Dictionary<Transform, Transform>();
            var normalized = new GameObject(go.name + "(normalized)");
            normalized.transform.position = go.transform.position;

            if (removeScaling && removeRotation)
            {
                RemoveScaleAndRotationRecursive(go.transform, normalized.transform, boneMap);
            }
            else if (removeScaling)
            {
                RemoveScaleAndRotationRecursive(go.transform, normalized.transform, boneMap);
            }
            else if (removeRotation)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new ArgumentNullException();
            }

            return (normalized, boneMap);
        }

        static void RemoveScaleRecursive(Transform src, Transform dst, Dictionary<Transform, Transform> boneMap)
        {
            boneMap[src] = dst;

            foreach (Transform child in src)
            {
                if (child.gameObject.activeSelf)
                {
                    var dstChild = new GameObject(child.name);
                    dstChild.transform.SetParent(dst);
                    dstChild.transform.position = child.position; // copy world position
                    dstChild.transform.rotation = child.localToWorldMatrix.rotation; // copy world rotation
                    // scale is removed
                    RemoveScaleRecursive(child, dstChild.transform, boneMap);
                }
            }
        }

        static void RemoveScaleAndRotationRecursive(Transform src, Transform dst, Dictionary<Transform, Transform> boneMap)
        {
            boneMap[src] = dst;

            foreach (Transform child in src)
            {
                if (child.gameObject.activeSelf)
                {
                    var dstChild = new GameObject(child.name);
                    dstChild.transform.SetParent(dst);
                    dstChild.transform.position = child.position; // copy world position

                    RemoveScaleAndRotationRecursive(child, dstChild.transform, boneMap);
                }
            }
        }

        public static MeshAttachInfo CreateMeshInfo(Transform src, Dictionary<Transform, Transform> boneMap)
        {
            Transform dst;
            if (!boneMap.TryGetValue(src, out dst))
            {
                return default;
            }

            // SkinnedMeshRenderer
            var smr = src.GetComponent<SkinnedMeshRenderer>();
            var mesh = MeshFreezer.NormalizeSkinnedMesh(
                smr,
                boneMap);
            if (mesh != null)
            {
                var info = new MeshAttachInfo
                {
                    Mesh = mesh,
                    Materials = smr.sharedMaterials,
                };
                if (smr.rootBone != null)
                {
                    if (boneMap.TryGetValue(smr.rootBone, out Transform found))
                    {
                        info.RootBone = found;
                    }
                }
                return info;
            }

            // MeshRenderer
            var mr = src.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var dstMesh = MeshFreezer.NormalizeNoneSkinnedMesh(mr);
                if (dstMesh != null)
                {
                    return new MeshAttachInfo
                    {
                        Mesh = dstMesh,
                        Materials = mr.sharedMaterials,
                    };
                }
            }

            return default;
        }


        /// <summary>
        /// 回転とスケールを除去したヒエラルキーのコピーを作成する(MeshをBakeする)
        /// </summary>
        /// <param name="go">対象のヒエラルキーのルート</param>
        /// <param name="bakeCurrentBlendShape">BlendShapeを0クリアするか否か。false の場合 BlendShape の現状を Bake する</param>
        /// <param name="createAvatar">Avatarを作る関数</param>
        /// <returns></returns>
        public static (GameObject, Dictionary<Transform, Transform>, Dictionary<Transform, MeshAttachInfo>) NormalizeHierarchyFreezeMesh(
            GameObject go,
            bool removeScaling = true,
            bool removeRotation = true
        )
        {
            //
            // 正規化されたヒエラルキーを作る
            //
            var (normalized, boneMap) = CreateNormalizedHierarchy(go, removeScaling, removeRotation);

            //
            // 各メッシュから回転・スケールを取り除いてBinding行列を再計算する
            //
            var result = new Dictionary<Transform, MeshAttachInfo>();
            foreach (var src in go.transform.Traverse())
            {
                var info = CreateMeshInfo(src, boneMap);
                if (info != null)
                {
                    result.Add(src, info);
                }
            }
            return (normalized, boneMap, result);
        }

        public static void WriteBackResult(GameObject go, GameObject normalized, Dictionary<Transform, Transform> boneMap)
        {
            Func<Transform, Transform> getSrc = dst =>
            {
                foreach (var (k, v) in boneMap)
                {
                    if (v == dst)
                    {
                        return k;
                    }
                }
                throw new NotImplementedException();
            };
            foreach (var (src, dst) in boneMap)
            {
                src.localPosition = dst.localPosition;
                src.localRotation = dst.localRotation;
                src.localScale = dst.localScale;
                var srcR = src.GetComponent<SkinnedMeshRenderer>();
                var dstR = dst.GetComponent<SkinnedMeshRenderer>();
                if (srcR != null && dstR != null)
                {
                    srcR.sharedMesh = dstR.sharedMesh;
                    srcR.bones = dstR.bones.Select(x => getSrc(x)).ToArray();
                }
            }
        }
    }
}

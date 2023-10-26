using System;
using System.Collections.Generic;
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


        /// <summary>
        /// 回転とスケールを除去したヒエラルキーのコピーを作成する(MeshをBakeする)
        /// </summary>
        /// <param name="go">対象のヒエラルキーのルート</param>
        /// <param name="bakeCurrentBlendShape">BlendShapeを0クリアするか否か。false の場合 BlendShape の現状を Bake する</param>
        /// <param name="createAvatar">Avatarを作る関数</param>
        /// <returns></returns>
        public static (GameObject, Dictionary<Transform, Transform>) NormalizeHierarchyFreezeMesh(GameObject go,
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
            foreach (var src in go.transform.Traverse())
            {
                Transform dst;
                if (!boneMap.TryGetValue(src, out dst))
                {
                    continue;
                }

                {
                    // SkinnedMeshRenderer
                    var srcRenderer = src.GetComponent<SkinnedMeshRenderer>();
                    var (mesh, dstBones) = MeshFreezer.NormalizeSkinnedMesh(srcRenderer, boneMap, dst.localToWorldMatrix);
                    if (mesh != null)
                    {
                        var dstRenderer = dst.gameObject.AddComponent<SkinnedMeshRenderer>();
                        dstRenderer.sharedMaterials = srcRenderer.sharedMaterials;
                        if (srcRenderer.rootBone != null)
                        {
                            if (boneMap.TryGetValue(srcRenderer.rootBone, out Transform found))
                            {
                                dstRenderer.rootBone = found;
                            }
                        }
                        dstRenderer.bones = dstBones;
                        dstRenderer.sharedMesh = mesh;
                    }
                }

                {
                    // MeshRenderer
                    var srcRenderer = src.GetComponent<MeshRenderer>();
                    if (srcRenderer != null)
                    {
                        var dstMesh = MeshFreezer.NormalizeNoneSkinnedMesh(srcRenderer);
                        if (dstMesh != null)
                        {
                            var dstFilter = dst.gameObject.AddComponent<MeshFilter>();
                            dstFilter.sharedMesh = dstMesh;
                            // Materialをコピー
                            var dstRenderer = dst.gameObject.AddComponent<MeshRenderer>();
                            dstRenderer.sharedMaterials = srcRenderer.sharedMaterials;
                        }
                    }
                }
            }

            return (normalized, boneMap);
        }
    }
}

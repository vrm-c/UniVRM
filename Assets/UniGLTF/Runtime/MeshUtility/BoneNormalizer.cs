using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.Utils;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    public static class BoneNormalizer
    {
        private static MeshAttachInfo CreateMeshInfo(Transform src)
        {
            // SkinnedMeshRenderer
            var smr = src.GetComponent<SkinnedMeshRenderer>();
            var mesh = MeshFreezer.NormalizeSkinnedMesh(smr);
            if (mesh != null)
            {
                return new MeshAttachInfo
                {
                    Mesh = mesh,
                    Materials = smr.sharedMaterials,
                    Bones = smr.bones,
                    RootBone = smr.rootBone,
                };
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
        public static Dictionary<Transform, MeshAttachInfo> NormalizeHierarchyFreezeMesh(
            GameObject go,
            bool removeScaling = true,
            bool removeRotation = true
        )
        {
            //
            // 各メッシュから回転・スケールを取り除いてBinding行列を再計算する
            //
            var result = new Dictionary<Transform, MeshAttachInfo>();
            foreach (var src in go.transform.Traverse())
            {
                var info = CreateMeshInfo(src);
                if (info != null)
                {
                    result.Add(src, info);
                }
            }
            return result;
        }

        public static void Replace(GameObject go, Dictionary<Transform, MeshAttachInfo> newMesh,
            bool FreezeRotation, bool FreezeScaling)
        {
            var boneMap = go.transform.Traverse().ToDictionary(x => x, x => new EuclideanTransform(x.rotation, x.position));
            // Func<Transform, Transform> getSrc = dst =>
            // {
            //     foreach (var (k, v) in boneMap)
            //     {
            //         if (v == dst)
            //         {
            //             return k;
            //         }
            //     }
            //     throw new NotImplementedException();
            // };

            foreach (var (src, tr) in boneMap)
            {
                src.position = tr.Translation;
                if (FreezeRotation)
                {
                    src.rotation = Quaternion.identity;
                }
                else
                {
                    src.rotation = tr.Rotation;
                }
                if (FreezeScaling)
                {
                    src.localScale = Vector3.one;
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (newMesh.TryGetValue(src, out var info))
                {
                    info.ReplaceMesh(src.gameObject);
                }
            }
        }
    }
}
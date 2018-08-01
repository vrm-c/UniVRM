using System;
using System.Collections.Generic;
using System.Linq;
using UniHumanoid;
using UnityEngine;


namespace VRM
{
    public static class BoneNormalizer
    {
        /// <summary>
        /// 回転とスケールを除去したヒエラルキーをコピーする
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        static void CopyAndBuild(Transform src, Transform dst, Dictionary<Transform, Transform> boneMap)
        {
            boneMap[src] = dst;

            foreach (Transform child in src)
            {
                if (child.gameObject.activeSelf)
                {
                    var dstChild = new GameObject(child.name);
                    dstChild.transform.SetParent(dst);
                    dstChild.transform.position = child.position; // copy position only

                    CopyAndBuild(child, dstChild.transform, boneMap);
                }
            }
        }

        static IEnumerable<Transform> Traverse(this Transform t)
        {
            yield return t;
            foreach (Transform child in t)
            {
                foreach (var x in child.Traverse())
                {
                    yield return x;
                }
            }
        }

        static void EnforceTPose(GameObject go)
        {
            var animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                throw new ArgumentException("Animator with avatar is required");
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                throw new ArgumentException("avatar is required");
            }

            if (!avatar.isValid)
            {
                throw new ArgumentException("invalid avatar");
            }

            if (!avatar.isHuman)
            {
                throw new ArgumentException("avatar is not human");
            }

            HumanPoseTransfer.SetTPose(avatar, go.transform);
        }

        static GameObject NormalizeHierarchy(GameObject go, Dictionary<Transform, Transform> boneMap)
        {
            //
            // 回転・スケールの無いヒエラルキーをコピーする
            //
            var normalized = new GameObject(go.name + "(normalized)");
            normalized.transform.position = go.transform.position;
            CopyAndBuild(go.transform, normalized.transform, boneMap);

            //
            // 新しいヒエラルキーからAvatarを作る
            //
            {
                var src = go.GetComponent<Animator>();

                var map = Enum.GetValues(typeof(HumanBodyBones))
                    .Cast<HumanBodyBones>()
                    .Where(x => x != HumanBodyBones.LastBone)
                    .Select(x => new { Key = x, Value = src.GetBoneTransform(x) })
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Key, x => boneMap[x.Value])
                    ;

                var animator = normalized.AddComponent<Animator>();
                var vrmHuman = go.GetComponent<VRMHumanoidDescription>();
                var avatarDescription = AvatarDescription.Create();
                if (vrmHuman != null && vrmHuman.Description != null)
                {
                    avatarDescription.armStretch = vrmHuman.Description.armStretch;
                    avatarDescription.legStretch = vrmHuman.Description.legStretch;
                    avatarDescription.upperArmTwist = vrmHuman.Description.upperArmTwist;
                    avatarDescription.lowerArmTwist = vrmHuman.Description.lowerArmTwist;
                    avatarDescription.upperLegTwist = vrmHuman.Description.upperLegTwist;
                    avatarDescription.lowerLegTwist = vrmHuman.Description.lowerLegTwist;
                    avatarDescription.feetSpacing = vrmHuman.Description.feetSpacing;
                    avatarDescription.hasTranslationDoF = vrmHuman.Description.hasTranslationDoF;
                }
                avatarDescription.SetHumanBones(map);
                var avatar = avatarDescription.CreateAvatar(normalized.transform);

                avatar.name = go.name + ".normalized";
                animator.avatar = avatar;

                var humanPoseTransfer = normalized.AddComponent<HumanPoseTransfer>();
                humanPoseTransfer.Avatar = avatar;
            }

            return normalized;
        }

        /// <summary>
        /// srcのSkinnedMeshRendererを正規化して、dstにアタッチする
        /// </summary>
        /// <param name="src">正規化前のSkinnedMeshRendererのTransform</param>
        /// <param name="dst">正規化後のSkinnedMeshRendererのTransform</param>
        /// <param name="boneMap">正規化前のボーンから正規化後のボーンを得る</param>
        static void NormalizeSkinnedMesh(Transform src, Transform dst, Dictionary<Transform, Transform> boneMap)
        {
            var srcRenderer = src.GetComponent<SkinnedMeshRenderer>();
            if (srcRenderer == null 
                || !srcRenderer.enabled
                || srcRenderer.sharedMesh == null
                || srcRenderer.sharedMesh.vertexCount == 0)
            {
                // 有効なSkinnedMeshRendererが無かった
                return;
            }

            // clear blendShape
            var srcMesh = srcRenderer.sharedMesh;
            var originalSrcMesh = srcMesh;
            for (int i = 0; i < srcMesh.blendShapeCount; ++i)
            {
                srcRenderer.SetBlendShapeWeight(i, 0);
            }

            var bones = srcRenderer.bones.Select(x => boneMap[x]).ToArray();
            var hasBoneWeight = srcRenderer.bones!=null && srcRenderer.bones.Length > 0;
            if (!hasBoneWeight)
            {
                // Before bake, bind no weight bones
                //Debug.LogFormat("no weight: {0}", srcMesh.name);

                srcMesh = srcMesh.Copy();
                var bw = new BoneWeight
                {
                    boneIndex0 = 0,
                    boneIndex1 = 0,
                    boneIndex2 = 0,
                    boneIndex3 = 0,
                    weight0 = 1.0f,
                    weight1 = 0.0f,
                    weight2 = 0.0f,
                    weight3 = 0.0f,
                };
                srcMesh.boneWeights = Enumerable.Range(0, srcMesh.vertexCount).Select(x => bw).ToArray();
                srcMesh.bindposes = new Matrix4x4[] { Matrix4x4.identity };

                srcRenderer.rootBone = srcRenderer.transform;
                bones = new[] { boneMap[srcRenderer.transform] };
                srcRenderer.bones = new[] { srcRenderer.transform };
                srcRenderer.sharedMesh = srcMesh;
            }

            // BakeMesh
            var mesh = srcMesh.Copy();
            mesh.name = srcMesh.name + ".baked";
            srcRenderer.BakeMesh(mesh);
            mesh.boneWeights = srcMesh.boneWeights; // restore weights. clear when BakeMesh
            // recalc bindposes
            mesh.bindposes = bones.Select(x => x.worldToLocalMatrix * dst.transform.localToWorldMatrix).ToArray();

            //var m = src.localToWorldMatrix; // include scaling
            var m = default(Matrix4x4);
            m.SetTRS(Vector3.zero, src.rotation, Vector3.one); // rotation only
            mesh.ApplyMatrix(m);

            //
            // BlendShapes
            //
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshTangents = mesh.tangents.Select(x => (Vector3)x).ToArray();

            var _meshVertices = new Vector3[meshVertices.Length];
            var _meshNormals = new Vector3[meshVertices.Length];
            var _meshTangents = new Vector3[meshVertices.Length];

            var blendShapeMesh = new Mesh();
            for (int i = 0; i < srcMesh.blendShapeCount; ++i)
            {
                // check blendShape
                srcRenderer.sharedMesh.GetBlendShapeFrameVertices(i, 0, _meshVertices, _meshNormals, _meshTangents);
                var hasVertices = !_meshVertices.All(x => x == Vector3.zero);
                var hasNormals = !_meshNormals.All(x => x == Vector3.zero);
                var hasTangents = !_meshTangents.All(x => x == Vector3.zero);

                srcRenderer.SetBlendShapeWeight(i, 100.0f);
                srcRenderer.BakeMesh(blendShapeMesh);
                if (blendShapeMesh.vertices.Length != mesh.vertices.Length)
                {
                    throw new Exception("diffrent vertex count");
                }
                srcRenderer.SetBlendShapeWeight(i, 0);

                Vector3[] vertices = null;
                if (hasVertices)
                {
                    vertices = blendShapeMesh.vertices;
                    // to delta
                    for (int j = 0; j < vertices.Length; ++j)
                    {
                        vertices[j] = m.MultiplyPoint(vertices[j]) - meshVertices[j];
                    }
                }
                else
                {
                    vertices = new Vector3[mesh.vertexCount];
                }

                Vector3[] normals = null;
                if (hasNormals)
                {
                    normals = blendShapeMesh.normals;
                    // to delta
                    for (int j = 0; j < normals.Length; ++j)
                    {
                        normals[j] = m.MultiplyVector(normals[j]) - meshNormals[j];
                    }
                }
                else
                {
                    normals = new Vector3[mesh.vertexCount];
                }

                Vector3[] tangents = null;
                if (hasTangents)
                {
                    tangents = blendShapeMesh.tangents.Select(x => (Vector3)x).ToArray();
                    // to delta
                    for (int j = 0; j < tangents.Length; ++j)
                    {
                        tangents[j] = m.MultiplyVector(tangents[j]) - meshTangents[j];
                    }
                }
                else
                {
                    tangents = new Vector3[mesh.vertexCount];
                }

                var name = srcMesh.GetBlendShapeName(i);
                if (string.IsNullOrEmpty(name))
                {
                    name = String.Format("{0}", i);
                }

                var weight = srcMesh.GetBlendShapeFrameWeight(i, 0);

                try
                {
                    mesh.AddBlendShapeFrame(name,
                        weight,
                        vertices,
                        normals,
                        tangents
                        );
                }
                catch (Exception)
                {
                    Debug.LogErrorFormat("fail to mesh.AddBlendShapeFrame {0}.{1}",
                        mesh.name,
                        srcMesh.GetBlendShapeName(i)
                        );
                    throw;
                }
            }

            var dstRenderer = dst.gameObject.AddComponent<SkinnedMeshRenderer>();
            dstRenderer.sharedMaterials = srcRenderer.sharedMaterials;
            if (srcRenderer.rootBone != null)
            {
                dstRenderer.rootBone = boneMap[srcRenderer.rootBone];
            }
            dstRenderer.bones = bones;
            dstRenderer.sharedMesh = mesh;

            if (!hasBoneWeight)
            {
                // restore bones
                srcRenderer.bones = new Transform[] { };
                srcRenderer.sharedMesh = originalSrcMesh;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        static void NormalizeNoneSkinnedMesh(Transform src, Transform dst)
        {
            var srcFilter = src.GetComponent<MeshFilter>();
            if (srcFilter == null
                || srcFilter.sharedMesh == null
                || srcFilter.sharedMesh.vertexCount == 0)
            {
                return;
            }

            var srcRenderer = src.GetComponent<MeshRenderer>();
            if (srcRenderer == null || !srcRenderer.enabled)
            {
                return;
            }

            // Meshに乗っているボーンの姿勢を適用する
            var dstFilter = dst.gameObject.AddComponent<MeshFilter>();

            var dstMesh = srcFilter.sharedMesh.Copy();
            dstMesh.ApplyRotationAndScale(src.localToWorldMatrix);
            dstFilter.sharedMesh = dstMesh;

            // Materialをコピー
            var dstRenderer = dst.gameObject.AddComponent<MeshRenderer>();
            dstRenderer.sharedMaterials = srcRenderer.sharedMaterials;
        }

        public struct NormalizedResult
        {
            public GameObject Root;
            public Dictionary<Transform, Transform> BoneMap;
        }

        /// <summary>
        /// モデルの正規化を実行する
        /// </summary>
        /// <param name="go">対象モデルのルート</param>
        /// <param name="forceTPose">強制的にT-Pose化するか</param>
        /// <returns>正規化済みのモデル</returns>
        public static NormalizedResult Execute(GameObject go, bool forceTPose)
        {
            Dictionary<Transform, Transform> boneMap = new Dictionary<Transform, Transform>();

            //
            // T-Poseにする
            //
            if (forceTPose)
            {
                EnforceTPose(go);
            }

            //
            // 正規化されたヒエラルキーを作る
            //
            var normalized = NormalizeHierarchy(go, boneMap);

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

                NormalizeSkinnedMesh(src, dst, boneMap);

                NormalizeNoneSkinnedMesh(src, dst);
            }

            return new NormalizedResult
            {
                Root = normalized,
                BoneMap = boneMap
            };
        }
    }
}

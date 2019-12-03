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

                var srcHumanBones = Enum.GetValues(typeof(HumanBodyBones))
                    .Cast<HumanBodyBones>()
                    .Where(x => x != HumanBodyBones.LastBone)
                    .Select(x => new { Key = x, Value = src.GetBoneTransform(x) })
                    .Where(x => x.Value != null)
                    ;

                var map =
                       srcHumanBones
                       .Where(x => boneMap.ContainsKey(x.Value))
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

        class BlendShapeReport
        {
            string m_name;
            int m_count;
            struct BlendShapeStat
            {
                public int Index;
                public string Name;
                public int VertexCount;
                public int NormalCount;
                public int TangentCount;

                public override string ToString()
                {
                    return string.Format("[{0}]{1}: {2}, {3}, {4}\n", Index, Name, VertexCount, NormalCount, TangentCount);
                }
            }
            List<BlendShapeStat> m_stats = new List<BlendShapeStat>();
            public int Count
            {
                get { return m_stats.Count; }
            }
            public BlendShapeReport(Mesh mesh)
            {
                m_name = mesh.name;
                m_count = mesh.vertexCount;
            }
            public void SetCount(int index, string name, int v, int n, int t)
            {
                m_stats.Add(new BlendShapeStat
                {
                    Index = index,
                    Name = name,
                    VertexCount = v,
                    NormalCount = n,
                    TangentCount = t,
                });
            }
            public override string ToString()
            {
                return String.Format("NormalizeSkinnedMesh: {0}({1}verts)\n{2}",
                    m_name,
                    m_count,
                    String.Join("", m_stats.Select(x => x.ToString()).ToArray()));
            }
        }

        static BoneWeight[] MapBoneWeight(BoneWeight[] src,
            Dictionary<Transform, Transform> boneMap,
            Transform[] srcBones,
            Transform[] dstBones
            )
        {
            var indexMap =
            srcBones
                .Select((x, i) => new { i, x })
                .Select(x => {
                    Transform dstBone;
                    if(boneMap.TryGetValue(x.x, out dstBone))
                    {
                        return dstBones.IndexOf(dstBone);
                    }
                    else
                    {
                        return -1;
                    }
                 })
                .ToArray();

            for (int i = 0; i < srcBones.Length; ++i)
            {
                if (indexMap[i] < 0)
                {
                    Debug.LogWarningFormat("{0} is removed", srcBones[i].name);
                }
            }

            var dst = new BoneWeight[src.Length];
            Array.Copy(src, dst, src.Length);

            for (int i = 0; i < src.Length; ++i)
            {
                var x = src[i];

                if (indexMap[x.boneIndex0] != -1)
                {
                    dst[i].boneIndex0 = indexMap[x.boneIndex0];
                    dst[i].weight0 = x.weight0;
                }
                else if (x.weight0 > 0)
                {
                    Debug.LogWarningFormat("{0} weight0 to {1} is lost", i, srcBones[x.boneIndex0].name);
                    dst[i].weight0 = 0;
                }

                if (indexMap[x.boneIndex1] != -1)
                {
                    dst[i].boneIndex1 = indexMap[x.boneIndex1];
                    dst[i].weight1 = x.weight1;
                }
                else if (x.weight1 > 0)
                {
                    Debug.LogWarningFormat("{0} weight0 to {1} is lost", i, srcBones[x.boneIndex1].name);
                    dst[i].weight1 = 0;
                }

                if (indexMap[x.boneIndex2] != -1)
                {
                    dst[i].boneIndex2 = indexMap[x.boneIndex2];
                    dst[i].weight2 = x.weight2;
                }
                else if (x.weight2 > 0)
                {
                    Debug.LogWarningFormat("{0} weight0 to {1} is lost", i, srcBones[x.boneIndex2].name);
                    dst[i].weight2 = 0;
                }

                if (indexMap[x.boneIndex3] != -1)
                {
                    dst[i].boneIndex3 = indexMap[x.boneIndex3];
                    dst[i].weight3 = x.weight3;
                }
                else if (x.weight3 > 0)
                {
                    Debug.LogWarningFormat("{0} weight0 to {1} is lost", i, srcBones[x.boneIndex3].name);
                    dst[i].weight3 = 0;
                }
            }

            return dst;
        }

        /// <summary>
        /// srcのSkinnedMeshRendererを正規化して、dstにアタッチする
        /// </summary>
        /// <param name="src">正規化前のSkinnedMeshRendererのTransform</param>
        /// <param name="dst">正規化後のSkinnedMeshRendererのTransform</param>
        /// <param name="boneMap">正規化前のボーンから正規化後のボーンを得る</param>
        static void NormalizeSkinnedMesh(Transform src, Transform dst, Dictionary<Transform, Transform> boneMap, bool clearBlendShape)
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

            var srcMesh = srcRenderer.sharedMesh;
            var originalSrcMesh = srcMesh;

            // clear blendShape
            if (clearBlendShape)
            {
                for (int i = 0; i < srcMesh.blendShapeCount; ++i)
                {
                    srcRenderer.SetBlendShapeWeight(i, 0);
                }
            }

            var dstBones = srcRenderer.bones
                .Where(x => boneMap.ContainsKey(x))
                .Select(x => boneMap[x])
                .ToArray();
            var hasBoneWeight = srcRenderer.bones != null && srcRenderer.bones.Length > 0;
            if (!hasBoneWeight)
            {
                // Before bake, bind no weight bones
                //Debug.LogFormat("no weight: {0}", srcMesh.name);

                srcMesh = srcMesh.Copy(true);
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
                dstBones = new[] { boneMap[srcRenderer.transform] };
                srcRenderer.bones = new[] { srcRenderer.transform };
                srcRenderer.sharedMesh = srcMesh;
            }

            // BakeMesh
            var mesh = srcMesh.Copy(false);
            mesh.name = srcMesh.name + ".baked";
            srcRenderer.BakeMesh(mesh);
           
            var blendShapeValues = new Dictionary<int,float>();
            for (int i = 0; i < srcMesh.blendShapeCount; i++)
            {
                var val = srcRenderer.GetBlendShapeWeight(i);
                if (val > 0) blendShapeValues.Add(i, val);
            }
        
            mesh.boneWeights = MapBoneWeight(srcMesh.boneWeights, boneMap, srcRenderer.bones, dstBones); // restore weights. clear when BakeMesh

            // recalc bindposes
            mesh.bindposes = dstBones.Select(x => x.worldToLocalMatrix * dst.transform.localToWorldMatrix).ToArray();

            //var m = src.localToWorldMatrix; // include scaling
            var m = default(Matrix4x4);
            m.SetTRS(Vector3.zero, src.rotation, Vector3.one); // rotation only
            mesh.ApplyMatrix(m);

            //
            // BlendShapes
            //
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
#if VRM_NORMALIZE_BLENDSHAPE_TANGENT
            var meshTangents = mesh.tangents.Select(x => (Vector3)x).ToArray();
#endif

            var originalBlendShapePositions = new Vector3[meshVertices.Length];
            var originalBlendShapeNormals = new Vector3[meshVertices.Length];
            var originalBlendShapeTangents = new Vector3[meshVertices.Length];

            var report = new BlendShapeReport(srcMesh);
            var blendShapeMesh = new Mesh();
            for (int i = 0; i < srcMesh.blendShapeCount; ++i)
            {
                // check blendShape
                srcRenderer.sharedMesh.GetBlendShapeFrameVertices(i, 0, originalBlendShapePositions, originalBlendShapeNormals, originalBlendShapeTangents);
                var hasVertices = originalBlendShapePositions.Count(x => x != Vector3.zero);
                var hasNormals = originalBlendShapeNormals.Count(x => x != Vector3.zero);
#if VRM_NORMALIZE_BLENDSHAPE_TANGENT
                var hasTangents = originalBlendShapeTangents.Count(x => x != Vector3.zero);
#else
                var hasTangents = 0;
#endif
                var name = srcMesh.GetBlendShapeName(i);
                if (string.IsNullOrEmpty(name))
                {
                    name = String.Format("{0}", i);
                }

                report.SetCount(i, name, hasVertices, hasNormals, hasTangents);

                srcRenderer.SetBlendShapeWeight(i, 100.0f);
                srcRenderer.BakeMesh(blendShapeMesh);
                if (blendShapeMesh.vertices.Length != mesh.vertices.Length)
                {
                    throw new Exception("different vertex count");
                }

                var value = blendShapeValues.ContainsKey(i) ? blendShapeValues[i] : 0;
                srcRenderer.SetBlendShapeWeight(i, value);

                Vector3[] vertices = blendShapeMesh.vertices;
                
                for (int j = 0; j < vertices.Length; ++j)
                {
                    if (originalBlendShapePositions[j] == Vector3.zero)
                    {
                        vertices[j] = Vector3.zero;
                    }
                    else
                    {
                        vertices[j] = m.MultiplyPoint(vertices[j]) - meshVertices[j];
                    }
                }

                Vector3[] normals = blendShapeMesh.normals;
                for (int j = 0; j < normals.Length; ++j)
                {
                    if (originalBlendShapeNormals[j] == Vector3.zero)
                    {
                        normals[j] = Vector3.zero;
                        
                    }
                    else
                    {
                        normals[j] = m.MultiplyVector(normals[j]) - meshNormals[j];
                    }
                }

                Vector3[] tangents = blendShapeMesh.tangents.Select(x => (Vector3)x).ToArray();
#if VRM_NORMALIZE_BLENDSHAPE_TANGENT
                for (int j = 0; j < tangents.Length; ++j)
                {
                    if (originalBlendShapeTangents[j] == Vector3.zero)
                    {
                        tangents[j] = Vector3.zero;
                    }
                    else
                    {
                        tangents[j] = m.MultiplyVector(tangents[j]) - meshTangents[j];
                    }
                }
#endif

                var weight = srcMesh.GetBlendShapeFrameWeight(i, 0);

                try
                {
                    mesh.AddBlendShapeFrame(name,
                        weight,
                        vertices,
                        hasNormals > 0 ? normals : null,
                        hasTangents > 0 ? tangents : null
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

            if (report.Count > 0)
            {
                Debug.LogFormat("{0}", report.ToString());
            }

            var dstRenderer = dst.gameObject.AddComponent<SkinnedMeshRenderer>();
            dstRenderer.sharedMaterials = srcRenderer.sharedMaterials;
            if (srcRenderer.rootBone != null)
            {
                dstRenderer.rootBone = boneMap[srcRenderer.rootBone];
            }
            dstRenderer.bones = dstBones;
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

            var dstMesh = srcFilter.sharedMesh.Copy(false);
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
        public static NormalizedResult Execute(GameObject go, bool forceTPose, bool clearBlendShapeBeforeNormalize)
        {
            Dictionary<Transform, Transform> boneMap = new Dictionary<Transform, Transform>();

            //
            // T-Poseにする
            //
            if (forceTPose)
            {
                var hips = go.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                var hipsPosition = hips.position;
                var hipsRotation = hips.rotation;
                try
                {
                    EnforceTPose(go);
                }
                finally
                {
                    hips.position = hipsPosition; // restore hipsPosition
                    hips.rotation = hipsRotation;
                }
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

                NormalizeSkinnedMesh(src, dst, boneMap, clearBlendShapeBeforeNormalize);

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

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

        /// <summary>
        /// index が 有効であれば、setter に weight を渡す。無効であれば setter に 0 を渡す。
        /// </summary>
        /// <param name="indexMap"></param>
        /// <param name="srcIndex"></param>
        /// <param name="weight"></param>
        /// <param name="setter"></param>
        static bool CopyOrDropWeight(int[] indexMap, int srcIndex, float weight, Action<int, float> setter)
        {
            if (srcIndex < 0 || srcIndex >= indexMap.Length)
            {
                // ありえるかどうかわからないが BoneWeight.boneIndexN に変な値が入っている. 
                setter(0, 0);
                return false;
            }

            var dstIndex = indexMap[srcIndex];
            if (dstIndex != -1)
            {
                // 有効なindex。weightをコピーする
                setter(dstIndex, weight);
                return true;
            }
            else
            {
                // 無効なindex。0でクリアする
                setter(0, 0);
                return false;
            }
        }

        /// <summary>
        /// BoneWeight[] src から新しいボーンウェイトを作成する。
        /// </summary>
        /// <param name="src">変更前のBoneWeight[]</param>
        /// <param name="boneMap">新旧のボーンの対応表。新しい方は無効なボーンが除去されてnullの部分がある</param>
        /// <param name="srcBones">変更前のボーン配列</param>
        /// <param name="dstBones">変更後のボーン配列。除去されたボーンがある場合、変更前より短い</param>
        /// <returns></returns>
        public static BoneWeight[] MapBoneWeight(BoneWeight[] src,
            Dictionary<Transform, Transform> boneMap,
            Transform[] srcBones,
            Transform[] dstBones
            )
        {
            // 処理前後の index の対応表を作成する
            var indexMap = new int[srcBones.Length];
            for (int i = 0; i < srcBones.Length; ++i)
            {
                var srcBone = srcBones[i];
                if (srcBone == null)
                {
                    // 元のボーンが無い
                    indexMap[i] = -1;
                    Debug.LogWarningFormat("bones[{0}] is null", i);
                }
                else
                {
                    if (boneMap.TryGetValue(srcBone, out Transform dstBone))
                    {
                        // 対応するボーンが存在する
                        var dstIndex = dstBones.IndexOf(dstBone);
                        if (dstIndex == -1)
                        {
                            // ありえない。バグ
                            throw new Exception();
                        }
                        indexMap[i] = dstIndex;
                    }
                    else
                    {
                        // 先のボーンが無い
                        indexMap[i] = -1;
                        Debug.LogWarningFormat("{0} is removed", srcBone.name);
                    }
                }
            }

            // 新しいBoneWeightを作成する
            var newBoneWeights = new BoneWeight[src.Length];
            for (int i = 0; i < src.Length; ++i)
            {
                BoneWeight srcBoneWeight = src[i];

                // 0
                CopyOrDropWeight(indexMap, srcBoneWeight.boneIndex0, srcBoneWeight.weight0, (newIndex, newWeight) =>
                {
                    newBoneWeights[i].boneIndex0 = newIndex;
                    newBoneWeights[i].weight0 = newWeight;
                });
                // 1
                CopyOrDropWeight(indexMap, srcBoneWeight.boneIndex1, srcBoneWeight.weight1, (newIndex, newWeight) =>
                {
                    newBoneWeights[i].boneIndex1 = newIndex;
                    newBoneWeights[i].weight1 = newWeight;
                });
                // 2
                CopyOrDropWeight(indexMap, srcBoneWeight.boneIndex2, srcBoneWeight.weight2, (newIndex, newWeight) =>
                {
                    newBoneWeights[i].boneIndex2 = newIndex;
                    newBoneWeights[i].weight2 = newWeight;
                });
                // 3
                CopyOrDropWeight(indexMap, srcBoneWeight.boneIndex3, srcBoneWeight.weight3, (newIndex, newWeight) =>
                {
                    newBoneWeights[i].boneIndex3 = newIndex;
                    newBoneWeights[i].weight3 = newWeight;
                });
            }

            return newBoneWeights;
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

            // 元の Transform[] bones から、無効なboneを取り除いて前に詰めた配列を作る
            var dstBones = srcRenderer.bones
                .Where(x => x != null && boneMap.ContainsKey(x))
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

            var blendShapeValues = new Dictionary<int, float>();
            for (int i = 0; i < srcMesh.blendShapeCount; i++)
            {
                var val = srcRenderer.GetBlendShapeWeight(i);
                if (val > 0) blendShapeValues.Add(i, val);
            }

            // 新しい骨格のボーンウェイトを作成する
            mesh.boneWeights = MapBoneWeight(srcMesh.boneWeights, boneMap, srcRenderer.bones, dstBones);

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

                var frameCount = srcMesh.GetBlendShapeFrameCount(i);
                for (int f = 0; f < frameCount; f++)
                {

                    var weight = srcMesh.GetBlendShapeFrameWeight(i, f);

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
        public static GameObject Execute(GameObject go, bool forceTPose, bool clearBlendShapeBeforeNormalize)
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

            CopyVRMComponents(go, normalized, boneMap);

            // return new NormalizedResult
            // {
            //     Root = normalized,
            //     BoneMap = boneMap
            // };
            return normalized;
        }

        /// <summary>
        /// VRMを構成するコンポーネントをコピーする。
        /// </summary>
        /// <param name="go">コピー元</param>
        /// <param name="root">コピー先</param>
        /// <param name="map">コピー元とコピー先の対応関係</param>
        static void CopyVRMComponents(GameObject go, GameObject root,
            Dictionary<Transform, Transform> map)
        {
            {
                // blendshape
                var src = go.GetComponent<VRMBlendShapeProxy>();
                if (src != null)
                {
                    var dst = root.AddComponent<VRMBlendShapeProxy>();
                    dst.BlendShapeAvatar = src.BlendShapeAvatar;
                }
            }

            {
                var secondary = go.transform.Find("secondary");
                if (secondary == null)
                {
                    secondary = go.transform;
                }

                var dstSecondary = root.transform.Find("secondary");
                if (dstSecondary == null)
                {
                    dstSecondary = new GameObject("secondary").transform;
                    dstSecondary.SetParent(root.transform, false);
                }

                // 揺れモノ
                foreach (var src in go.transform.GetComponentsInChildren<VRMSpringBoneColliderGroup>())
                {
                    var dst = map[src.transform];
                    var dstColliderGroup = dst.gameObject.AddComponent<VRMSpringBoneColliderGroup>();
                    dstColliderGroup.Colliders = src.Colliders.Select(y =>
                    {
                        var offset = dst.worldToLocalMatrix.MultiplyPoint(src.transform.localToWorldMatrix.MultiplyPoint(y.Offset));
                        return new VRMSpringBoneColliderGroup.SphereCollider
                        {
                            Offset = offset,
                            Radius = y.Radius
                        };
                    }).ToArray();
                }

                foreach (var src in go.transform.GetComponentsInChildren<VRMSpringBone>())
                {
                    // Copy VRMSpringBone
                    var dst = dstSecondary.gameObject.AddComponent<VRMSpringBone>();
                    dst.m_comment = src.m_comment;
                    dst.m_stiffnessForce = src.m_stiffnessForce;
                    dst.m_gravityPower = src.m_gravityPower;
                    dst.m_gravityDir = src.m_gravityDir;
                    dst.m_dragForce = src.m_dragForce;
                    if (src.m_center != null)
                    {
                        dst.m_center = map[src.m_center];
                    }

                    dst.RootBones = src.RootBones.Select(x => map[x]).ToList();
                    dst.m_hitRadius = src.m_hitRadius;
                    if (src.ColliderGroups != null)
                    {
                        dst.ColliderGroups = src.ColliderGroups
                            .Select(x => map[x.transform].GetComponent<VRMSpringBoneColliderGroup>()).ToArray();
                    }
                }
            }

#pragma warning disable 0618
            {
                // meta(obsolete)
                var src = go.GetComponent<VRMMetaInformation>();
                if (src != null)
                {
                    src.CopyTo(root);
                }
            }
#pragma warning restore 0618

            {
                // meta
                var src = go.GetComponent<VRMMeta>();
                if (src != null)
                {
                    var dst = root.AddComponent<VRMMeta>();
                    dst.Meta = src.Meta;
                }
            }

            {
                // firstPerson
                var src = go.GetComponent<VRMFirstPerson>();
                if (src != null)
                {
                    src.CopyTo(root, map);
                }
            }

            {
                // humanoid
                var dst = root.AddComponent<VRMHumanoidDescription>();
                var src = go.GetComponent<VRMHumanoidDescription>();
                if (src != null)
                {
                    dst.Avatar = src.Avatar;
                    dst.Description = src.Description;
                }
                else
                {
                    var animator = go.GetComponent<Animator>();
                    if (animator != null)
                    {
                        dst.Avatar = animator.avatar;
                    }
                }
            }
        }
    }
}

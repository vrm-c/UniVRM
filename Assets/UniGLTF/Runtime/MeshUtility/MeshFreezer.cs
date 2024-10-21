using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class MeshFreezer
    {
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
                        var dstIndex = Array.IndexOf(dstBones, dstBone);
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

        public static Mesh NormalizeSkinnedMesh(SkinnedMeshRenderer src, bool bakeCurrentBlendShape)
        {
            if (src == null
                || !src.enabled
                || src.sharedMesh == null
                || src.sharedMesh.vertexCount == 0)
            {
                // 有効なSkinnedMeshRendererが無かった
                return default;
            }

            if (!HasBoneWeight(src))
            {
                // Before bake, bind no weight bones
                //
                // blendshape があって bone が無い SkinnedMeshRenderer と思われる。
                // SkinnedMeshRenderer.transform に対する boneweight を付与する。
                AssignSingleBoneWeight(src);
            }

            var backcup = new List<float>();
            for (int i = 0; i < src.sharedMesh.blendShapeCount; ++i)
            {
                backcup.Add(src.GetBlendShapeWeight(i));
            }

            // BakeMesh
            var mesh = new Mesh
            {
                name = src.sharedMesh.name + ".baked"
            };

            if (bakeCurrentBlendShape)
            {
                // blendShape weight の現状を使用する
                src.BakeMesh(mesh);
                BakeBlendShapes(src, mesh, backcup);
            }
            else
            {
                // blendShape weight をすべて 0 clear する
                for (int i = 0; i < src.sharedMesh.blendShapeCount; ++i)
                {
                    src.SetBlendShapeWeight(i, 0.0f);
                }
                src.BakeMesh(mesh);
                BakeBlendShapes(src, mesh, null);
            }

            // restore blendshape weights
            for (int i = 0; i < backcup.Count; ++i)
            {
                src.SetBlendShapeWeight(i, backcup[i]);
            }

            mesh.boneWeights = src.sharedMesh.boneWeights;

            // apply SkinnedMesh.transform rotation
            var m = Matrix4x4.TRS(Vector3.zero, src.transform.rotation, Vector3.one);
            mesh.ApplyMatrixAlsoBlendShapes(m);

            return mesh;
        }

        public static bool HasBoneWeight(SkinnedMeshRenderer src)
        {
            return src.bones != null && src.bones.Length > 0;
        }

        public static void AssignSingleBoneWeight(SkinnedMeshRenderer src)
        {
            var srcMesh = src.sharedMesh.Copy(true);
            srcMesh.ApplyRotationAndScale(src.transform.localToWorldMatrix, false);

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
            src.bones = new[] { src.rootBone ?? src.transform };
            srcMesh.bindposes = src.bones.Select(x => x.worldToLocalMatrix).ToArray();

            src.sharedMesh = srcMesh;
        }

        /// <param name="blendShapeBase">basemesh の blendshape 状態</param>
        private static void BakeBlendShapes(SkinnedMeshRenderer src, Mesh mesh, List<float> blendShapeBase)
        {
            var srcMesh = src.sharedMesh;

            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshTangents = Array.Empty<Vector3>();
            if (Symbols.VRM_NORMALIZE_BLENDSHAPE_TANGENT)
            {
                meshTangents = mesh.tangents.Select(x => (Vector3)x).ToArray();
            }

            var originalBlendShapePositions = new Vector3[meshVertices.Length];
            var originalBlendShapeNormals = new Vector3[meshVertices.Length];
            var originalBlendShapeTangents = new Vector3[meshVertices.Length];

            var report = new BlendShapeReport(srcMesh);
            var blendShapeMesh = new Mesh();
            for (int i = 0; i < srcMesh.blendShapeCount; ++i)
            {
                // check blendShape
                src.sharedMesh.GetBlendShapeFrameVertices(i, 0, originalBlendShapePositions, originalBlendShapeNormals, originalBlendShapeTangents);
                var hasVertices = originalBlendShapePositions.Count(x => x != Vector3.zero);
                var hasNormals = originalBlendShapeNormals.Count(x => x != Vector3.zero);
                var hasTangents = 0;
                if (Symbols.VRM_NORMALIZE_BLENDSHAPE_TANGENT)
                {
                    hasTangents = originalBlendShapeTangents.Count(x => x != Vector3.zero);
                }
                var name = srcMesh.GetBlendShapeName(i);
                if (string.IsNullOrEmpty(name))
                {
                    name = String.Format("{0}", i);
                }

                report.SetCount(i, name, hasVertices, hasNormals, hasTangents);

                src.SetBlendShapeWeight(i, 100.0f);
                src.BakeMesh(blendShapeMesh);
                if (blendShapeMesh.vertices.Length != mesh.vertices.Length)
                {
                    throw new Exception("different vertex count");
                }

                src.SetBlendShapeWeight(i, blendShapeBase == null ? 0 : blendShapeBase[i]);

                Vector3[] vertices = blendShapeMesh.vertices;

                for (int j = 0; j < vertices.Length; ++j)
                {
                    vertices[j] = vertices[j] - meshVertices[j];
                }

                Vector3[] normals = blendShapeMesh.normals;
                for (int j = 0; j < normals.Length; ++j)
                {
                    normals[j] = normals[j].normalized - meshNormals[j].normalized;
                }

                Vector3[] tangents = blendShapeMesh.tangents.Select(x => (Vector3)x).ToArray();
                if (Symbols.VRM_NORMALIZE_BLENDSHAPE_TANGENT)
                {
                    for (int j = 0; j < tangents.Length; ++j)
                    {
                        tangents[j] = tangents[j].normalized - meshTangents[j].normalized;
                    }
                }

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
        }

        public static Mesh NormalizeNoneSkinnedMesh(MeshRenderer srcRenderer, bool freezeRotation)
        {
            if (srcRenderer == null || !srcRenderer.enabled)
            {
                return default;
            }

            if (srcRenderer.TryGetComponent<MeshFilter>(out var srcFilter))
            {
                if (srcFilter.sharedMesh == null || srcFilter.sharedMesh.vertexCount == 0)
                {
                    return default;
                }
            }
            else
            {
                return default;
            }

            var dstMesh = srcFilter.sharedMesh.Copy(false);
            // Meshに乗っているボーンの姿勢を適用する
            if (freezeRotation)
            {
                dstMesh.ApplyRotationAndScale(srcRenderer.transform.localToWorldMatrix);
            }
            else
            {
                var (t, r, s) = srcRenderer.transform.localToWorldMatrix.Decompose();
                dstMesh.ApplyRotationAndScale(Matrix4x4.TRS(t, Quaternion.identity, s));
            }
            return dstMesh;
        }
    }
}

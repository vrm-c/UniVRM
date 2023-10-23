using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    /// <summary>
    /// BlendShape の有無で Mesh を分割する
    /// </summary>
    public static class TabMeshSeparator
    {
        private const string ASSET_SUFFIX = ".mesh.asset";

        private enum BlendShapeLogic
        {
            WithBlendShape,
            WithoutBlendShape,
        }

        public static bool TryExecutable(GameObject root, out string msg)
        {
            if (root == null)
            {
                msg = MeshUtilityMessages.NO_GAMEOBJECT_SELECTED.Msg();
                return false;
            }

            if (root.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0)
            {
                msg = MeshUtilityMessages.NO_SKINNED_MESH.Msg();
                return false;
            }

            msg = "";
            return true;
        }

        public static bool OnGUI(GameObject root)
        {
            var _isInvokeSuccess = false;
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                {
                    _isInvokeSuccess = TabMeshSeparator.Execute(root);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        private static bool Execute(GameObject root)
        {
            // copy
            var outputObject = GameObject.Instantiate(root);
            outputObject.name = outputObject.name + "_mesh_separation";

            // 改変と asset の作成
            var list = SeparationProcessing(outputObject);

            // asset の永続化
            foreach (var (src, with, without) in list)
            {
                SaveMesh(src, with, BlendShapeLogic.WithBlendShape);
                SaveMesh(src, without, BlendShapeLogic.WithoutBlendShape);
            }
            return true;
        }

        private static void SaveMesh(Mesh mesh, Mesh newMesh, BlendShapeLogic blendShapeLabel)
        {
            // save mesh as asset
            var assetPath = string.Format("{0}{1}", Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            Debug.Log(assetPath);
            if (!string.IsNullOrEmpty((AssetDatabase.GetAssetPath(mesh))))
            {
                var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh)).Replace("\\", "/");
                assetPath = string.Format("{0}/{1}{2}", directory, Path.GetFileNameWithoutExtension(mesh.name) + "_" + blendShapeLabel.ToString(), ASSET_SUFFIX);
            }
            else
            {
                assetPath = string.Format("Assets/{0}{1}", Path.GetFileNameWithoutExtension(mesh.name) + "_" + blendShapeLabel.ToString(), ASSET_SUFFIX);
            }
            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(newMesh, assetPath);
        }

        /// <summary>
        /// 対象のヒエラルキーに含まれるすべての SkinnedMeshRenderer に対して、
        /// BlendShape を含む Mesh と 含まない Mesh への分割を実施する。
        /// 
        /// 各 SkinnedMeshRenderer(smr) は、
        /// 
        /// smr - mesh(with blendshape)
        ///  + smr(without) - mesh(without blendshape)
        /// 
        /// のように変化する。 
        /// </summary>
        /// <param name="go"></param>
        /// <return>(Mesh 分割前, Mesh BlendShape有り、Mesh BlendShape無し)のリストを返す</return>
        public static List<(Mesh Src, Mesh With, Mesh Without)> SeparationProcessing(GameObject go)
        {
            var list = new List<(Mesh Src, Mesh With, Mesh Without)>();
            var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    var (mesh, with, without) = SeparatePolyWithBlendShape(skinnedMeshRenderer);
                    if (mesh != null)
                    {
                        list.Add((mesh, with, without));
                    }
                }
            }
            return list;
        }

        private static (Mesh mesh, Mesh With, Mesh Without) SeparatePolyWithBlendShape(SkinnedMeshRenderer skinnedMeshRendererInput)
        {
            var indicesUsedByBlendShape = new Dictionary<int, int>();
            var mesh = skinnedMeshRendererInput.sharedMesh;

            // retrieve the original BlendShape data
            for (int i = 0; i < mesh.blendShapeCount; ++i)
            {
                var deltaVertices = new Vector3[mesh.vertexCount];
                var deltaNormals = new Vector3[mesh.vertexCount];
                var deltaTangents = new Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, deltaNormals, deltaTangents);

                for (int j = 0; j < deltaVertices.Length; j++)
                {
                    if (!deltaVertices[j].Equals(Vector3.zero))
                    {
                        if (!indicesUsedByBlendShape.Values.Contains(j))
                        {
                            indicesUsedByBlendShape.Add(indicesUsedByBlendShape.Count, j);
                        }
                    }
                }
            }

            var subMeshCount = mesh.subMeshCount;
            var submeshesWithBlendShape = new Dictionary<int, int[]>();
            var submeshesWithoutBlendShape = new Dictionary<int, int[]>();
            var vertexIndexWithBlendShape = new Dictionary<int, int>();
            var vertexCounterWithBlendShape = 0;
            var vertexIndexWithoutBlendShape = new Dictionary<int, int>();
            var vertexCounterWithoutBlendShape = 0;

            // check blendshape's vertex index from submesh
            for (int i = 0; i < subMeshCount; i++)
            {
                var triangle = mesh.GetTriangles(i);
                var submeshWithBlendShape = new List<int>();
                var submeshWithoutBlendShape = new List<int>();

                for (int j = 0; j < triangle.Length; j += 3)
                {
                    if (indicesUsedByBlendShape.Values.Contains(triangle[j]) ||
                        indicesUsedByBlendShape.Values.Contains(triangle[j + 1]) ||
                        indicesUsedByBlendShape.Values.Contains(triangle[j + 2]))
                    {
                        BuildNewTriangleList(vertexIndexWithBlendShape, triangle, j, submeshWithBlendShape, ref vertexCounterWithBlendShape);
                    }
                    else
                    {
                        BuildNewTriangleList(vertexIndexWithoutBlendShape, triangle, j, submeshWithoutBlendShape, ref vertexCounterWithoutBlendShape);
                    }
                }
                if (submeshWithBlendShape.Count > 0)
                    submeshesWithBlendShape.Add(i, submeshWithBlendShape.ToArray());
                if (submeshWithoutBlendShape.Count > 0)
                    submeshesWithoutBlendShape.Add(i, submeshWithoutBlendShape.ToArray()); ;
            }

            // check if any BlendShape exists
            if (submeshesWithoutBlendShape.Count > 0)
            {
                // put the mesh without BlendShape in a new SkinnedMeshRenderer
                var srcGameObject = skinnedMeshRendererInput.gameObject;
                var srcTransform = skinnedMeshRendererInput.transform.parent;
                var targetObjectForMeshWithoutBS = GameObject.Instantiate(srcGameObject);
                targetObjectForMeshWithoutBS.name = srcGameObject.name + "_WithoutBlendShape";
                targetObjectForMeshWithoutBS.transform.SetParent(srcTransform);
                var skinnedMeshRendererWithoutBS = targetObjectForMeshWithoutBS.GetComponent<SkinnedMeshRenderer>();

                // build meshes with/without BlendShape
                var with = BuildNewMesh(skinnedMeshRendererInput, vertexIndexWithBlendShape, submeshesWithBlendShape, BlendShapeLogic.WithBlendShape);
                var without = BuildNewMesh(skinnedMeshRendererWithoutBS, vertexIndexWithoutBlendShape, submeshesWithoutBlendShape, BlendShapeLogic.WithoutBlendShape);
                return (mesh, with, without);
            }
            else
            {
                return default;
            }
        }

        private static void BuildNewTriangleList(Dictionary<int, int> newVerticesListLookUp, int[] triangleList, int index,
                                                 List<int> newTriangleList, ref int vertexCounter)
        {
            // build new vertex list and triangle list
            // vertex 1
            if (!newVerticesListLookUp.Keys.Contains(triangleList[index]))
            {
                newVerticesListLookUp.Add(triangleList[index], vertexCounter);
                newTriangleList.Add(vertexCounter);
                vertexCounter++;
            }
            else
            {
                var newVertexIndex = newVerticesListLookUp[triangleList[index]];
                newTriangleList.Add(newVertexIndex);
            }
            // vertex 2
            if (!newVerticesListLookUp.Keys.Contains(triangleList[index + 1]))
            {
                newVerticesListLookUp.Add(triangleList[index + 1], vertexCounter);
                newTriangleList.Add(vertexCounter);
                vertexCounter++;
            }
            else
            {
                var newVertexIndex = newVerticesListLookUp[triangleList[index + 1]];
                newTriangleList.Add(newVertexIndex);
            }
            // vertex 3
            if (!newVerticesListLookUp.Keys.Contains(triangleList[index + 2]))
            {
                newVerticesListLookUp.Add(triangleList[index + 2], vertexCounter);
                newTriangleList.Add(vertexCounter);
                vertexCounter++;
            }
            else
            {
                var newVertexIndex = newVerticesListLookUp[triangleList[index + 2]];
                newTriangleList.Add(newVertexIndex);
            }
        }

        private static Mesh BuildNewMesh(SkinnedMeshRenderer skinnedMeshRenderer, Dictionary<int, int> newIndexLookUpDict,
                                         Dictionary<int, int[]> subMeshes, BlendShapeLogic blendShapeLabel)
        {
            // get original mesh data
            var materialList = new List<Material>();
            skinnedMeshRenderer.GetSharedMaterials(materialList);
            var mesh = skinnedMeshRenderer.sharedMesh;
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshTangents = mesh.tangents;
            var meshColors = mesh.colors;
            var meshBoneWeights = mesh.boneWeights;
            var meshUVs = mesh.uv;

            // build new mesh
            var materialListNew = new List<Material>();
            var newMesh = new Mesh();

            if (mesh.vertexCount > ushort.MaxValue)
            {
#if UNITY_2017_3_OR_NEWER
                Debug.LogFormat("exceed 65535 vertices: {0}", mesh.vertexCount);
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                throw new NotImplementedException(String.Format("exceed 65535 vertices: {0}", integrator.Positions.Count.ToString()));
#endif
            }

            var newDataLength = newIndexLookUpDict.Count;
            var newIndexLookUp = newIndexLookUpDict.Keys.ToArray();

            newMesh.vertices = newIndexLookUp.Select(x => meshVertices[x]).ToArray();
            if (meshNormals.Length > 0) newMesh.normals = newIndexLookUp.Select(x => meshNormals[x]).ToArray();
            if (meshTangents.Length > 0) newMesh.tangents = newIndexLookUp.Select(x => meshTangents[x]).ToArray();
            if (meshColors.Length > 0) newMesh.colors = newIndexLookUp.Select(x => meshColors[x]).ToArray();
            if (meshBoneWeights.Length > 0) newMesh.boneWeights = newIndexLookUp.Select(x => meshBoneWeights[x]).ToArray();
            if (meshUVs.Length > 0) newMesh.uv = newIndexLookUp.Select(x => meshUVs[x]).ToArray();
            newMesh.bindposes = mesh.bindposes;

            // add BlendShape data
            if (blendShapeLabel == BlendShapeLogic.WithBlendShape)
            {
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    // get original BlendShape data
                    var srcVertices = new Vector3[mesh.vertexCount];
                    var srcNormals = new Vector3[mesh.vertexCount];
                    var srcTangents = new Vector3[mesh.vertexCount];
                    mesh.GetBlendShapeFrameVertices(i, 0, srcVertices, srcNormals, srcTangents);

                    // declare the size for the destination array
                    var dstVertices = new Vector3[newDataLength];
                    var dstNormals = new Vector3[newDataLength];
                    var dstTangents = new Vector3[newDataLength];

                    dstVertices = newIndexLookUp.Select(x => srcVertices[x]).ToArray();
                    dstNormals = newIndexLookUp.Select(x => srcNormals[x]).ToArray();
                    dstTangents = newIndexLookUp.Select(x => srcTangents[x]).ToArray();
                    newMesh.AddBlendShapeFrame(mesh.GetBlendShapeName(i), mesh.GetBlendShapeFrameWeight(i, 0),
                                               dstVertices, dstNormals, dstTangents);
                }
            }

            newMesh.subMeshCount = subMeshes.Count;
            var cosMaterialIndex = subMeshes.Keys.ToArray();

            // build material list
            for (int i = 0; i < subMeshes.Count; i++)
            {
                newMesh.SetTriangles(subMeshes[cosMaterialIndex[i]], i);
                materialListNew.Add(materialList[cosMaterialIndex[i]]);
            }
            skinnedMeshRenderer.sharedMaterials = materialListNew.ToArray();
            skinnedMeshRenderer.sharedMesh = newMesh;

            return newMesh;
        }
    }
}

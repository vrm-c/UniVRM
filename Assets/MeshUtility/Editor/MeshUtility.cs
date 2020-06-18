using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MeshUtility
{
    public class MeshUtility : MonoBehaviour
    {
        const string ASSET_SUFFIX = ".mesh.asset";

        private enum BlendShapeLogic
        {
            WithBlendShape,
            WithoutBlendShape,
        }

        [MenuItem("Mesh Utility/Extract BlendShape Mesh")]
        static void CreateGameObjectWithBlendShapeMeshSeparate()
        {
            var go = Selection.activeTransform.gameObject;

            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0)
            {
                ExtractBlendShapeMeshFromSkinnedMesh(go);
                go.SetActive(false);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No skinnedMeshRenderer contained", "ok");
            }
        }

        private static void ExtractBlendShapeMeshFromSkinnedMesh(GameObject go)
        {
            var outputObject = Instantiate(go);
            var skinnedMeshRenderers = outputObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    BlendShapeDivide(skinnedMeshRenderer);
                }
            }
        }

        private static void BlendShapeDivide(SkinnedMeshRenderer skinnedMeshRendererInput)
        {
            var indicesUsedByBlendShape = new Dictionary<int, int>();
            var mesh = skinnedMeshRendererInput.sharedMesh;
            var zeroMovement = new Vector3(0, 0, 0);

            // retrieve the original BlendShape data
            for (int i = 0; i < mesh.blendShapeCount; ++i)
            {
                var deltaVertices = new Vector3[mesh.vertexCount];
                var deltaNormals = new Vector3[mesh.vertexCount];
                var deltaTangents = new Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, deltaNormals, deltaTangents);

                for (int j = 0; j < deltaVertices.Length; j++)
                {
                    if (!deltaVertices[j].Equals(zeroMovement))
                    {
                        if (!indicesUsedByBlendShape.Values.Contains(j))
                        {
                            indicesUsedByBlendShape.Add(indicesUsedByBlendShape.Count, j);
                        }
                    }
                }
            }

            var subMeshCount = mesh.subMeshCount;
            var submeshesWithoutBlendShape = new int[subMeshCount][];
            var submeshesWithBlendShape = new int[subMeshCount][];
            var vertexIndexWithBlendShape = new Dictionary<int, int>();
            var vertexCounterWithBlendShape = 0;
            var vertexIndexWithoutBlendShape = new Dictionary<int, int>();
            var vertexCounterWithoutBlendShape = 0;

            // check blendshape's vertex index from submesh
            for (int i = 0; i < subMeshCount; i++)
            {
                var triangle = mesh.GetTriangles(i);
                var submeshWithBlendShape = new List<int>();
                var meshWithoutBlendShape = new List<int>();

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
                        BuildNewTriangleList(vertexIndexWithoutBlendShape, triangle, j, meshWithoutBlendShape, ref vertexCounterWithoutBlendShape);
                    }
                }
                submeshesWithBlendShape[i] = submeshWithBlendShape.ToArray();
                submeshesWithoutBlendShape[i] = meshWithoutBlendShape.ToArray();
            }

            // check if any BlendShape exists
            if (submeshesWithoutBlendShape.Any(x => x.Length != 0))
            {
                // put the mesh without BlendShape in a new SkinnedMeshRenderer
                var srcGameObject = skinnedMeshRendererInput.gameObject;
                var srcTransform = skinnedMeshRendererInput.transform.parent;
                var targetObjectForMeshWithoutBS = Instantiate(srcGameObject);
                targetObjectForMeshWithoutBS.name = srcGameObject.name + "_WithoutBlendShape";
                targetObjectForMeshWithoutBS.transform.SetParent(srcTransform);
                var skinnedMeshRendererWithoutBS = targetObjectForMeshWithoutBS.GetComponent<SkinnedMeshRenderer>();

                // build meshes with/without BlendShape
                BuildNewMesh(skinnedMeshRendererInput, vertexIndexWithBlendShape, submeshesWithBlendShape, BlendShapeLogic.WithBlendShape);
                BuildNewMesh(skinnedMeshRendererWithoutBS, vertexIndexWithoutBlendShape, submeshesWithoutBlendShape, BlendShapeLogic.WithoutBlendShape);
            }
        }

        private static void BuildNewTriangleList(Dictionary<int, int> newVerticesListLookUp, int[] triangleList, int index,
                                                 List<int> newTriangleList, ref int lookupCounter)
        {
            // build new vertex list and triangle list
            // vertex 1
            if (!newVerticesListLookUp.Keys.Contains(triangleList[index]))
            {
                newVerticesListLookUp.Add(triangleList[index], lookupCounter);
                newTriangleList.Add(lookupCounter);
                lookupCounter++;
            }
            else
            {
                var newVertexIndex = newVerticesListLookUp[triangleList[index]];
                newTriangleList.Add(newVertexIndex);
            }
            // vertex 2
            if (!newVerticesListLookUp.Keys.Contains(triangleList[index + 1]))
            {
                newVerticesListLookUp.Add(triangleList[index + 1], lookupCounter);
                newTriangleList.Add(lookupCounter);
                lookupCounter++;
            }
            else
            {
                var newVertexIndex = newVerticesListLookUp[triangleList[index + 1]];
                newTriangleList.Add(newVertexIndex);
            }
            // vertex 3
            if (!newVerticesListLookUp.Keys.Contains(triangleList[index + 2]))
            {
                newVerticesListLookUp.Add(triangleList[index + 2], lookupCounter);
                newTriangleList.Add(lookupCounter);
                lookupCounter++;
            }
            else
            {
                var newVertexIndex = newVerticesListLookUp[triangleList[index + 2]];
                newTriangleList.Add(newVertexIndex);
            }
        }

        private static void BuildNewMesh(SkinnedMeshRenderer skinnedMeshRenderer, Dictionary<int, int> newIndexLookUpDict,
                                         int[][] subMeshes, BlendShapeLogic blendShapeLabel)
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
            newMesh.normals = newIndexLookUp.Select(x => meshNormals[x]).ToArray();
            if (meshTangents.Length > 0) newMesh.tangents = newIndexLookUp.Select(x => meshTangents[x]).ToArray();
            if (meshColors.Length > 0) newMesh.colors = newIndexLookUp.Select(x => meshColors[x]).ToArray();
            newMesh.boneWeights = newIndexLookUp.Select(x => meshBoneWeights[x]).ToArray();
            newMesh.uv = newIndexLookUp.Select(x => meshUVs[x]).ToArray();
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

            // build material list
            var subMeshCounter = 0;
            for (int i = 0; i < subMeshes.Length; i++)
            {
                if (subMeshes[i].Length > 0)
                {
                    newMesh.SetTriangles(subMeshes[i], subMeshCounter);
                    materialListNew.Add(materialList[i]);
                    subMeshCounter++;
                    newMesh.subMeshCount++;
                }
            }

            newMesh.subMeshCount = subMeshCounter;
            skinnedMeshRenderer.sharedMaterials = materialListNew.ToArray();
            skinnedMeshRenderer.sharedMesh = newMesh;

            // save mesh as asset
            var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh)).Replace("\\", "/");
            var assetPath = directory + "/" + Path.GetFileNameWithoutExtension(mesh.name) + "_" + blendShapeLabel.ToString() + ASSET_SUFFIX;
            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(newMesh, assetPath);
        }
    }
}
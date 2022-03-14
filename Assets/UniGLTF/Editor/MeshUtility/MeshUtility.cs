using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UniGLTF.MeshUtility
{
    public class MeshUtility
    {
        public const string ASSET_SUFFIX = ".mesh.asset";
        private static readonly Vector3 ZERO_MOVEMENT = Vector3.zero;

        public static Object GetPrefab(GameObject instance)
        {
#if UNITY_2018_2_OR_NEWER
            return PrefabUtility.GetCorrespondingObjectFromSource(instance);
#else
            return PrefabUtility.GetPrefabParent(go);
#endif
        }

        public enum BlendShapeLogic
        {
            WithBlendShape,
            WithoutBlendShape,
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
                    if (!deltaVertices[j].Equals(ZERO_MOVEMENT))
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

#if UNITY_EDITOR
        public static bool IsGameObjectSelected()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        public static void IntegrateSelected(GameObject go)
        {
            var meshWithMaterials = StaticMeshIntegrator.Integrate(go.transform);

            // save as asset
            var assetPath = "";
#if UNITY_2018_2_OR_NEWER
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
            var prefab = PrefabUtility.GetPrefabParent(go);
#endif
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}_{2}{3}",
                     Path.GetDirectoryName(prefabPath),
                     Path.GetFileNameWithoutExtension(prefabPath),
                    go.name,
                     ASSET_SUFFIX
                     );
            }
            else
            {
                var path = EditorUtility.SaveFilePanel(
                        "Save mesh",
                        "Assets",
                        go.name + ".asset",
                        "asset");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                assetPath = UnityPath.FromFullpath(path).Value;
            }

            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(meshWithMaterials.Mesh, assetPath);

            // add component
            var meshObject = new GameObject(go.name + ".static_meshes_integrated");
            if (go.transform.parent != null)
            {
                meshObject.transform.SetParent(go.transform.parent, false);
            }
            meshObject.transform.localPosition = go.transform.localPosition;
            meshObject.transform.localRotation = go.transform.localRotation;
            meshObject.transform.localScale = go.transform.localScale;

            var filter = meshObject.AddComponent<MeshFilter>();
            filter.sharedMesh = meshWithMaterials.Mesh;
            var renderer = meshObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = meshWithMaterials.Materials;
        }
#endif
        public static void MeshIntegrator(GameObject go)
        {
            MeshIntegratorUtility.Integrate(go, onlyBlendShapeRenderers: true);
            MeshIntegratorUtility.Integrate(go, onlyBlendShapeRenderers: false);

            var outputObject = GameObject.Instantiate(go);
            outputObject.name = outputObject.name + "_mesh_integration";
            var skinnedMeshes = outputObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            var normalMeshes = outputObject.GetComponentsInChildren<MeshFilter>();

            // destroy integrated meshes in the source
            foreach (var skinnedMesh in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_NAME ||
                    skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_BLENDSHAPE_NAME)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
            }
            foreach (var skinnedMesh in skinnedMeshes)
            {
                // destroy original meshes in the copied GameObject
                if (!(skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_NAME ||
                    skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_BLENDSHAPE_NAME))
                {
                    GameObject.DestroyImmediate(skinnedMesh);
                }
                // check if the integrated mesh is empty
                else if (skinnedMesh.sharedMesh.subMeshCount == 0)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
                // save mesh data
                else if (skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_NAME ||
                         skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_BLENDSHAPE_NAME)
                {
                    SaveMeshData(skinnedMesh.sharedMesh);
                }
            }
            foreach (var normalMesh in normalMeshes)
            {
                if (normalMesh.sharedMesh.name != MeshIntegratorUtility.INTEGRATED_MESH_NAME)
                {
                    if (normalMesh.gameObject.GetComponent<MeshRenderer>())
                    {
                        GameObject.DestroyImmediate(normalMesh.gameObject.GetComponent<MeshRenderer>());
                    }
                    GameObject.DestroyImmediate(normalMesh);
                }
            }
        }

        public static void SaveMesh(Mesh mesh, Mesh newMesh, BlendShapeLogic blendShapeLabel)
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

        private static void SaveMeshData(Mesh mesh)
        {
            var assetPath = string.Format("{0}{1}", Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            Debug.Log(assetPath);
            if (!string.IsNullOrEmpty((AssetDatabase.GetAssetPath(mesh))))
            {
                var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh)).Replace("\\", "/");
                assetPath = string.Format("{0}/{1}{2}", directory, Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            }
            else
            {
                assetPath = string.Format("Assets/{0}{1}", Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            }
            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(mesh, assetPath);
        }
    }
}
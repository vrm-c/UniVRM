using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UniGLTF;


namespace VRM
{
    /// <summary>
    /// プレビュー向けのシーンを管理する
    /// </summary>
    public class PreviewSceneManager : MonoBehaviour
    {
        public GameObject Prefab;

#if UNITY_EDITOR
        public static PreviewSceneManager GetOrCreate(GameObject prefab, BlendShapeBinding[] values)
        {
            if (prefab == null)
            {
                return null;
            }

            PreviewSceneManager manager = null;

            // if we already instantiated a PreviewInstance previously but just lost the reference, then use that same instance instead of making a new one
            var managers = GameObject.FindObjectsOfType<PreviewSceneManager>();
            foreach (var x in managers)
            {
                if (x.Prefab == prefab)
                {
                    Debug.LogFormat("find {0}", manager);
                    return manager;
                }
                Debug.LogFormat("destroy {0}", x);
                GameObject.DestroyImmediate(x.gameObject);
            }

            // no previous instance detected, so now let's make a fresh one
            // very important: this loads the PreviewInstance prefab and temporarily instantiates it into PreviewInstance
            var go = GameObject.Instantiate(prefab,
                prefab.transform.position,
                prefab.transform.rotation
                );
            go.name = "__PREVIEW_SCENE_MANGER__";
            manager = go.AddComponent<PreviewSceneManager>();
            manager.Initialize(prefab, values);

            // HideFlags are special editor-only settings that let you have *secret* GameObjects in a scene, or to tell Unity not to save that temporary GameObject as part of the scene
            go.hideFlags |= HideFlags.DontSaveInEditor;
            go.hideFlags |= HideFlags.DontSaveInBuild;
#if VRM_DEVELOP
#else
            go.hideFlags |= HideFlags.HideAndDontSave;
            ; // you could also hide it from the hierarchy or inspector, but personally I like knowing everything that's there
#endif
            //Debug.LogFormat("Create {0}", manager);

            return manager;
        }
#endif

        private void Initialize(GameObject prefab, BlendShapeBinding[] values)
        {
            Prefab = prefab;

            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
            //PreviewLayer = (int)propInfo.GetValue(null, new object[0]);

            /*
            foreach (var x in transform.Traverse())
            {
                x.gameObject.layer = PreviewLayer;
            }
            */

            m_meshes = transform.Traverse()
                .Select(x => MeshPreviewItem.Create(x, transform))
                .Where(x => x != null)
                .ToArray()
                ;

            foreach(var x in m_meshes)
            {
                if (x.SkinnedMeshRenderer != null)
                {
                    var mesh = x.SkinnedMeshRenderer.sharedMesh;
                    m_blendShapeNameMap.Add(m_blendShapeNameMap.Count, 
                        Enumerable.Range(0, mesh.blendShapeCount).Select(y => mesh.GetBlendShapeName(y)).ToArray());
                }
            }

            Bake(values);

            m_rendererPathList = m_meshes.Select(x => x.Path).ToArray();
            m_skinnedMeshRendererPathList = m_meshes
                .Where(x => x.SkinnedMeshRenderer != null)
                .Select(x => x.Path)
                .ToArray();
        }

        MeshPreviewItem[] m_meshes;
        public IEnumerable<MeshPreviewItem> EnumRenderItems
        {
            get
            {
                if (m_meshes != null)
                {
                    foreach (var x in m_meshes)
                    {
                        yield return x;
                    }
                }
            }
        }

        string[] m_rendererPathList;
        public string[] RendererPathList
        {
            get { return m_rendererPathList; }
        }

        string[] m_skinnedMeshRendererPathList;
        public string[] SkinnedMeshRendererPathList
        {
            get { return m_skinnedMeshRendererPathList; }
        }

        Dictionary<int, string[]> m_blendShapeNameMap = new Dictionary<int, string[]>();
        public string[] GetBlendShapeNames(int skinnedMeshIndex)
        {
            string[] names = null;
            if(m_blendShapeNameMap.TryGetValue(skinnedMeshIndex, out names))
            {
                return names;
            }
            return null;
        }

        public string[] GetMaterialNames(int rendererIndex)
        {
            if (rendererIndex >= 0 && rendererIndex < m_meshes.Length)
            {
                return m_meshes[rendererIndex].MaterialsNames;
            }
            return null;
        }

        Bounds m_bounds;
        public void Bake(BlendShapeBinding[] values)
        {
            //Debug.LogFormat("Bake");
            m_bounds = default(Bounds);
            if (m_meshes != null)
            {
                foreach (var x in m_meshes)
                {
                    x.Bake(values);
                    m_bounds.Expand(x.Mesh.bounds.size);
                }
            }
        }

        /*
        int PreviewLayer
        {
            get;
            set;
        }
        */

        /// <summary>
        /// カメラパラメーターを決める
        /// </summary>
        /// <param name="camera"></param>
        public void SetupCamera(Camera camera)
        {
            float magnitude = m_bounds.extents.magnitude * 0.5f;
            float distance = magnitude;
            camera.fieldOfView = 27f;
            camera.backgroundColor = Color.gray;
            camera.clearFlags = CameraClearFlags.Color;
            // this used to be "-Vector3.forward * num" but I hardcoded my camera position instead
            camera.transform.position = new Vector3(0f, 1.4f, distance);
            camera.transform.rotation = Quaternion.Euler(0, 180f, 0);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = distance + magnitude * 1.1f;

            //previewLayer のみ表示する
            //camera.cullingMask = 1 << PreviewLayer;
        }
    }
}

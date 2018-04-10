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
        public static PreviewSceneManager GetOrCreate(GameObject prefab)
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
            manager.Initialize(prefab);

            // HideFlags are special editor-only settings that let you have *secret* GameObjects in a scene, or to tell Unity not to save that temporary GameObject as part of the scene
            foreach (var x in go.transform.Traverse())
            {
                x.gameObject.hideFlags = HideFlags.None
                | HideFlags.DontSave
                //| HideFlags.DontSaveInBuild
#if VRM_DEVELOP
#else
                | HideFlags.HideAndDontSave
#endif
                ;
            }

            return manager;
        }
#endif

        public void Clean()
        {
            foreach (var kv in m_materialMap)
            {
                UnityEngine.Object.DestroyImmediate(kv.Value.Material);
            }
        }

        private void Initialize(GameObject prefab)
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

            var materialNames = new List<string>();
            var map = new Dictionary<Material, Material>();
            Func<Material, Material> getOrCreateMaterial = src =>
            {
                if (src == null) return null;
                if (string.IsNullOrEmpty(src.name)) return null; // !

                Material dst;
                if(!map.TryGetValue(src, out dst))
                {
                    dst = new Material(src);
                    map.Add(src, dst);

                    materialNames.Add(src.name);
                    m_materialMap.Add(src.name, MaterialItem.Create(dst));
                }
                return dst;
            };

            m_meshes = transform.Traverse()
                .Select(x => MeshPreviewItem.Create(x, transform, getOrCreateMaterial))
                .Where(x => x != null)
                .ToArray()
                ;
            MaterialNames = materialNames.ToArray();

            m_blendShapeMeshes = m_meshes
                .Where(x => x.SkinnedMeshRenderer != null
                && x.SkinnedMeshRenderer.sharedMesh.blendShapeCount>0)
                .ToArray();

            //Bake(values, materialValues);

            m_rendererPathList = m_meshes.Select(x => x.Path).ToArray();
            m_skinnedMeshRendererPathList = m_meshes
                .Where(x => x.SkinnedMeshRenderer != null)
                .Select(x => x.Path)
                .ToArray();

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                var head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null)
                {
                    m_target = head;
                }
            }
        }

        MeshPreviewItem[] m_meshes;
        MeshPreviewItem[] m_blendShapeMeshes;
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

        public string[] MaterialNames
        {
            get;
            private set;
        }

        Dictionary<string, MaterialItem> m_materialMap = new Dictionary<string, MaterialItem>();

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

        public string[] GetBlendShapeNames(int blendShapeMeshIndex)
        {
            if (blendShapeMeshIndex >= 0 && blendShapeMeshIndex < m_blendShapeMeshes.Length)
            {
                var item = m_blendShapeMeshes[blendShapeMeshIndex];
                return item.BlendShapeNames;
            }

            return null;
        }

        public MaterialItem GetMaterialItem(string materialName)
        {
            MaterialItem item;
            if(!m_materialMap.TryGetValue(materialName, out item)){
                return null;
            }

            return item;
        }

        public Transform m_target;
        public Vector3 TargetPosition
        {
            get
            {
                if (m_target == null)
                {
                    return new Vector3(0, 1.4f, 0);
                }
                return m_target.position + new Vector3(0, 0.1f, 0);
            }
        }

#if UNITY_EDITOR
        Bounds m_bounds;
        public void Bake(BlendShapeBinding[] values=null, MaterialValueBinding[] materialValues=null, float weight=1.0f)
        {
            //Debug.LogFormat("Bake");
            m_bounds = default(Bounds);
            if (m_meshes != null)
            {
                foreach (var x in m_meshes)
                {
                    x.Bake(values, weight);
                    m_bounds.Expand(x.Mesh.bounds.size);
                }
            }

            // Udpate Material
            if (materialValues != null && m_materialMap != null)
            {
                // clear
                //Debug.LogFormat("clear material");
                foreach (var kv in m_materialMap)
                {
                    foreach (var _kv in kv.Value.PropMap)
                    {
                        kv.Value.Material.SetColor(_kv.Key, _kv.Value.DefaultValues);
                    }
                }

                foreach (var x in materialValues)
                {
                    MaterialItem item;
                    if (m_materialMap.TryGetValue(x.MaterialName, out item))
                    {
                        //Debug.Log("set material");
                        PropItem prop;
                        if (item.PropMap.TryGetValue(x.ValueName, out prop))
                        {
                            var value = x.BaseValue + (x.TargetValue - x.BaseValue) * weight;
                            item.Material.SetColor(x.ValueName, value);
                        }
                    }
                }
            }

        }
#endif
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
        public void SetupCamera(Camera camera, Vector3 target, float yaw, float pitch, float distance)
        {
            camera.backgroundColor = Color.gray;
            camera.clearFlags = CameraClearFlags.Color;

            // projection
            //float magnitude = m_bounds.extents.magnitude * 0.5f;
            //float distance = magnitude;
            //var distance = target.magnitude;

            camera.fieldOfView = 27f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = distance /*+ magnitude*/ * 2.1f;

#if false
            // this used to be "-Vector3.forward * num" but I hardcoded my camera position instead
            camera.transform.position = new Vector3(0f, 1.4f, distance);
            camera.transform.rotation = Quaternion.Euler(0, 180f, 0);
#else
            camera.transform.position = target + Quaternion.Euler(pitch, yaw, 0) * Vector3.forward * distance;
            camera.transform.LookAt(target);
#endif

            //previewLayer のみ表示する
            //camera.cullingMask = 1 << PreviewLayer;
        }
    }
}

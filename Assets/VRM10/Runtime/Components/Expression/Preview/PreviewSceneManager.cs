using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UniGLTF;


namespace UniVRM10
{
    /// <summary>
    /// プレビュー向けのシーンを管理する
    /// </summary>
    public sealed class PreviewSceneManager : MonoBehaviour
    {
        public GameObject Prefab;

        public bool hasError;

#if UNITY_EDITOR
        public static PreviewSceneManager GetOrCreate(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            PreviewSceneManager manager = null;

            // if we already instantiated a PreviewInstance previously but just lost the reference, then use that same instance instead of making a new one
#if UNITY_2022_3_OR_NEWER
            var managers = GameObject.FindObjectsByType<PreviewSceneManager>(FindObjectsSortMode.InstanceID);
#else
            var managers = GameObject.FindObjectsOfType<PreviewSceneManager>();
#endif
            foreach (var x in managers)
            {
                if (x.Prefab == prefab)
                {
                    UniGLTFLogger.Log($"find {manager}");
                    return manager;
                }
                UniGLTFLogger.Log($"destroy {x}");
                GameObject.DestroyImmediate(x.gameObject);
            }

            //Debug.Log("new prefab. instanciate");
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
                if (Symbols.VRM_DEVELOP)
                {
                    x.gameObject.hideFlags = HideFlags.None |
                                             HideFlags.DontSave;
                }
                else
                {
                    x.gameObject.hideFlags = HideFlags.None |
                                             HideFlags.DontSave |
                                             HideFlags.HideAndDontSave;
                }
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

#if UNITY_EDITOR
        private void Initialize(GameObject prefab)
        {
            hasError = false;
            
            Prefab = prefab;

            var materialNames = new List<string>();

            // preview シーン用に Material を複製する。
            // Expression のカスタムエディタのマテリアル変更は、
            // 複製したものに適用される。
            var map = new Dictionary<Material, Material>();
            Func<Material, Material> getOrCreateMaterial = src =>
            {
                if (src == null) return null;
                if (string.IsNullOrEmpty(src.name)) return null; // !

                Material dst;
                if (!map.TryGetValue(src, out dst))
                {
                    dst = new Material(src);
                    map.Add(src, dst);
                    
                    if (!PreviewMaterialUtil.TryCreateForPreview(dst, out var previewMaterialItem))
                    {
                        hasError = true;
                        // Return cloned material for preview
                        return dst;
                    }
                    
                    m_materialMap.Add(src.name, previewMaterialItem);

                    materialNames.Add(src.name);
                }
                return dst;
            };

            m_meshes = transform.Traverse()
                .Select(x => PreviewMeshItem.Create(x, transform, getOrCreateMaterial))
                .Where(x => x != null)
                .ToArray()
                ;
            MaterialNames = materialNames.ToArray();

            m_blendShapeMeshes = m_meshes
                .Where(x => x.SkinnedMeshRenderer != null
                && x.SkinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                .ToArray();

            m_rendererPathList = m_meshes.Select(x => x.Path).ToArray();
            m_skinnedMeshRendererPathList = m_meshes
                .Where(x => x.SkinnedMeshRenderer != null)
                .Select(x => x.Path)
                .ToArray();

            if(TryGetComponent<Animator>(out var animator))
            {
                var head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null)
                {
                    m_target = head;
                }
            }
        }
#endif

        PreviewMeshItem[] m_meshes;
        PreviewMeshItem[] m_blendShapeMeshes;
        public IEnumerable<PreviewMeshItem> EnumRenderItems
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

        Dictionary<string, PreviewMaterialItem> m_materialMap = new Dictionary<string, PreviewMaterialItem>();

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

        public PreviewMaterialItem GetMaterialItem(string materialName)
        {
            PreviewMaterialItem item;
            if (!m_materialMap.TryGetValue(materialName, out item))
            {
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
        public void Bake(VRM10Expression bake, float weight)
        {
            if (bake == null)
            {
                return;
            }

            //
            // Bake Expression
            //
            m_bounds = default(Bounds);
            if (m_meshes != null)
            {
                if (bake != null)
                {
                    foreach (var x in m_meshes)
                    {
                        x.Bake(bake.MorphTargetBindings, weight);
                        m_bounds.Expand(x.Mesh.bounds.size);
                    }
                }
            }

            //
            // Update Material
            //
            if (m_materialMap != null)
            {
                // clear
                foreach (var kv in m_materialMap)
                {
                    kv.Value.Clear();
                }

                // color
                if (bake.MaterialColorBindings != null)
                {
                    foreach (var x in bake.MaterialColorBindings)
                    {
                        PreviewMaterialItem item;
                        if (m_materialMap.TryGetValue(x.MaterialName, out item))
                        {
                            //Debug.Log("set material");
                            PropItem prop;
                            if (item.PropMap.TryGetValue(x.BindType, out prop))
                            {
                                // var valueName = x.ValueName;
                                // if (valueName.EndsWith("_ST_S")
                                // || valueName.EndsWith("_ST_T"))
                                // {
                                //     valueName = valueName.Substring(0, valueName.Length - 2);
                                // }

                                var value = item.Material.GetVector(prop.Name);
                                value += ((x.TargetValue - prop.DefaultValues) * weight);
                                item.Material.SetColor(prop.Name, value);
                            }
                        }
                    }
                }

                // uv
                if (bake.MaterialUVBindings != null)
                {
                    foreach (var x in bake.MaterialUVBindings)
                    {
                        PreviewMaterialItem item;
                        if (m_materialMap.TryGetValue(x.MaterialName, out item))
                        {
                            item.AddScaleOffset(x.ScalingOffset, weight);
                        }
                    }
                }
            }
        }
#endif

        /// <summary>
        /// カメラパラメーターを決める
        /// </summary>
        /// <param name="camera"></param>
        public void SetupCamera(Camera camera, Vector3 target, float yaw, float pitch, Vector3 position)
        {
            camera.backgroundColor = Color.gray;
            camera.clearFlags = CameraClearFlags.Color;

            // projection
            //float magnitude = m_bounds.extents.magnitude * 0.5f;
            //float distance = magnitude;
            //var distance = target.magnitude;

            camera.fieldOfView = 27f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = -position.z /*+ magnitude*/ * 2.1f;

            var t = Matrix4x4.Translate(position);
            var r = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(pitch, yaw, 0), Vector3.one);
            // 回転してから移動
            var m = r * t;

            camera.transform.position = target + m.ExtractPosition();
            camera.transform.rotation = m.ExtractRotation();
            //camera.transform.LookAt(target);

            //previewLayer のみ表示する
            //camera.cullingMask = 1 << PreviewLayer;
        }
    }
}

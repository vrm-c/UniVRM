using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    [CustomPropertyDrawer(typeof(BoneMeshEraser.EraseBone))]
    public class EraseBoneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //EditorGUI.BeginProperty(position, label, property);

            var leftWidth = 0.6f;
            var rightWidth = 1.0f - leftWidth;

            var leftSide = new Rect(position.x, position.y, position.width * leftWidth, position.height);
            var rightSide = new Rect(position.width * leftWidth, position.y, position.width * rightWidth, position.height);
            {
                EditorGUI.PropertyField(leftSide, property.FindPropertyRelative("Bone"), new GUIContent("", ""));
                EditorGUI.PropertyField(rightSide, property.FindPropertyRelative("Erase"));
            }

            //EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            return height;
        }
    }

    public class BoneMeshEraserWizard : ScriptableWizard
    {
        public const string BONE_MESH_ERASER_NAME = "BoneMeshEraser";
        const string ASSET_SUFFIX = ".asset";

        [SerializeField]
        SkinnedMeshRenderer m_skinnedMesh;

        [SerializeField]
        Animator m_animator;

        [SerializeField]
        Transform EraseRoot;

        [SerializeField]
        BoneMeshEraser.EraseBone[] m_eraseBones;

        // [MenuItem(MeshUtility.MENU_PARENT + "BoneMeshEraser Wizard", priority = 31)]
        // static void CreateWizard()
        // {
        //     ScriptableWizard.DisplayWizard<BoneMeshEraserWizard>("BoneMeshEraser", "Erase triangles by bone", "Erase");
        // }

        private void OnEnable()
        {
            var root = Selection.activeGameObject;
            if (root != null)
            {
                m_animator = root.GetComponent<Animator>();
                m_skinnedMesh = root.GetComponent<SkinnedMeshRenderer>();
                OnValidate();
            }
        }

        void OnValidate()
        {
            //Debug.Log("OnValidate");
            if (m_skinnedMesh == null)
            {
                m_eraseBones = new BoneMeshEraser.EraseBone[] { };
                return;
            }

            if (EraseRoot == null)
            {
                if (m_animator != null)
                {
                    EraseRoot = m_animator.GetBoneTransform(HumanBodyBones.Head);
                    //Debug.LogFormat("head: {0}", EraseRoot);
                }
            }

            m_eraseBones = m_skinnedMesh.bones.Select(x =>
            {
                var eb = new BoneMeshEraser.EraseBone
                {
                    Bone = x,
                };

                if (EraseRoot != null)
                {
                    // 首の子孫を消去
                    if (eb.Bone.Ancestor().Any(y => y == EraseRoot))
                    {
                        //Debug.LogFormat("erase {0}", x);
                        eb.Erase = true;
                    }
                }

                return eb;
            })
            .ToArray();
        }

        void OnWizardUpdate()
        {
            helpString = "select target skinnedMesh and animator";
        }

        SkinnedMeshRenderer _Erase(GameObject go)
        {
            if (go == null)
            {
                Debug.LogWarning("select root object in hierarchy");
                return null;
            }
            if (m_skinnedMesh == null)
            {
                Debug.LogWarning("no skinnedmesh");
                return null;
            }

            var bones = m_skinnedMesh.bones;
            var eraseBones = m_eraseBones
                .Where(x => x.Erase)
                .Select(x => Array.IndexOf(bones, x.Bone))
                .ToArray();

            var meshNode = new GameObject(BONE_MESH_ERASER_NAME);
            meshNode.transform.SetParent(go.transform, false);

            var erased = meshNode.AddComponent<SkinnedMeshRenderer>();
            erased.sharedMesh = BoneMeshEraser.CreateErasedMesh(m_skinnedMesh.sharedMesh, eraseBones);
            erased.sharedMaterials = m_skinnedMesh.sharedMaterials;
            erased.bones = m_skinnedMesh.bones;

            return erased;
        }

        void Erase()
        {
            var go = Selection.activeGameObject;
            var renderer = _Erase(go);
            if (renderer == null)
            {
                return;
            }

            // save mesh to Assets
            var assetPath = string.Format("{0}{1}", go.name, ASSET_SUFFIX);
            var prefab = MeshUtility.GetPrefab(go);
            if (prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                assetPath = string.Format("{0}/{1}{2}",
                    Path.GetDirectoryName(prefabPath),
                    Path.GetFileNameWithoutExtension(prefabPath),
                    ASSET_SUFFIX
                    );
            }

            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(renderer.sharedMesh, assetPath);
        }

        void OnWizardCreate()
        {
            //Debug.Log("OnWizardCreate");
            Erase();

            // close
        }

        void OnWizardOtherButton()
        {
            //Debug.Log("OnWizardOtherButton");
            Erase();
        }
    }
}

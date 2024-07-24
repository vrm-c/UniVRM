using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace UniHumanoid
{
    class BoneNode : IEnumerable<BoneNode>
    {
        public HumanBodyBones Bone { get; private set; }

        public List<BoneNode> Children = new List<BoneNode>();

        public int[] Muscles;

        public BoneNode(HumanBodyBones bone, params int[] muscles)
        {
            Bone = bone;
            Muscles = muscles;
        }

        public IEnumerator<BoneNode> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(BoneNode child)
        {
            Children.Add(child);
        }
    }

    class BoneTreeViewItem : TreeViewItem
    {
        //HumanBodyBones m_bone;

        public BoneTreeViewItem(int id, int depth, HumanBodyBones bone) : base(id, depth, bone.ToString())
        {
            //m_bone = bone;
        }
    }

    class MuscleTreeViewItem : TreeViewItem
    {
        public int Muscle
        {
            get;
            private set;
        }

        public MuscleTreeViewItem(int id, int depth, int muscle) : base(id, depth, HumanTrait.MuscleName[muscle])
        {
            Muscle = muscle;
        }
    }

    class BoneTreeView : TreeView
    {
        static BoneNode Skeleton = new BoneNode(HumanBodyBones.Hips)
        {
            new BoneNode(HumanBodyBones.Spine, 0, 1, 2){
                new BoneNode(HumanBodyBones.Chest, 3, 4, 5){
                    new BoneNode(HumanBodyBones.UpperChest, 6, 7, 8){
                        new BoneNode(HumanBodyBones.Neck, 9, 10, 11){
                            new BoneNode(HumanBodyBones.Head, 12, 13, 14){
                                new BoneNode(HumanBodyBones.LeftEye, 15, 16),
                                new BoneNode(HumanBodyBones.RightEye, 17, 18)
                            }
                        },
                        new BoneNode(HumanBodyBones.LeftShoulder, 37, 38){
                            new BoneNode(HumanBodyBones.LeftUpperArm, 39, 40, 41){
                                new BoneNode(HumanBodyBones.LeftLowerArm, 42, 43){
                                    new BoneNode(HumanBodyBones.LeftHand, 44, 45)
                                }
                            }
                        },
                        new BoneNode(HumanBodyBones.RightShoulder, 46, 47){
                            new BoneNode(HumanBodyBones.RightUpperArm, 48, 49, 50){
                                new BoneNode(HumanBodyBones.RightLowerArm, 51, 52){
                                    new BoneNode(HumanBodyBones.RightHand, 53, 54)
                                }
                            }
                        }
                    }
                }
            },
            new BoneNode(HumanBodyBones.LeftUpperLeg, 21, 22, 23){
                new BoneNode(HumanBodyBones.LeftLowerLeg, 24, 25){
                    new BoneNode(HumanBodyBones.LeftFoot, 26, 27){
                        new BoneNode(HumanBodyBones.LeftToes, 28)
                    }
                }
            },
            new BoneNode(HumanBodyBones.RightUpperLeg, 29, 30, 31){
                new BoneNode(HumanBodyBones.RightLowerLeg, 32, 33){
                    new BoneNode(HumanBodyBones.RightFoot, 34, 35){
                        new BoneNode(HumanBodyBones.RightToes, 36)
                    }
                }
            }
        };

        //Animator m_animator;
        HumanPoseHandler m_handler;
        HumanPose m_pose;

        bool m_updated;

        public void Begin()
        {
            m_handler.GetHumanPose(ref m_pose);
        }

        public void End()
        {
            if (m_updated)
            {
                m_handler.SetHumanPose(ref m_pose);
            }
            m_updated = false;
        }

        public BoneTreeView(TreeViewState treeViewState, MultiColumnHeader header, HumanPoseHandler handler)
            : base(treeViewState, header)
        {
            m_handler = handler;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = 0, depth = -1 };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>(200);

            // We use the GameObject instanceIDs as ids for items as we want to 
            // select the game objects and not the transform components.
            rows.Clear();

            var item = CreateTreeViewItemForBone(HumanBodyBones.Hips);
            root.AddChild(item);
            rows.Add(item);

            if (IsExpanded(item.id))
            {
                AddChildrenRecursive(Skeleton, item, rows);
            }
            else
            {
                item.children = CreateChildListForCollapsedParent();
            }

            SetupDepthsFromParentsAndChildren(root);

            return rows;
        }

        void AddChildrenRecursive(BoneNode bone, TreeViewItem item, IList<TreeViewItem> rows)
        {
            int childCount = bone.Children.Count;
            item.children = new List<TreeViewItem>(childCount);
            if (bone.Muscles != null)
            {
                foreach (var muscle in bone.Muscles)
                {
                    var childItem = new MuscleTreeViewItem(muscle + 20000, -1, muscle);
                    item.AddChild(childItem);
                    rows.Add(childItem);
                }
            }

            foreach (var child in bone.Children)
            {
                var childItem = CreateTreeViewItemForBone(child.Bone);
                item.AddChild(childItem);
                rows.Add(childItem);

                //if (child.Children.Count > 0)
                {
                    if (IsExpanded(childItem.id))
                    {
                        AddChildrenRecursive(child, childItem, rows);
                    }
                    else
                    {
                        childItem.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        static TreeViewItem CreateTreeViewItemForBone(HumanBodyBones bone)
        {
            return new TreeViewItem((int)bone, -1, Enum.GetName(typeof(HumanBodyBones), bone));
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, int index, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (index)
            {
                case 0:
                    {
                        // Default icon and label
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;

                case 1:
                    {
                        var muscleItem = args.item as MuscleTreeViewItem;
                        if (muscleItem != null)
                        {
                            var muscleIndex = muscleItem.Muscle;
                            var muscles = m_pose.muscles;
                            var value = EditorGUI.Slider(cellRect, GUIContent.none, muscles[muscleIndex], -1f, 1f);
                            if (value != muscles[muscleIndex])
                            {
                                muscles[muscleIndex] = value;
                                m_updated = true;
                            }
                        }
                        else
                        {

                        }
                    }
                    break;
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 250,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Muscle value"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 110,
                    minWidth = 60,
                    autoResize = true
                },
            };
            return new MultiColumnHeaderState(columns);
        }
    }


    [CustomEditor(typeof(MuscleInspector))]
    public class MuscleInspectorEditor : Editor
    {
        [NonSerialized] bool m_Initialized;
        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        //[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        SearchField m_SearchField;
        BoneTreeView m_TreeView;

        MuscleInspector m_target;
        HumanPoseHandler m_handler;


        MultiColumnHeader GetHeaderState()
        {
            //bool firstInit = m_MultiColumnHeaderState == null;

            var headerState = BoneTreeView.CreateDefaultMultiColumnHeaderState();
            /*
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
            {
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            }
            m_MultiColumnHeaderState = headerState;
            */
            var multiColumnHeader = new MultiColumnHeader(headerState);
            multiColumnHeader.ResizeToFit();
            return multiColumnHeader;
        }

        void OnEnable()
        {
            var mi = this.target as MuscleInspector;
            if (mi.TryGetComponent<Animator>(out var animator)
            && animator.avatar != null
            && animator.avatar.isValid
            && animator.avatar.isHuman
            )
            {
                UniGLTF.UniGLTFLogger.Log("MuscleInspectorEditor.OnEnable");
                m_handler = new HumanPoseHandler(animator.avatar, animator.transform);

                m_TreeView = new BoneTreeView(new TreeViewState(), GetHeaderState(), m_handler);
            }
        }

        void OnDisable()
        {
            if (m_handler != null)
            {
                m_handler.Dispose();
                m_handler = null;
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_TreeView == null)
            {
                EditorGUILayout.HelpBox("Animator required", MessageType.Error);
                return;
            }

            var rect = GUILayoutUtility.GetRect(0, 10000, 0, m_TreeView.totalHeight);
            m_TreeView.Begin();
            m_TreeView.OnGUI(rect);
            m_TreeView.End();
        }
    }
}

using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace UniVRM10
{
    class VRM10SpringBoneTreeView : TreeView

    {
        VRM10Controller m_root;

        Dictionary<int, VRM10SpringBone> m_boneMap = new Dictionary<int, VRM10SpringBone>();

        public bool TryGetSpringBone(int id, out VRM10SpringBone bone)
        {
            return m_boneMap.TryGetValue(id, out bone);
        }

        public VRM10SpringBoneTreeView(TreeViewState treeViewState, VRM10Controller root)
            : base(treeViewState)
        {
            m_root = root;
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override TreeViewItem BuildRoot()
        {
            m_boneMap.Clear();
            var root = new TreeViewItem { id = 0, depth = -1, displayName = m_root.name };
            var allItems = new List<TreeViewItem>();
            var bones = m_root.GetComponentsInChildren<VRM10SpringBone>();
            var id = 1;
            for (int i = 0; i < bones.Length; ++i, ++id)
            {
                var bone = bones[i];

                m_boneMap.Add(id, bone);

                allItems.Add(new TreeViewItem
                {
                    id = id,
                    displayName = bone.name,
                });
            }
            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }
    }
}

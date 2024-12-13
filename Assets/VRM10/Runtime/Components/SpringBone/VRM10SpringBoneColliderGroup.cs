using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public class VRM10SpringBoneColliderGroup : MonoBehaviour
    {
        [SerializeField]
        public string Name;

        public string GUIName(int i) => $"{i:00}:{Name}";

        [SerializeField]
        public List<VRM10SpringBoneCollider> Colliders = new List<VRM10SpringBoneCollider>();

        string GetName()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return Colliders[0].name;
        }

#if UNITY_EDITOR
        [ContextMenu("Create child and move to that")]
        public void Separate()
        {
            var vrm = GetComponentInParent<Vrm10Instance>();
            if (vrm == null)
            {
                return;
            }
            if (Colliders == null || Colliders.Count == 0)
            {
                return;
            }

            UnityEditor.Undo.IncrementCurrentGroup();
            UnityEditor.Undo.SetCurrentGroupName("VRM10SpringBoneColliderGroup.Separate");
            var undo = UnityEditor.Undo.GetCurrentGroup();

            var child = new GameObject($"group: {GetName()}");
            UnityEditor.Undo.RegisterCreatedObjectUndo(child, "child");
            UnityEditor.Undo.SetTransformParent(child.transform, transform, "setParent");

            var group = UnityEditor.Undo.AddComponent<VRM10SpringBoneColliderGroup>(child);
            group.Name = Name;
            group.Colliders = Colliders.ToList();
            for (int i = 0; i < vrm.SpringBone.ColliderGroups.Count; ++i)
            {
                if (vrm.SpringBone.ColliderGroups[i] == this)
                {
                    vrm.SpringBone.ColliderGroups[i] = group;
                }
            }
            foreach (var spring in vrm.SpringBone.Springs)
            {
                for (int i = 0; i < spring.ColliderGroups.Count; ++i)
                {
                    if (spring.ColliderGroups[i] == this)
                    {
                        spring.ColliderGroups[i] = group;
                    }
                }
            }

            if (Application.isPlaying)
            {
                Destroy(this);
            }
            else
            {
                UnityEditor.Undo.DestroyObjectImmediate(this);
            }

            UnityEditor.Undo.RegisterFullObjectHierarchyUndo(vrm.gameObject, "VRM10SpringBoneColliderGroup.Separate");

            UnityEditor.Undo.CollapseUndoOperations(undo);
        }
#endif

        public void OnDrawGizmosSelected()
        {
            foreach (var collider in Colliders)
            {
                if (collider != null)
                {
                    collider.DrawGizmos();
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;


namespace UniGLTF.Utils
{
    public class ForceTransformUniqueName
    {
        public delegate string GetNameFunc<T>(T t);
        public delegate void SetNameFunc<T>(T t, string newName);
        public delegate string GetLeafNameFunc<T>(T t);

        HashSet<string> m_uniqueNameSet = new HashSet<string>();
        int m_counter = 1;

        public static void Process(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>();
            Process(transforms,
                t => t.name,
                (t, name) => t.name = name,
                (t) => (t.parent != null && t.childCount == 0) ? $"{t.parent.name}-{t.name}" : null);
        }

        public static void Process<T>(IReadOnlyList<T> transforms,
            GetNameFunc<T> getName,
            SetNameFunc<T> setName,
            GetLeafNameFunc<T> getLeafName)
        {
            var uniqueName = new ForceTransformUniqueName();
            foreach (var t in transforms)
            {
                uniqueName.RenameIfDupName(t,
                    getName,
                    setName,
                    getLeafName
                );
            }
        }

        public void RenameIfDupName<T>(T t,
            GetNameFunc<T> getName,
            SetNameFunc<T> setName,
            GetLeafNameFunc<T> getLeafName)
        {
            if (!m_uniqueNameSet.Contains(getName(t)))
            {
                m_uniqueNameSet.Add(getName(t));
                return;
            }

            {
                var leafName = getLeafName(t);
                if (!string.IsNullOrEmpty(leafName))
                {
                    /// AvatarBuilder:BuildHumanAvatar で同名の Transform があるとエラーになる。
                    /// 
                    /// AvatarBuilder 'GLTF': Ambiguous Transform '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/neck_1/neck_2/head/ENDSITE' and '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/l_shoulder/l_up_arm/l_low_arm/l_hand/ENDSITE' found in hierarchy for human bone 'Head'. Transform name mapped to a human bone must be unique.
                    /// UnityEngine.AvatarBuilder:BuildHumanAvatar (UnityEngine.GameObject,UnityEngine.HumanDescription)
                    /// 
                    /// 主に BVH の EndSite 由来の GameObject 名が重複することへの対策
                    ///  ex: parent-ENDSITE
                    if (!m_uniqueNameSet.Contains(leafName))
                    {
                        DoRename(t, getName, setName, leafName);
                        return;
                    }
                }
            }

            // 連番
            for (int i = 0; i < 100; ++i)
            {
                // ex: name.1
                var newName = $"{getName(t)}{m_counter++}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    DoRename(t, getName, setName, newName);
                    return;
                }
            }

            throw new NotImplementedException();
        }

        private void DoRename<T>(T t,
            GetNameFunc<T> getName,
            SetNameFunc<T> setName,
            string newName)
        {
            UniGLTFLogger.Warning($"force rename !!: {getName(t)} => {newName}");
            setName(t, newName);
            m_uniqueNameSet.Add(newName);
        }
    }
}
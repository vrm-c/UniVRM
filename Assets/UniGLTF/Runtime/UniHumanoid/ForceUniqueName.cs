using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniHumanoid
{
    class ForceUniqueName
    {
        HashSet<string> m_uniqueNameSet = new HashSet<string>();
        int m_counter = 1;

        public static void Process(Transform root)
        {
            var uniqueName = new ForceUniqueName();
            var transforms = root.GetComponentsInChildren<Transform>();
            foreach (var t in transforms)
            {
                uniqueName.RenameIfDupName(t);
            }
        }

        public void RenameIfDupName(Transform t)
        {
            if (!m_uniqueNameSet.Contains(t.name))
            {
                m_uniqueNameSet.Add(t.name);
                return;
            }

            if (t.parent != null && t.childCount == 0)
            {
                /// AvatarBuilder:BuildHumanAvatar で同名の Transform があるとエラーになる。
                /// 
                /// AvatarBuilder 'GLTF': Ambiguous Transform '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/neck_1/neck_2/head/ENDSITE' and '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/l_shoulder/l_up_arm/l_low_arm/l_hand/ENDSITE' found in hierarchy for human bone 'Head'. Transform name mapped to a human bone must be unique.
                /// UnityEngine.AvatarBuilder:BuildHumanAvatar (UnityEngine.GameObject,UnityEngine.HumanDescription)
                /// UniHumanoid.AvatarDescription:CreateAvatar (UnityEngine.Transform) 
                /// 
                /// 主に BVH の EndSite 由来の GameObject 名が重複することへの対策
                ///  ex: parent-ENDSITE
                var newName = $"{t.parent.name}-{t.name}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    Debug.LogWarning($"force rename !!: {t.name} => {newName}");
                    t.name = newName;
                    m_uniqueNameSet.Add(newName);
                    return;
                }
            }

            // 連番
            for (int i = 0; i < 100; ++i)
            {
                // ex: name.1
                var newName = $"{t.name}{m_counter++}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    Debug.LogWarning($"force rename: {t.name} => {newName}", t);
                    t.name = newName;
                    m_uniqueNameSet.Add(newName);
                    return;
                }
            }

            throw new NotImplementedException();
        }
    }
}
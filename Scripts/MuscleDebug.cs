using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniHumanoid
{
    public class MuscleDebug : MonoBehaviour
    {
        Avatar GetAvatar()
        {
            var animator = GetComponent<Animator>();
            if (animator != null && animator.avatar != null)
            {
                return animator.avatar;
            }

            var transfer = GetComponent<HumanPoseTransfer>();
            if (transfer != null && transfer.Avatar != null)
            {
                return transfer.Avatar;
            }

            return null;
        }

        HumanPoseHandler m_handler;

        public HumanPose m_pose;

        [Serializable]
        public struct Muscle
        {
            public int Index;
            public string Name;
            public float Value;
        }

        public Vector3 BodyPosition;

        public Muscle[] Muscles;

        private void OnEnable()
        {
            var avatar = GetAvatar();
            if (avatar == null)
            {
                enabled = false;
                return;
            }

            m_handler = new HumanPoseHandler(avatar, transform);

            Muscles = HumanTrait.MuscleName.Select((x, i) =>
            {
                return new Muscle
                {
                    Index = i,
                    Name = x,
                };
            })
            .ToArray()
            ;
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            m_handler.GetHumanPose(ref m_pose);

            BodyPosition = m_pose.bodyPosition;

            for (int i = 0; i < m_pose.muscles.Length; ++i)
            {
                Muscles[i].Value = m_pose.muscles[i];
            }
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MuscleDebug.Muscle))]
    public class MuscleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position,
                             SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("Name");
            var valueProp = property.FindPropertyRelative("Value");
            /*
            var labl = string.Format("{0}: {1}", 
                nameProp.stringValue, 
                valueProp.floatValue
                );
                */
            EditorGUI.LabelField(position, nameProp.stringValue, string.Format("{0:0.00}", valueProp.floatValue));
        }
    }
#endif
}

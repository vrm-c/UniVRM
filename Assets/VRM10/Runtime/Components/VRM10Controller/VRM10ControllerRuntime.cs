using System;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Play時 と Editorからの参照情報置き場
    /// </summary>
    public class VRM10ControllerRuntime
    {
        VRM10Controller m_target;
        VRM10Constraint[] m_constraints;
        Transform m_head;

        public VRM10ControllerRuntime(VRM10Controller target)
        {
            m_target = target;
            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                throw new Exception();
            }
            m_head = animator.GetBoneTransform(HumanBodyBones.Head);
            target.Vrm.LookAt.Setup(animator, m_head, target.LookAtTargetType, target.Gaze);
            target.Vrm.Expression.Setup(target, target.Vrm.LookAt, target.Vrm.LookAt.EyeDirectionApplicable);

            if (m_constraints == null)
            {
                m_constraints = target.GetComponentsInChildren<VRM10Constraint>();
            }
        }

        /// <summary>
        /// 毎フレーム関連コンポーネントを解決する
        /// 
        /// * Contraint
        /// * Spring
        /// * LookAt
        /// * Expression
        /// 
        /// </summary>
        public void Process()
        {
            // 
            // constraint
            //
            foreach (var constraint in m_constraints)
            {
                constraint.Process();
            }

            //
            // spring
            //
            m_target.SpringBone.Process(m_target.SpringBoneCenter);

            //
            // gaze control
            //
            m_target.Vrm.LookAt.Process(m_target.LookAtTargetType, m_target.Gaze);

            //
            // expression
            //
            m_target.Vrm.Expression.Process();
        }
    }
}

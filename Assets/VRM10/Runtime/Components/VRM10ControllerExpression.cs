using System;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public class VRM10ControllerExpression : IDisposable
    {
        [SerializeField]
        public VRM10ExpressionAvatar ExpressionAvatar;

        ExpressionMerger m_merger;

        public void Dispose()
        {
            if (m_merger != null)
            {
                m_merger.RestoreMaterialInitialValues();
            }
        }

        IExpressionAccumulator m_accumulator;

        public IExpressionAccumulator Accumulator
        {
            get
            {
                if (m_accumulator == null)
                {
                    m_accumulator = new DefaultExpressionAccumulator();
                }
                return m_accumulator;
            }
        }

        public void OnStart(Transform transform)
        {
            if (ExpressionAvatar != null)
            {
                if (m_merger == null)
                {
                    m_merger = new ExpressionMerger(ExpressionAvatar.Clips, transform);
                }

                Accumulator.OnStart(ExpressionAvatar);
            }
        }

        public void Apply()
        {
            m_merger.SetValues(m_accumulator.FrameExpression());
        }
    }
}

using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 対象の初期位置と現在位置の差分(delta)を、自身の初期位置に対してWeightを乗算して加算する。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10PositionConstraint : VRM10Constraint, IVRM10ConstraintSourceDestination
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        public ObjectSpace SourceCoordinate = default;

        [SerializeField]
        public ObjectSpace DestinationCoordinate = default;

        [SerializeField]
        public AxisMask FreezeAxes = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        ConstraintSource m_src;

        public TR GetSourceCoords()
        {
            if (Source == null)
            {
                throw new ConstraintException(ConstraintException.ExceptionTypes.NoSource);
            }

            switch (SourceCoordinate)
            {
                case ObjectSpace.model:
                    {
                        if (ModelRoot == null)
                        {
                            throw new ConstraintException(ConstraintException.ExceptionTypes.NoModelWithModelSpace);
                        }

                        if (m_src == null)
                        {
                            return new TR(ModelRoot.rotation, Source.position);
                        }

                        // runtime
                        return new TR(ModelRoot.rotation, m_src.ModelInitial.Translation);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src == null)
                        {
                            return TR.FromWorld(Source);
                        }

                        // runtime
                        var parent = TR.Identity;
                        if (Source.parent != null)
                        {
                            parent = TR.FromWorld(Source.parent);
                        }
                        return parent * m_src.LocalInitial;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public TR GetSourceCurrent()
        {
            var coords = GetSourceCoords();
            if (m_src == null)
            {
                return coords;
            }

            return new TR(Delta) * coords;
        }

        public Vector3 Delta
        {
            get;
            private set;
        }

        ConstraintDestination m_dst;

        public TR GetDstCoords()
        {
            switch (DestinationCoordinate)
            {
                case ObjectSpace.model:
                    {
                        if (ModelRoot == null)
                        {
                            throw new ConstraintException(ConstraintException.ExceptionTypes.NoModelWithModelSpace);
                        }
                        return new TR(ModelRoot.rotation, transform.position);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src == null)
                        {
                            return TR.FromWorld(transform);
                        }

                        // runtime
                        var parent = TR.Identity;
                        if (transform.parent != null)
                        {
                            parent = TR.FromWorld(transform.parent);
                        }
                        return parent * m_dst.LocalInitial;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public TR GetDstCurrent()
        {
            var coords = GetDstCoords();
            if (m_src == null)
            {
                return coords;
            }

            return new TR(Delta) * coords;
        }

        /// <summary>
        /// Editorで設定値の変更を反映するために、クリアする
        /// </summary>
        void OnValidate()
        {
            // Debug.Log("Validate");
            if (m_src != null && m_src.ModelRoot != ModelRoot)
            {
                m_src = null;
            }
            if (m_dst != null && m_dst.ModelRoot != ModelRoot)
            {
                m_dst = null;
            }
        }

        void Reset()
        {
            var current = transform;
            while (current.parent != null)
            {
                current = current.parent;
            }
            ModelRoot = current;
        }

        public override void Process()
        {
            if (Source == null)
            {
                enabled = false;
                return;
            }

            if (m_src == null)
            {
                m_src = new ConstraintSource(Source, ModelRoot);
            }
            if (m_dst == null)
            {
                m_dst = new ConstraintDestination(transform, ModelRoot);
            }

            Delta = FreezeAxes.Freeze(m_src.TranslationDelta(SourceCoordinate));
            m_dst.ApplyTranslation(Delta, Weight, DestinationCoordinate, ModelRoot);
        }
    }
}

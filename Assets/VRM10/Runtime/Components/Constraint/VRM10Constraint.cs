using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    public abstract class VRM10Constraint : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        protected virtual void Reset()
        {
            var current = transform;
            while (true)
            {
                current = current.parent;
                if (current.parent == null)
                {
                    // root
                    break;
                }
                if (current.GetComponent<Vrm10Instance>() != null)
                {
                    // model root
                    break;
                }
            }
            ModelRoot = current;
        }

        #region Source
        public abstract Transform Source { get; set; }

        public ConstraintSource m_src;


        protected TR SourceInitialCoords(ObjectSpace space)
        {
            switch (space)
            {
                case ObjectSpace.model:
                    if (m_src == null)
                    {
                        return new TR(ModelRoot.rotation, Source.position);
                    }
                    else
                    {
                        var r = (TR.FromParent(ModelRoot) * m_src.ModelInitial).Rotation;
                        var t = (TR.FromParent(Source) * m_src.LocalInitial).Translation;
                        return new TR(r, t);
                    }

                case ObjectSpace.local:
                    if (m_src == null)
                    {
                        return TR.FromWorld(Source);
                    }
                    else
                    {
                        return TR.FromParent(Source) * m_src.LocalInitial;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region Destination
        protected ConstraintDestination m_dst;

        protected TR DestinationInitialCoords(ObjectSpace space)
        {
            switch (space)
            {
                case ObjectSpace.model:
                    if (m_dst == null)
                    {
                        return new TR(ModelRoot.rotation, transform.position);
                    }
                    else
                    {
                        var r = (TR.FromParent(ModelRoot) * m_dst.ModelInitial).Rotation;
                        var t = (TR.FromParent(transform) * m_dst.LocalInitial).Translation;
                        return new TR(r, t);
                    }

                case ObjectSpace.local:
                    if (m_dst == null)
                    {
                        return TR.FromWorld(transform);
                    }
                    else
                    {
                        return TR.FromParent(transform) * m_dst.LocalInitial;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        public void Process()
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

            OnProcess();
        }

        public abstract void OnProcess();
    }
}

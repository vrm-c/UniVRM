using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;


namespace UniVRM10
{
    /// <summary>
    /// 対象の初期回転と現在回転の差分(delta)を、自身の初期回転と自身の初期回転にdeltaを乗算したものに対してWeightでSlerpする。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRM10RotationConstraint : VRM10Constraint
    {
        [SerializeField]
        public Transform Source = default;

        [SerializeField]
        public VRM10RotationOffset SourceOffset = VRM10RotationOffset.Identity;

        [SerializeField]
        public ObjectSpace SourceCoordinate = default;

        [SerializeField]
        public ObjectSpace DestinationCoordinate = default;

        [SerializeField]
        [EnumFlags]
        public AxisMask FreezeAxes = default;

        [SerializeField]
        [Range(0, 10.0f)]
        public float Weight = 1.0f;

        [SerializeField]
        public Transform ModelRoot = default;

        ConstraintSource m_src;

        /// <summary>
        /// Source の座標系
        /// </summary>
        /// <returns></returns>
        public TR GetSourceCoords()
        {
            switch (SourceCoordinate)
            {
                case ObjectSpace.model:
                    {

                        var r = Quaternion.identity;
                        if (Source != null)
                        {
                            if (ModelRoot != null)
                            {
                                r = ModelRoot.rotation;
                            }
                        }

                        var t = Vector3.zero;
                        if (Source != null)
                        {
                            t = Source.position;
                        }

                        return new TR(r, t);
                    }

                case ObjectSpace.local:
                    {
                        if (Source != null)
                        {
                            if (m_src != null)
                            {
                                // runtime
                                var parent = TR.Identity;
                                if (Source.parent != null)
                                {
                                    parent = TR.FromWorld(Source.parent);
                                }
                                return parent * m_src.LocalInitial;
                            }
                            else
                            {
                                return TR.FromWorld(Source);
                            }
                        }

                        return TR.Identity;
                    }
            }

            throw new NotImplementedException();
        }

        public TR GetSourceCurrent()
        {
            switch (SourceCoordinate)
            {
                case ObjectSpace.model:
                    {
                        var r = Quaternion.identity;
                        if (Source != null)
                        {
                            if (ModelRoot != null)
                            {
                                r = ModelRoot.rotation;
                                if (m_src != null)
                                {
                                    r *= m_src.RotationDelta(ObjectSpace.model);
                                }
                            }
                        }

                        var t = Vector3.zero;
                        if (Source != null)
                        {
                            t = Source.position;
                        }

                        return new TR(r, t);
                    }

                case ObjectSpace.local:
                    {
                        if (Source != null)
                        {
                            if (m_src != null)
                            {
                                // runtime
                                var parent = TR.Identity;
                                if (Source.parent != null)
                                {
                                    parent = TR.FromWorld(Source.parent);
                                }
                                return parent * m_src.LocalInitial;
                            }
                            else
                            {
                                return TR.FromWorld(Source);
                            }
                        }

                        return TR.Identity;
                    }
            }

            throw new NotImplementedException();
        }

        public Quaternion Delta
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
                        var r = Quaternion.identity;
                        if (ModelRoot != null)
                        {
                            r = ModelRoot.rotation;
                        }
                        return new TR(r, transform.position);
                    }

                case ObjectSpace.local:
                    {
                        if (m_src != null)
                        {
                            // runtime
                            var parent = TR.Identity;
                            if (transform.parent != null)
                            {
                                parent = TR.FromWorld(transform.parent);
                            }
                            return parent * m_dst.LocalInitial;
                        }
                        else
                        {
                            return TR.FromWorld(transform);
                        }
                    }
            }

            throw new NotImplementedException();
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

        /// <summary>
        /// SourceのUpdateよりも先か後かはその時による。
        /// 厳密に制御するのは無理。
        /// </summary>
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

            // 軸制限をしたオイラー角
            Delta = m_src.RotationDelta(SourceCoordinate);
            var fleezed = FreezeAxes.Freeze(Delta.eulerAngles);
            var rotation = Quaternion.Euler(fleezed);
            // Debug.Log($"{delta} => {rotation}");
            // オイラー角を再度Quaternionへ。weight を加味してSlerpする
            m_dst.ApplyRotation(rotation, Weight, DestinationCoordinate, ModelRoot);
        }
    }
}

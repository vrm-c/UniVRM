using UnityEngine;
using System.Linq;
using System;

namespace VRM
{
    [Obsolete("Use VRMBlendShapeProxy")]
    public class BlendShapeClipHandler
    {
        BlendShapeClip m_clip;
        [Obsolete("Use Clip")]
        public BlendShapeClip Cilp
        {
            get { return Clip; }
        }
        public BlendShapeClip Clip
        {
            get { return m_clip; }
        }
        SkinnedMeshRenderer[] m_renderers;

        public BlendShapeClipHandler(BlendShapeClip clip, Transform transform)
        {
            m_clip = clip;

            if (m_clip != null && m_clip.Values != null && transform != null)
            {
                m_renderers = m_clip.Values.Select(x =>
                {
                    var target = UniGLTF.UnityExtensions.GetFromPath(transform, x.RelativePath);
                    return target.GetComponent<SkinnedMeshRenderer>();
                })
                 .ToArray();
            }
        }

        public float LastValue
        {
            get;
            private set;
        }

        public void Apply(float value)
        {
            LastValue = value;

            if (m_clip == null) return;
            if (m_renderers == null) return;

            for (int i = 0; i < m_clip.Values.Length; ++i)
            {
                var binding = m_clip.Values[i];
                var target = m_renderers[i];
                if (binding.Index >= 0 && binding.Index < target.sharedMesh.blendShapeCount)
                {
                    target.SetBlendShapeWeight(binding.Index, binding.Weight * value);
                }
            }
        }
    }
}

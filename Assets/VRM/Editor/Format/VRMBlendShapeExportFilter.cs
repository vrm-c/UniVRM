using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public class VRMBlendShapeExportFilter : IBlendShapeExportFilter
    {
        static bool ClipsContainsName(IReadOnlyList<BlendShapeClip> clips, bool onlyPreset, BlendShapeBinding binding)
        {
            foreach (var c in clips)
            {
                if (onlyPreset)
                {
                    if (c.Preset == BlendShapePreset.Unknown)
                    {
                        continue;
                    }
                }

                foreach (var b in c.Values)
                {
                    if (b.RelativePath == binding.RelativePath && b.Index == binding.Index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public VRMExportSettings VRMExportSettings;
        public List<BlendShapeClip> Clips;

        public VRMBlendShapeExportFilter(GameObject exportRoot, VRMExportSettings settings)
        {
            VRMExportSettings = settings;
            Clips = new List<BlendShapeClip>();
            if (exportRoot != null)
            {
                var proxy = exportRoot.GetComponent<VRMBlendShapeProxy>();
                if (proxy != null)
                {
                    if (proxy.BlendShapeAvatar != null)
                    {
                        Clips.AddRange(proxy.BlendShapeAvatar.Clips);
                    }
                }
            }
        }

        public bool UseBlendShape(int index, string relativePath)
        {
            if (VRMExportSettings.ReduceBlendshape)
            {
                if (!ClipsContainsName(Clips, VRMExportSettings, new BlendShapeBinding
                {
                    Index = index,
                    RelativePath = relativePath,
                }))
                {
                    // skip
                    return false;
                }
            }

            return true;
        }
    }
}

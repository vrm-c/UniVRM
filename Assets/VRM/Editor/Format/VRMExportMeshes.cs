using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// Export時にMeshを一覧する。
    /// 
    /// Mesh関連の Validation する。
    /// Meshのエクスポートサイズを試算する。
    /// </summary>
    [Serializable]
    public class VRMExportMeshes : MeshExportValidator
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

        public override bool UseBlendShape(int index, string relativePath)
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

        public void SetRoot(GameObject ExportRoot, VRMExportSettings settings)
        {
            VRMExportSettings = settings;
            Clips = new List<BlendShapeClip>();
            if (ExportRoot != null)
            {
                var proxy = ExportRoot.GetComponent<VRMBlendShapeProxy>();
                if (proxy != null)
                {
                    if (proxy.BlendShapeAvatar != null)
                    {
                        Clips.AddRange(proxy.BlendShapeAvatar.Clips);
                    }
                }
            }

            SetRoot(ExportRoot, settings.MeshExportSettings);
        }
    }
}

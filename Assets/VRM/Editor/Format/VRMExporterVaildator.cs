using System.Collections.Generic;
using UniGLTF;
using UniGLTF.M17N;
using UnityEngine;

namespace VRM
{
    public static class VRMExporterValidator
    {
        public enum VRMExporterWizardMessages
        {
            [LangMsg(Languages.ja, "VRMBlendShapeProxyが必要です。先にVRMフォーマットに変換してください")]
            [LangMsg(Languages.en, "VRMBlendShapeProxy is required. Please convert to VRM format first")]
            NEEDS_VRM_BLENDSHAPE_PROXY,
        }

        public static bool ReduceBlendshape;

        public static IEnumerable<Validation> Validate(GameObject ExportRoot)
        {
            if (ExportRoot == null)
            {
                yield break;
            }

            if (ReduceBlendshape && ExportRoot.GetComponent<VRMBlendShapeProxy>() == null)
            {
                yield return Validation.Error(VRMExporterWizardMessages.NEEDS_VRM_BLENDSHAPE_PROXY.Msg());
            }

            var vrmMeta = ExportRoot.GetComponent<VRMMeta>();
            if (vrmMeta != null && vrmMeta.Meta != null && vrmMeta.Meta.Thumbnail != null)
            {
                var thumbnailName = vrmMeta.Meta.Thumbnail.name;
                if (NameValidator.IsFileNameLengthTooLong(thumbnailName))
                {
                    yield return Validation.Error(NameValidator.ValidationMessages.FILENAME_TOO_LONG.Msg() + thumbnailName);
                }
            }
        }
    }
}

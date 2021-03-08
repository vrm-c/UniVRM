using System.Collections;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public class VRMEditorImporterContext : EditorImporterContext
    {
        VRMImporterContext m_context;

        public VRMEditorImporterContext(VRMImporterContext context) : base(context)
        {
            m_context = context;
        }

        public override bool AvoidOverwriteAndLoad(UnityPath assetPath, UnityEngine.Object o)
        {
            if (o is BlendShapeAvatar)
            {
                var loaded = assetPath.LoadAsset<BlendShapeAvatar>();
                var proxy = m_context.Root.GetComponent<VRMBlendShapeProxy>();
                proxy.BlendShapeAvatar = loaded;

                return true;
            }

            if (o is BlendShapeClip)
            {
                return true;
            }

            return base.AvoidOverwriteAndLoad(assetPath, o);
        }

        public override UnityPath GetAssetPath(UnityPath prefabPath, UnityEngine.Object o, bool meshAsSubAsset)
        {
            if (o is BlendShapeAvatar
                || o is BlendShapeClip)
            {
                var dir = prefabPath.GetAssetFolder(".BlendShapes");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is Avatar)
            {
                var dir = prefabPath.GetAssetFolder(".Avatar");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is VRMMetaObject)
            {
                var dir = prefabPath.GetAssetFolder(".MetaObject");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else if (o is UniHumanoid.AvatarDescription)
            {
                var dir = prefabPath.GetAssetFolder(".AvatarDescription");
                var assetPath = dir.Child(o.name.EscapeFilePath() + ".asset");
                return assetPath;
            }
            else
            {
                return base.GetAssetPath(prefabPath, o, meshAsSubAsset);
            }
        }
    }
}

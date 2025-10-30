using UnityEngine;

namespace UniGLTF.UniUnlit
{
    public class UniUnlitContext
    {
        public Material Material { get; }
        public bool UnsafeEditMode { get; set; } = false;

        public UniUnlitContext(Material material)
        {
            Material = material;
        }

        public UniUnlitRenderMode RenderMode
        {
            get => UniUnlitUtil.GetRenderMode(Material);
            set
            {
                UniUnlitUtil.SetRenderMode(Material, value);
                if (!UnsafeEditMode) UniUnlitUtil.ValidateProperties(Material, isRenderModeChangedByUser: true);
            }
        }

        public UniUnlitCullMode CullMode
        {
            get => UniUnlitUtil.GetCullMode(Material);
            set
            {
                UniUnlitUtil.SetCullMode(Material, value);
                if (!UnsafeEditMode) UniUnlitUtil.ValidateProperties(Material);
            }
        }

        public UniUnlitVertexColorBlendOp VColBlendMode
        {
            get => UniUnlitUtil.GetVColBlendMode(Material);
            set
            {
                UniUnlitUtil.SetVColBlendMode(Material, value);
                if (!UnsafeEditMode) UniUnlitUtil.ValidateProperties(Material);
            }
        }

        public Color MainColorSrgb
        {
            get => Material.GetColor(UniUnlitUtil.PropNameColor);
            set => Material.SetColor(UniUnlitUtil.PropNameColor, value);
        }

        public Texture MainTexture
        {
            get => Material.GetTexture(UniUnlitUtil.PropNameMainTex);
            set => Material.SetTexture(UniUnlitUtil.PropNameMainTex, value);
        }

        public Vector2 MainTextureOffset
        {
            get => Material.GetTextureOffset(UniUnlitUtil.PropNameMainTex);
            set => Material.SetTextureOffset(UniUnlitUtil.PropNameMainTex, value);
        }

        public Vector2 MainTextureScale
        {
            get => Material.GetTextureScale(UniUnlitUtil.PropNameMainTex);
            set => Material.SetTextureScale(UniUnlitUtil.PropNameMainTex, value);
        }

        public float Cutoff
        {
            get => Material.GetFloat(UniUnlitUtil.PropNameCutoff);
            set => Material.SetFloat(UniUnlitUtil.PropNameCutoff, value);
        }

        public void Validate()
        {
            UniUnlitUtil.ValidateProperties(Material, isRenderModeChangedByUser: true);
        }
    }
}
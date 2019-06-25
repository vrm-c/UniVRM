using UnityEngine;


namespace UniGLTF
{
    public interface IShaderStore
    {
        Shader GetShader(glTFMaterial material);
    }

    public class ShaderStore : IShaderStore
    {
        readonly string m_defaultShaderName = "Standard";
        Shader m_default;
        Shader Default
        {
            get
            {
                if (m_default == null)
                {
                    m_default = Shader.Find(m_defaultShaderName);
                }
                return m_default;
            }
        }

        Shader m_vcolor;
        Shader VColor
        {
            get
            {
                if (m_vcolor == null) m_vcolor = Shader.Find("UniGLTF/StandardVColor");
                return m_vcolor;
            }
        }

        Shader m_uniUnlit;
        Shader UniUnlit
        {
            get
            {
                if (m_uniUnlit == null) m_uniUnlit = Shader.Find("UniGLTF/UniUnlit");
                return m_uniUnlit;
            }
        }

        Shader m_unlitTexture;
        Shader UnlitTexture
        {
            get
            {
                if (m_unlitTexture == null) m_unlitTexture = Shader.Find("Unlit/Texture");
                return m_unlitTexture;
            }
        }

        Shader m_unlitColor;
        Shader UnlitColor
        {
            get
            {
                if (m_unlitColor == null) m_unlitColor = Shader.Find("Unlit/Color");
                return m_unlitColor;
            }
        }

        Shader m_unlitTransparent;
        Shader UnlitTransparent
        {
            get
            {
                if (m_unlitTransparent == null) m_unlitTransparent = Shader.Find("Unlit/Transparent");
                return m_unlitTransparent;
            }
        }

        Shader m_unlitCutout;
        Shader UnlitCutout
        {
            get
            {
                if (m_unlitCutout == null) m_unlitCutout = Shader.Find("Unlit/Transparent Cutout");
                return m_unlitCutout;
            }
        }

        //ImporterContext m_context;
        public ShaderStore(ImporterContext _)
        {
            //m_context = context;
        }

        public static bool IsWhite(float[] color)
        {
            if (color == null) return false;
            if(color.Length!=4)return false;
            if(color[0]!=1
                || color[1]!=1
                || color[2]!=1
                || color[3] != 1)
            {
                return false;
            }
            return true;
        }

        public Shader GetShader(glTFMaterial material)
        {
            if (material == null)
            {
                return Default;
            }

            if (material.extensions != null && material.extensions.KHR_materials_unlit != null)
            {
                return UniUnlit;
            }

            // standard
            return Default;
        }
    }
}

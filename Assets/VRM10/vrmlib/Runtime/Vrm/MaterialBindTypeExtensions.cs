using System;

namespace VrmLib
{
    public static class MaterialBindTypeExtensions
    {
        public const string UV_PROPERTY = "_MainTex_ST";
        public const string COLOR_PROPERTY = "_Color";
        public const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
        public const string RIM_COLOR_PROPERTY = "_RimColor";
        public const string OUTLINE_COLOR_PROPERTY = "_OutlineColor";
        public const string SHADE_COLOR_PROPERTY = "_ShadeColor";

        #region UnlitMaterial
        static string _GetProperty(UnlitMaterial unlit, MaterialBindType bindType)
        {
            switch (bindType)
            {
                // case MaterialBindType.UvOffset:
                // case MaterialBindType.UvScale:
                //     return UV_PROPERTY;

                case MaterialBindType.Color:
                    return COLOR_PROPERTY;
            }

            throw new NotImplementedException();
        }

        static MaterialBindType _GetBindType(UnlitMaterial unlit, string property)
        {
            switch (property)
            {
                // case UV_PROPERTY:
                //     return MaterialBindType.UvOffset;

                case COLOR_PROPERTY:
                    return MaterialBindType.Color;
            }

            throw new NotImplementedException();
        }
        #endregion

        #region PBRMaterial
        static string _GetProperty(PBRMaterial pbr, MaterialBindType bindType)
        {
            switch (bindType)
            {
                // case MaterialBindType.UvOffset:
                // case MaterialBindType.UvScale:
                //     return UV_PROPERTY;

                case MaterialBindType.Color:
                    return COLOR_PROPERTY;

                case MaterialBindType.EmissionColor:
                    return EMISSION_COLOR_PROPERTY;
            }

            throw new NotImplementedException();
        }

        static MaterialBindType _GetBindType(PBRMaterial pbr, string property)
        {
            switch (property)
            {
                // case UV_PROPERTY:
                //     return MaterialBindType.UvOffset;

                case COLOR_PROPERTY:
                    return MaterialBindType.Color;

                case EMISSION_COLOR_PROPERTY:
                    return MaterialBindType.EmissionColor;
            }

            throw new NotImplementedException();
        }
        #endregion

        #region MToon
        static string _GetProperty(MToonMaterial mtoon, MaterialBindType bindType)
        {
            switch (bindType)
            {
                // case MaterialBindType.UvOffset:
                // case MaterialBindType.UvScale:
                //     return UV_PROPERTY;

                case MaterialBindType.Color:
                    return COLOR_PROPERTY;

                case MaterialBindType.EmissionColor:
                    return EMISSION_COLOR_PROPERTY;

                case MaterialBindType.ShadeColor:
                    return SHADE_COLOR_PROPERTY;

                case MaterialBindType.RimColor:
                    return RIM_COLOR_PROPERTY;

                case MaterialBindType.OutlineColor:
                    return OUTLINE_COLOR_PROPERTY;

            }

            throw new NotImplementedException();
        }

        static MaterialBindType _GetBindType(this MToonMaterial mtoon, string property)
        {
            switch (property)
            {
                // case UV_PROPERTY:
                //     return MaterialBindType.UvOffset;

                case COLOR_PROPERTY:
                    return MaterialBindType.Color;

                case EMISSION_COLOR_PROPERTY:
                    return MaterialBindType.EmissionColor;

                case RIM_COLOR_PROPERTY:
                    return MaterialBindType.RimColor;

                case SHADE_COLOR_PROPERTY:
                    return MaterialBindType.ShadeColor;

                case OUTLINE_COLOR_PROPERTY:
                    return MaterialBindType.OutlineColor;
            }

            throw new NotImplementedException();
        }
        #endregion

        public static string GetProperty(this MaterialBindType bindType, Material material)
        {
            if (material is MToonMaterial mtoon)
            {
                return _GetProperty(mtoon, bindType);
            }

            if (material is UnlitMaterial unlit)
            {
                return _GetProperty(unlit, bindType);
            }

            if (material is PBRMaterial pbr)
            {
                return _GetProperty(pbr, bindType);
            }

            throw new NotImplementedException();
        }

        public static MaterialBindType GetBindType(this Material material, string property)
        {
            if (material is MToonMaterial mtoon)
            {
                return _GetBindType(mtoon, property);
            }

            if (material is UnlitMaterial unlit)
            {
                return _GetBindType(unlit, property);
            }

            if (material is PBRMaterial pbr)
            {
                return _GetBindType(pbr, property);
            }

            throw new NotImplementedException();
        }

        public static string GetProperty(MaterialBindType bindType)
        {
            switch (bindType)
            {
                // case MaterialBindType.UvOffset:
                // case MaterialBindType.UvScale:
                //     return UV_PROPERTY;

                case MaterialBindType.Color:
                    return COLOR_PROPERTY;

                case MaterialBindType.EmissionColor:
                    return EMISSION_COLOR_PROPERTY;

                case MaterialBindType.ShadeColor:
                    return SHADE_COLOR_PROPERTY;

                case MaterialBindType.RimColor:
                    return RIM_COLOR_PROPERTY;

                case MaterialBindType.OutlineColor:
                    return OUTLINE_COLOR_PROPERTY;

            }

            throw new NotImplementedException();
        }

        public static MaterialBindType GetBindType(string property)
        {
            switch (property)
            {
                case COLOR_PROPERTY:
                    return MaterialBindType.Color;

                case EMISSION_COLOR_PROPERTY:
                    return MaterialBindType.EmissionColor;

                case RIM_COLOR_PROPERTY:
                    return MaterialBindType.RimColor;

                case SHADE_COLOR_PROPERTY:
                    return MaterialBindType.ShadeColor;

                case OUTLINE_COLOR_PROPERTY:
                    return MaterialBindType.OutlineColor;
            }

            throw new NotImplementedException();
        }
    }
}

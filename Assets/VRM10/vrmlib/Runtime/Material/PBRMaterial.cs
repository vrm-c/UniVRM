using System;
using System.Numerics;

namespace VrmLib
{
    public class PBRMaterial : Material, IEquatable<PBRMaterial>
    {
        public PBRMaterial(string name) : base(name)
        {
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[PBR]{Name}");
            if (BaseColorTexture != null)
            {
                sb.Append(" ColorTex");
            }
            if (MetallicRoughnessTexture != null)
            {
                sb.Append("  MetallicRoughnessTex");
            }
            if (EmissiveTexture != null)
            {
                sb.Append(" EmissiveTex");
            }
            if (NormalTexture != null)
            {
                sb.Append(" NormalTex");
            }
            if (OcclusionTexture != null)
            {
                sb.Append(" OcclusionTex");
            }
            return sb.ToString();
        }

        public Single MetallicFactor;
        public Single RoughnessFactor;
        public Texture MetallicRoughnessTexture;

        public Vector3 EmissiveFactor = Vector3.Zero;
        public Texture EmissiveTexture;

        public Texture NormalTexture;
        public float NormalTextureScale = 1.0f;

        public Texture OcclusionTexture;
        public float OcclusionTextureStrength = 1.0f;

        public bool Equals(PBRMaterial other)
        {
            if (!base.Equals(other)) return false;
            if (MetallicFactor != other.MetallicFactor) return false;
            if (RoughnessFactor != other.RoughnessFactor) return false;
            if (MetallicRoughnessTexture != other.MetallicRoughnessTexture) return false;
            if (EmissiveFactor != other.EmissiveFactor) return false;
            if (EmissiveTexture != other.EmissiveTexture) return false;
            if (NormalTexture != other.NormalTexture) return false;
            if (NormalTextureScale != other.NormalTextureScale) return false;
            if (OcclusionTexture != other.OcclusionTexture) return false;
            if (OcclusionTextureStrength != other.OcclusionTextureStrength) return false;

            return true;
        }

        public override bool CanIntegrate(Material _rhs)
        {
            var rhs = _rhs as PBRMaterial;
            if (rhs == null)
            {
                return false;
            }

            return this.Equals(rhs);
        }
    }
}
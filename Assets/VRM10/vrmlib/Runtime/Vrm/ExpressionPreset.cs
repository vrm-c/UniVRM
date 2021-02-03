using System.Collections.Generic;
using System.Linq;

namespace VrmLib
{
    public class ExpressionPresetMigrationStringAttribute : System.Attribute
    {
        /// <summary>
        /// vrm-0.X での名前
        /// </summary>
        public string Vrm0;

        public ExpressionPresetMigrationStringAttribute(string name)
        {
            Vrm0 = name;
        }
    }

    /// <summary>
    /// VRM-1.0 順に並べ替え。
    /// 
    /// VRM-0.X とは変換表が必用デス
    /// </summary>
    public enum ExpressionPreset
    {
        [ExpressionPresetMigrationString("unknown")]
        Custom,
        // 喜怒哀楽驚
        [ExpressionPresetMigrationString("joy")]
        Happy,
        [ExpressionPresetMigrationString("angry")]
        Angry,
        [ExpressionPresetMigrationString("sorrow")]
        Sad,
        [ExpressionPresetMigrationString("fun")]
        Relaxed,
        [ExpressionPresetMigrationString(null)]
        Surprised,
        // Procedural(LipSync)
        [ExpressionPresetMigrationString("a")]
        Aa,
        [ExpressionPresetMigrationString("i")]
        Ih,
        [ExpressionPresetMigrationString("u")]
        Ou,
        [ExpressionPresetMigrationString("e")]
        Ee,
        [ExpressionPresetMigrationString("o")]
        Oh,
        // Procedural(Blink)
        [ExpressionPresetMigrationString("blink")]
        Blink,
        [ExpressionPresetMigrationString("blink_l")]
        BlinkLeft,
        [ExpressionPresetMigrationString("blink_r")]
        BlinkRight,
        // Procedural(LookAt)
        [ExpressionPresetMigrationString("lookup")]
        LookUp,
        [ExpressionPresetMigrationString("lookdown")]
        LookDown,
        [ExpressionPresetMigrationString("lookleft")]
        LookLeft,
        [ExpressionPresetMigrationString("lookright")]
        LookRight,
        // other
        [ExpressionPresetMigrationString("neutral")]
        Neutral,
    }

    public static class ExpressionPresetMigration
    {
        static readonly Dictionary<string, ExpressionPreset> s_map = GetValues().ToDictionary(x => x.Key, x => x.Value);

        static IEnumerable<KeyValuePair<string, ExpressionPreset>> GetValues()
        {
            var t = typeof(ExpressionPreset);
            foreach (var x in EnumUtil.Values<ExpressionPreset>())
            {
                var mi = t.GetMember(x.ToString()).FirstOrDefault(m => m.DeclaringType == t);
                var attr = mi.GetCustomAttributes(typeof(ExpressionPresetMigrationStringAttribute), true).First();
                if (attr is ExpressionPresetMigrationStringAttribute vrmAttr && !string.IsNullOrEmpty(vrmAttr.Vrm0))
                {
                    yield return new KeyValuePair<string, ExpressionPreset>(vrmAttr.Vrm0, x);
                }
            }
        }

        public static ExpressionPreset FromVrm0String(string src)
        {
            if (s_map.TryGetValue(src, out ExpressionPreset preset))
            {
                return preset;
            }
            else
            {
                return ExpressionPreset.Custom;
            }
        }
    }
}

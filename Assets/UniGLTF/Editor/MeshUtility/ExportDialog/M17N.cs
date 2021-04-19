using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace MeshUtility.M17N
{
    /// <summary>
    /// 多言語対応
    /// </summary>
    public enum Languages
    {
        ja,
        en,
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class LangMsgAttribute : System.Attribute
    {
        public Languages Language;
        public string Message;

        public LangMsgAttribute(Languages language, string msg)
        {
            Language = language;
            Message = msg;
        }
    }

    public static class MsgCache<T> where T : Enum
    {
        static Dictionary<Languages, Dictionary<T, string>> s_cache = new Dictionary<Languages, Dictionary<T, string>>();

        static LangMsgAttribute GetAttribute(T value, Languages language)
        {
            var t = typeof(T);
            var memberInfos = t.GetMember(value.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == t);
            var attr = enumValueMemberInfo.GetCustomAttributes(typeof(LangMsgAttribute), false).Select(x => (LangMsgAttribute)x).ToArray();
            if (attr == null || attr.Length == 0)
            {
                return null;
            }
            var match = attr.FirstOrDefault(x => x.Language == language);
            if (match != null)
            {
                return match;
            }
            return attr.First();
        }

        public static string Get(Languages language, T key)
        {
            if (!s_cache.TryGetValue(language, out Dictionary<T, string> map))
            {
                map = new Dictionary<T, string>();

                var t = typeof(T);
                foreach (T value in Enum.GetValues(t))
                {
                    var match = GetAttribute(value, language);
                    // Attribute。無かったら enum の ToString
                    map.Add(value, match != null ? match.Message : key.ToString());
                }

                s_cache.Add(language, map);
            }
            return map[key];
        }
    }
    public static class Getter
    {
        const string LANG_KEY = "VRM_LANG";

        static Languages? m_lang;

        public static Languages Lang
        {
            get
            {
                return m_lang.GetValueOrDefault();
            }
        }

        public static string Msg<T>(this T key) where T : Enum
        {
            return M17N.MsgCache<T>.Get(Lang, key);
        }

        public static void OnGuiSelectLang()
        {
            var lang = (M17N.Languages)EditorGUILayout.EnumPopup("lang", Lang);
            if (lang != Lang)
            {
                m_lang = lang;
                EditorPrefs.SetString(LANG_KEY, M17N.Getter.Lang.ToString());
            }
        }
    }
}

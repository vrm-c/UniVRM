using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UniGLTF
{
    public enum EnumSerializationType
    {
        AsInt,
        AsString,
        AsLowerString,
        AsUpperString,
    }

    public class BaseJsonSchemaAttribute : Attribute
    {
        #region Annotation
        public string Title;
        public string Description;
        #endregion

        #region integer, number
        public double Minimum = double.NaN;
        public bool ExclusiveMinimum;
        public double Maximum = double.NaN;
        public bool ExclusiveMaximum;
        public double MultipleOf;
        #endregion

        #region string
        public string Pattern;
        #endregion

        #region array
        public int MinItems;
        public int MaxItems;
        #endregion

        #region object
        public UniJSON.ValueNodeType ValueType;
        public int MinProperties;
        public bool Required;
        public string[] Dependencies;
        #endregion

        #region enum
        public EnumSerializationType EnumSerializationType;
        public object[] EnumValues;
        public object[] EnumExcludes;
        #endregion

        // シリアライズ時の除外条件をハードコーディングする
        public string[] SerializationConditions;

        // public PropertyExportFlags ExportFlags = PropertyExportFlags.Default;

        /// <summary>
        /// skip validator comparison
        /// </summary>
        public bool SkipSchemaComparison;

        /// <summary>
        /// Suppress errors if a value of the field which is not required by a schema is matched to this value.
        /// This feature will be useful to ignore invalid value which is known.
        /// </summary>
        public object ExplicitIgnorableValue;

        /// <summary>
        /// Suppress errors if length of a value of the field which is not required by a schema is matched to this value.
        /// This feature will be useful to ignore invalid value which is known.
        /// </summary>
        public int ExplicitIgnorableItemLength = -1;

        public void Merge(BaseJsonSchemaAttribute rhs)
        {
            if (rhs == null) return;

            if (string.IsNullOrEmpty(Title))
            {
                Title = rhs.Title;
            }
        }

        public virtual string GetInfo(FieldInfo fi)
        {
            return "";
        }

        public static bool IsNumber(Type t)
        {
            if (t == typeof(sbyte)
            || t == typeof(short)
            || t == typeof(int)
            || t == typeof(long)
            || t == typeof(byte)
            || t == typeof(ushort)
            || t == typeof(uint)
            || t == typeof(ulong)
            || t == typeof(float)
            || t == typeof(double)
            )
            {
                return true;
            }

            return false;
        }

        public static string GetTypeName(Type t)
        {
            if (t.IsArray)
            {
                return t.GetElementType().Name + "[]";
            }
            else if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return "List<" + t.GetGenericArguments()[0] + ">";
                }
                else if (t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return "Dictionary<" + string.Join(", ", t.GetGenericArguments().Select(x => x.Name).ToArray()) + ">";
                }
            }

            return t.Name;
        }

    }

    public class JsonSchemaAttribute : BaseJsonSchemaAttribute
    {
        public override string GetInfo(FieldInfo fi)
        {
            if (IsNumber(fi.FieldType))
            {
                var sb = new StringBuilder();
                if (!double.IsNaN(Minimum) && !double.IsNaN(Maximum))
                {
                    sb.Append(string.Format("{0} <= N <= {1}", Minimum, Maximum));
                }
                else if (!double.IsNaN(Minimum))
                {
                    sb.Append(string.Format("{0} <= N", Minimum));
                }
                else if (!double.IsNaN(Maximum))
                {
                    sb.Append(string.Format("N <= {0}", Maximum));
                }
                return sb.ToString();
            }
            else
            {
                if (EnumValues != null)
                {
                    return string.Join(", ", EnumValues.Select(x => x.ToString()).ToArray());
                }
                else
                {
                    return GetTypeName(fi.FieldType);
                }
            }
        }
    }

    public class ItemJsonSchemaAttribute : BaseJsonSchemaAttribute
    {
        public override string GetInfo(FieldInfo fi)
        {
            var sb = new StringBuilder();
            sb.Append(GetTypeName(fi.FieldType));
            if (!double.IsNaN(MinItems) && !double.IsNaN(MaxItems))
            {
                sb.Append(string.Format("{0} < N < {1}", MinItems, MaxItems));
            }
            else if (!double.IsNaN(MinItems))
            {
                sb.Append(string.Format("{0}< N", MinItems));
            }
            else if (!double.IsNaN(MaxItems))
            {

            }
            return sb.ToString();
        }
    }
}

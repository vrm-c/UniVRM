using System;


namespace UniJSON
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
        public ValueNodeType ValueType;
        public int MinProperties;
        public bool Required;
        public string[] Dependencies;
        #endregion

        #region enum
        public EnumSerializationType EnumSerializationType;
        public object[] EnumValues;
        public object[] EnumExcludes;
        #endregion

        public PropertyExportFlags ExportFlags = PropertyExportFlags.Default;

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
    }

    public class JsonSchemaAttribute : BaseJsonSchemaAttribute { }

    public class ItemJsonSchemaAttribute : BaseJsonSchemaAttribute { }
}

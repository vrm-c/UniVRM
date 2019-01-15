using System;
using System.Collections.Generic;

namespace UniJSON
{
    public class JsonSchemaValidationContext
    {
        Stack<string> m_stack = new Stack<string>();

        public bool EnableDiagnosisForNotRequiredFields = false;

        public JsonSchemaValidationContext(object o)
        {
            Push(o.GetType().Name);
        }

        public ActionDisposer Push(object o)
        {
            m_stack.Push(o.ToString());
            return new ActionDisposer(Pop);
        }

        public void Pop()
        {
            m_stack.Pop();
        }

        public bool IsEmpty()
        {
            return m_stack.Count == 1; // A first element will be remained.
        }

        public override string ToString()
        {
            return string.Join(".", m_stack.ToArray(), 0, m_stack.Count);
        }
    }


    public class JsonSchemaValidationException : Exception
    {
        public Exception Error
        {
            get; private set;
        }

        public JsonSchemaValidationException(JsonSchemaValidationContext context, string msg) : base(string.Format("[{0}] {1}", context, msg))
        {
        }

        public JsonSchemaValidationException(JsonSchemaValidationContext context, Exception ex) : base(string.Format("[{0}] {1}", context, ex))
        {
            Error = ex;
        }
    }


    public interface IJsonSchemaValidator
    {
        #region JsonSchema
        void Merge(IJsonSchemaValidator rhs);

        /// <summary>
        /// Parse json schema
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value);

        void ToJsonScheama(IFormatter f);
        #endregion

        #region Serializer
        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        /// <returns>return null if validate value</returns>
        JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext context, T value);

        void Serialize<T>(IFormatter f, JsonSchemaValidationContext context, T value);

        void Deserialize<T, U>(ListTreeNode<T> src, ref U dst) where T : IListTreeItem, IValue<T>;
        #endregion
    }
}

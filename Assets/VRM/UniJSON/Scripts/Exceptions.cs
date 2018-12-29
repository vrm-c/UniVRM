using System;


namespace UniJSON
{
    public class TreeValueException : ArgumentException
    {
        protected TreeValueException(string msg) : base(msg) { }
    }

    /// <summary>
    ///Exception failure
    /// </summary>
    public class ParserException : TreeValueException
    {
        public ParserException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Successfully parsed, but fail to getValue
    /// </summary>
    public class DeserializationException : TreeValueException
    {
        public DeserializationException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Formatter exception. key value violation
    /// </summary>
    public class FormatterException : TreeValueException
    {
        public FormatterException(string msg) : base(msg) { }
    }
}

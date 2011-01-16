using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    /// <summary>
    /// This is an object you can pass in instead of a table column to provide a literal value
    /// </summary>
    public class LiteralQueryParam
    {
        internal object m_literalObject = string.Empty;
        public const string STRING_LITERAL_LEFT_NAME = @"SubSonicLiteralLeft";
        private const string STRING_LITERAL_RIGHT_NAME = @"SubSonicLiteralRight";


        /// <summary>
        /// Creates a new string literal to be used on the left side of a <see cref="Constraint"/>
        /// </summary>
        /// <param name="objectToParse">The obect to call .ToString() on</param>
        public LiteralQueryParam(object objectToParse)
        {
            m_literalObject = objectToParse;
        }

        /// <summary>
        /// The literal string to be used
        /// </summary>
        public string ColumnValue
        {
            get { return m_literalObject == null ? string.Empty : m_literalObject.ToString(); }
        }

        public string ColumnName
        {
            get { return STRING_LITERAL_LEFT_NAME; }
        }

        public string ParameterName
        {
            get { return STRING_LITERAL_RIGHT_NAME; }
        }
    }
}

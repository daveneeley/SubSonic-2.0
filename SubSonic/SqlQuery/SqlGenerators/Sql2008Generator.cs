/*
 * SubSonic - http://subsonicproject.com
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an 
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
*/

using System.Data;

namespace SubSonic
{
    /// <summary>
    /// 
    /// </summary>
    public class Sql2008Generator : Sql2005Generator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sql2008Generator"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public Sql2008Generator(SqlQuery query)
            : base(query) {}

        /// <summary>
        /// Gets the type of the native.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        protected override string GetNativeType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Object:
                case DbType.AnsiString:
                    return "varchar";
                case DbType.AnsiStringFixedLength:
                    return "char";
                case DbType.String:
                    return "nvarchar";
                case DbType.StringFixedLength:
                    return "nchar";
                case DbType.Boolean:
                    return "bit";
                case DbType.SByte:
                case DbType.Binary:
                case DbType.Byte:
                    return "tinyint";
                case DbType.Currency:
                    return "money";
                case DbType.Time:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "datetime";
                case DbType.Decimal:
                    return "decimal";
                case DbType.Double:
                    return "float";
                case DbType.Guid:
                    return "uniqueidentifier";
                case DbType.UInt32:
                case DbType.UInt16:
                case DbType.Int16:
                case DbType.Int32:
                    return "int";
                case DbType.UInt64:
                case DbType.Int64:
                    return "bigint";
                case DbType.Single:
                    return "real";
                case DbType.VarNumeric:
                    return "numeric";
                case DbType.Xml:
                    return "xml";
                default:
                    return "nvarchar";
            }
        }
    }
}
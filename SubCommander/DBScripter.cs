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

using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SubSonic.SubCommander
{
    /// <summary>
    /// 
    /// </summary>
    public class DBScripter
    {
        /// <summary>
        /// Scripts the data.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public static Dictionary<string, StringBuilder> ScriptData(string providerName)
        {
            string[] tables = DataService.GetOrderedTableNames(providerName);
            Dictionary<string, StringBuilder> results = new Dictionary<string, StringBuilder>(tables.Length);

            foreach (string tbl in tables)
            {
                if (!CodeService.ShouldGenerate(tbl, providerName))
                    continue;

                SubSonic.TableSchema.Table schema = Query.BuildTableSchema(tbl, providerName);
                try
                {
                    int records = DataService.GetRecordCount(new Query(schema));

                    if (records > 200000)
                    {
                        Utilities.Utility.WriteTrace(string.Format("Table {0} has {1} records, This may take a while...", tbl, records));
                        //continue;
                    }

                    Utilities.Utility.WriteTrace(string.Format("Scripting Table Data: {0}", tbl));
                    Dictionary<string, StringBuilder> dataScript = DataService.ScriptData(tbl, providerName);
                    foreach (KeyValuePair<string, StringBuilder> kvp in dataScript)
                    {
                        results.Add(kvp.Key, kvp.Value);
                    }                    
                }
                catch (Exception ex)
                {
                    Utilities.Utility.WriteTrace(string.Format("Scripting failed for table {0}. Error: {1}, Trace: {2}", tbl, ex.Message, ex.StackTrace));
                }
            }

            return results;
        }

        /// <summary>
        /// Scripts the schema.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A Hashtable containing a list of table names and their corresponding StringBuilder</returns>
        public static Dictionary<string, StringBuilder> ScriptSchema(string connectionString, DataProvider provider, bool oneFile)
        {
            StringBuilder result = new StringBuilder();

            SqlConnection conn = new SqlConnection(connectionString);
            SqlConnectionStringBuilder cString = new SqlConnectionStringBuilder(connectionString);
            ServerConnection sconn = new ServerConnection(conn);
            Server server = new Server(sconn);
            Database db = server.Databases[cString.InitialCatalog];

            Dictionary<string, StringBuilder> dict = new Dictionary<string, StringBuilder>();

            if (oneFile)
            {
                Transfer trans = new Transfer(db);

                //set the objects to copy
                trans.CopyAllTables = false;
                trans.CopyAllDefaults = false;
                trans.CopyAllUserDefinedFunctions = true;//we don't have logic in SubSonic to decide which ones should or should not be generated, so better to be safe.
                trans.CopyAllStoredProcedures = false;
                trans.CopyAllViews = false;
                trans.CopySchema = false;
                trans.CopyAllLogins = false;

                foreach (Table tbl in db.Tables)
                {
                    if (!CodeService.ShouldGenerate(tbl.Name, provider.Name))
                        continue;
                    Utilities.Utility.WriteTrace(string.Format("Adding table {0}", tbl.Name));
                    trans.ObjectList.Add(tbl);
                }
                foreach (View v in db.Views)
                {
                    if (!CodeService.ShouldGenerate(v.Name, provider.Name))
                        continue;
                    Utilities.Utility.WriteTrace(string.Format("Adding view {0}", v.Name));
                    trans.ObjectList.Add(v);
                }
                foreach (Microsoft.SqlServer.Management.Smo.StoredProcedure sp in db.StoredProcedures)
                {
                    if (!provider.UseSPs || !CodeService.ShouldGenerate(sp.Name, provider.IncludeProcedures, provider.ExcludeProcedures, provider))
                        continue;
                    Utilities.Utility.WriteTrace(string.Format("Adding sproc {0}", sp.Name));
                    trans.ObjectList.Add(sp);
                }

                trans.CopyData = false;
                trans.DropDestinationObjectsFirst = true;
                trans.UseDestinationTransaction = true;

                trans.Options.AnsiFile = true;
                trans.Options.ScriptBatchTerminator = true;
                trans.Options.WithDependencies = true; //if this setting is false and you get an error, try installing SQL Server 2008 SP1 cumulative update 5 or higher..see http://support.microsoft.com/kb/976413
                trans.Options.DriAll = true;
                trans.Options.IncludeHeaders = false;
                trans.Options.IncludeIfNotExists = true;
                trans.Options.SchemaQualify = true;

                Utilities.Utility.WriteTrace("Scripting objects...");

                StringCollection script = trans.ScriptTransfer();

                foreach (string s in script)
                    result.AppendLine(s);
                result.AppendLine();
                result.AppendLine();

                dict.Add(provider.Name, result);
                return dict;
            }
            else 
            { 
                //use this method to append single tables and all of their dependencies one at a time
                Scripter scr = new Scripter(server);
                scr.Options.AnsiFile = true;
                scr.Options.ClusteredIndexes = true;
                scr.Options.DriAll = true;
                scr.Options.IncludeHeaders = false;
                scr.Options.IncludeIfNotExists = true;
                scr.Options.SchemaQualify = true;
                scr.Options.WithDependencies = false;

                UrnCollection u = new UrnCollection();
                foreach (Table tbl in db.Tables)
                {
                    if (CodeService.ShouldGenerate(tbl.Name, provider.Name))
                    {
                        u = new UrnCollection();
                        u.Add(tbl.Urn);
                        if (!tbl.IsSystemObject)
                        {
                            Utilities.Utility.WriteTrace(string.Format("Adding table {0}", tbl.Name));
                            result = new StringBuilder();
                            StringCollection sc = scr.Script(u);
                            foreach (string s in sc)
                                result.AppendLine(s);

                            dict.Add(string.Concat("Table_",tbl.Name), result);
                        }
                    }
                }
                foreach (View v in db.Views)
                {
                    if (CodeService.ShouldGenerate(v.Name, provider.Name))
                    {
                        u = new UrnCollection();
                        u.Add(v.Urn);
                        if (!v.IsSystemObject)
                        {
                            Utilities.Utility.WriteTrace(string.Format("Adding view {0}", v.Name));
                            result = new StringBuilder();
                            StringCollection sc = scr.Script(u);
                            foreach (string s in sc)
                                result.AppendLine(s);

                            dict.Add(string.Concat("View_",v.Name), result);
                        }
                    }
                }

                foreach (Microsoft.SqlServer.Management.Smo.StoredProcedure sp in db.StoredProcedures)
                {
                    if (CodeService.ShouldGenerate(sp.Name, provider.IncludeProcedures, provider.ExcludeProcedures, provider))
                    {
                        u = new UrnCollection();
                        u.Add(sp.Urn);
                        if (!sp.IsSystemObject)
                        {
                            Utilities.Utility.WriteTrace(string.Format("Adding sproc {0}", sp.Name));
                            result = new StringBuilder();
                            StringCollection sc = scr.Script(u);
                            foreach (string s in sc)
                                result.AppendLine(s);

                            dict.Add(string.Concat("Sproc_",sp.Name), result);
                        }
                    }
                }

                foreach (UserDefinedFunction udf in db.UserDefinedFunctions)
                {
                    if (CodeService.ShouldGenerate(udf.Name, provider.IncludeProcedures, provider.ExcludeProcedures, provider))
                    {
                        u = new UrnCollection();
                        u.Add(udf.Urn);
                        if (!udf.IsSystemObject)
                        {
                            Utilities.Utility.WriteTrace(string.Format("Adding udf {0}", udf.Name));
                            result = new StringBuilder();
                            StringCollection sc = scr.Script(u);
                            foreach (string s in sc)
                                result.AppendLine(s);

                            dict.Add(string.Concat("UDF_", udf.Name), result);
                        }
                    }
                }

                return dict;
            }
        }
    }
}
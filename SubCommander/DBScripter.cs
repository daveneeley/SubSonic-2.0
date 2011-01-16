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
        public static string ScriptData(string tableName, string providerName)
        {
            return DataService.ScriptData(tableName, providerName);
        }

        /// <summary>
        /// Scripts the schema.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static string ScriptSchema(string connectionString, DataProvider provider)
        {
            StringBuilder result = new StringBuilder();

            SqlConnection conn = new SqlConnection(connectionString);
            SqlConnectionStringBuilder cString = new SqlConnectionStringBuilder(connectionString);
            ServerConnection sconn = new ServerConnection(conn);
            Server server = new Server(sconn);
            Database db = server.Databases[cString.InitialCatalog];
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
            trans.Options.WithDependencies = false; //there is an error if you are running SQL Server 2008 SP1 that requires cumulative update 5 or higher..see http://support.microsoft.com/kb/976413
            trans.Options.DriAll = false;
            trans.Options.IncludeHeaders = false;
            trans.Options.IncludeIfNotExists = true;
            trans.Options.SchemaQualify = true;

            Utilities.Utility.WriteTrace("Scripting objects...");

            StringCollection script = trans.ScriptTransfer();

            foreach (string s in script)
                result.AppendLine(s);


            ////use this method to append single tables and all of their dependencies one at a time
            ////the downside to this method is that dependent tables will once for each table that requires it
            //Scripter scr = new Scripter(server);
            //scr.Options.AnsiFile = true;
            //scr.Options.ClusteredIndexes = true;
            //scr.Options.DriAll = true;
            //scr.Options.IncludeHeaders = false;
            //scr.Options.IncludeIfNotExists = true;
            //scr.Options.SchemaQualify = true;
            //scr.Options.WithDependencies = true;

            //UrnCollection u = new UrnCollection();
            //foreach (Table tbl in db.Tables)
            //{
            //    if (CodeService.ShouldGenerate(tbl.Name, provider.Name))
            //    {
            //        u = new UrnCollection();
            //        u.Add(tbl.Urn);
            //        if (!tbl.IsSystemObject)
            //        {
            //            StringCollection sc = scr.Script(u);
            //            foreach (string s in sc)
            //                result.AppendLine(s);
            //        }
            //    }
            //}

            
            result.AppendLine();
            result.AppendLine();

            return result.ToString();
        }
    }
}
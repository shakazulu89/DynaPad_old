using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;

namespace DynaPad
{
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    public class SqlFunctions
    {
        //[Attributes.MethodDescription("Sql Paramaterized Select")]
        public static object SqlSelect(string query, SqlConnection sqlCon, List<SqlParameter> parameters)
        {

            //Globals.Monitoring.AddUserAction("SQL FUNCTION", //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(query, parameters), "SQL SELECT");
            //EXAMPLE USAGE
            ////List<SqlParameter> sp = new List<SqlParameter>()
            //{
            //    new SqlParameter() {ParameterName = "@numClientID", Value= 1}
            //};
            //var test = SqlFunctions.SqlSelect("select top 1 * from clients where numclientid = @numClientID", conn, sp);

            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand selectCommand = new SqlCommand(query, sqlCon);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    selectCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                }
            }
            selectCommand.CommandTimeout = 60;
            adapter.SelectCommand = selectCommand;

            DataTable myDataTable = new DataTable();

            try
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                adapter.Fill(myDataTable);
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                ////Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "", "SQL Select error.");
                throw;
            }
            finally
            {
                adapter.Dispose();
                selectCommand.Dispose();
                sqlCon.Close();
            }

            return myDataTable;
        }
        //[Attributes.MethodDescription("Sql Select")]
        public static object SqlSelect(string query, SqlConnection sqlCon)
        {
            //Globals.Monitoring.AddUserAction("SQL FUNCTION", query, "SQL SELECT");

            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand selectCommand = new SqlCommand(query, sqlCon);
            selectCommand.CommandTimeout = 60;
            adapter.SelectCommand = selectCommand;

            DataTable myDataTable = new DataTable();

            try
            {

                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                adapter.Fill(myDataTable);
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                ////Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "", "SQL Select error.");
                throw;
            }
            finally
            {
                adapter.Dispose();
                selectCommand.Dispose();
                sqlCon.Close();
            }

            return myDataTable;
        }

        //[Attributes.MethodDescription("Sql Paramaterized Select Reader")]
        public static SqlDataReader SqlSelectReader(string query, SqlConnection sqlCon, List<SqlParameter> parameters)
        {

            //Globals.Monitoring.AddUserAction("SQL FUNCTION", //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(query, parameters), "SQL SELECT READER");
            //EXAMPLE USAGE
            ////List<SqlParameter> sp = new List<SqlParameter>()
            //{
            //    new SqlParameter() {ParameterName = "@numClientID", Value= 1}
            //};
            //var test = SqlFunctions.SqlSelect("select top 1 * from clients where numclientid = @numClientID", conn, sp);

            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand selectCommand = new SqlCommand(query, sqlCon);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    selectCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                }
            }
            selectCommand.CommandTimeout = 60;
            adapter.SelectCommand = selectCommand;

            SqlDataReader myDataReader = null;

            try
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                myDataReader = adapter.SelectCommand.ExecuteReader();
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                //Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "", "SQL Select Reader error.");
                throw;
            }
            finally
            {
                adapter.Dispose();
                selectCommand.Dispose();
                sqlCon.Close();
            }

            return myDataReader;
        }
        //[Attributes.MethodDescription("Sql Select Reader")]
        public static object SqlSelectReader(string query, SqlConnection sqlCon)
        {
            //Globals.Monitoring.AddUserAction("SQL FUNCTION", query, "SQL SELECT READER");

            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand selectCommand = new SqlCommand(query, sqlCon);
            selectCommand.CommandTimeout = 60;
            adapter.SelectCommand = selectCommand;

            SqlDataReader myDataReader = null;

            try
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                myDataReader = adapter.SelectCommand.ExecuteReader();
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                //Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "", "SQL Select Reader error.");
                throw;
            }
            finally
            {
                adapter.Dispose();
                selectCommand.Dispose();
                //sqlCon.Close(); // causing reader to go bad...
            }

            return myDataReader;
        }
        //[Attributes.MethodDescription("Sql Delete")]
        public static object SqlDelete(string query, string sqlTable, string identityColumn, string uniqueId, SqlConnection sqlCon, List<SqlParameter> parameters)
        {
            //Globals.Monitoring.AddUserAction("SQL FUNCTION", //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(query, parameters), "SQL DELETE");
            
            SqlDataAdapter adapter = new SqlDataAdapter();

            if (sqlTable == "Clients" || sqlTable == "DoctorListing" || sqlTable == "ClientFiles")
            {
                SqlCommand updateCommand = new SqlCommand("UPDATE " + sqlTable + " SET AutoAudit_ModifiedDate = @AutoAudit_ModifiedDate, AutoAudit_ModifiedBy = @AutoAudit_ModifiedBy WHERE " + identityColumn + " = @" + identityColumn, sqlCon);
                updateCommand.Parameters.AddWithValue("AutoAudit_ModifiedDate", DateTime.Now);
				updateCommand.Parameters.AddWithValue("AutoAudit_ModifiedBy", "1");//Globals.UserDetails.UserId);
                updateCommand.Parameters.AddWithValue(identityColumn, uniqueId);
                updateCommand.CommandTimeout = 60;
                adapter.UpdateCommand = updateCommand;
                adapter = new SqlDataAdapter();
            }
            
            //DoctorListing.AutoAudit_CreatedDate, DoctorListing.AutoAudit_CreatedBy, DoctorListing.AutoAudit_ModifiedDate, DoctorListing.AutoAudit_ModifiedBy
            SqlCommand deleteCommand = new SqlCommand(query, sqlCon);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    deleteCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                }
            }
            deleteCommand.CommandTimeout = 60;
            adapter.DeleteCommand = deleteCommand;

            DataTable myDataTable = new DataTable();

            try
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                adapter.Fill(myDataTable);
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                //Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "", "SQL Delete error.");
                throw;
            }
            finally
            {
                adapter.Dispose();
                deleteCommand.Dispose();
                sqlCon.Close();
            }

            return myDataTable;
        }

        //[Attributes.MethodDescription("Sql Insert")]
        public static string SqlInsert(string query, string sqlTable, string identityColumn, string uniqueId, SqlConnection sqlCon, List<SqlParameter> parameters, SqlTransaction transaction = null)
        {
            //DoctorListing.AutoAudit_CreatedDate, DoctorListing.AutoAudit_CreatedBy, DoctorListing.AutoAudit_ModifiedDate, DoctorListing.AutoAudit_ModifiedBy
            
            query = BuildInsertSQL(parameters, sqlTable);
            //Globals.Monitoring.AddUserAction("SQL FUNCTION",
                //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(query, parameters),
                //"SQL INSERT");

            SqlCommand insertCommand = new SqlCommand(query + ";SELECT SCOPE_IDENTITY() AS [SCOPE_IDENTITY];", sqlCon);
            if (transaction != null)
            {
                insertCommand.Transaction = transaction;
            }
            string identity = "";
            try
            {
                if (parameters != null)
                {
                    foreach (SqlParameter param in parameters)
                    {
                        insertCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue ?? DBNull.Value);
                    }
                }
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                identity = insertCommand.ExecuteScalar().ToString();
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                //Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "",
                   // "SQL Insert error.");
                throw;
            }
            finally
            {
                insertCommand.Dispose();
                sqlCon.Close();
            }

            return identity;
        }
        

        public static string BuildInsertSQL(List<SqlParameter> sqlParams, string tableName)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO " + tableName + " (");
            StringBuilder values = new StringBuilder("VALUES (");
            bool bFirst = true;
            //bool bIdentity = false;
            //string identityType = null;

            foreach (SqlParameter param in sqlParams)
            {
                if (bFirst)
                    bFirst = false;
                else
                {
                    sql.Append(", ");
                    values.Append(", ");
                }

                sql.Append(param.ParameterName);
                values.Append("@");
                values.Append(param.ParameterName);

            }
            sql.Append(") ");
            sql.Append(values);
            sql.Append(")");

            //if (bIdentity)
            //{
            //    sql.Append("; SELECT CAST(scope_identity() AS ");
            //    sql.Append(identityType);
            //    sql.Append(")");
            //}

            return sql.ToString();
        }

        // ** RULES
        // Last Paramater must be identity 
        // 
        //[Attributes.MethodDescription("Sql Update - Dashboard")]
        public static object SqlUpdate(string sqlTable, string identityColumn, string uniqueId, SqlConnection sqlCon, List<SqlParameter> parameters, string query = "") //, List<SqlParameter> parametersClaimant, List<SqlParameter> parametersListing
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand updateCommand = new SqlCommand(query, sqlCon);
            DataTable myDataTable = new DataTable();
            try
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }

                if (string.IsNullOrEmpty(query)) {query = "UPDATE " + sqlTable + " SET ";}

                if (parameters.Count > 0)
                {
                    // ** If table is audited add audit fields! ** //
                    if (sqlTable == "Clients" || sqlTable == "DoctorListing" || sqlTable == "ClientFiles")
                    {
                        parameters.Add(new SqlParameter("@AutoAudit_ModifiedDate", DateTime.Now));
						parameters.Add(new SqlParameter("@AutoAudit_ModifiedBy", "1"));//Globals.UserDetails.UserId));
                    }

                    foreach (SqlParameter param in parameters)
                    {
                        updateCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                        if (param.ParameterName == "@" + identityColumn) continue;
                        query += param.ParameterName.Replace("@", "") + " = " + param.ParameterName + ",";
                    }
                    query = query.TrimEnd(',') + " WHERE " + identityColumn + " = @" + identityColumn;
                    updateCommand.CommandText = query;

                    updateCommand.Parameters.AddWithValue("@" + identityColumn, uniqueId);

                    //Globals.Monitoring.AddUserAction("SQL FUNCTION",
                        //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(updateCommand.CommandText, parameters),
                        //"SQL UPDATE");
                    updateCommand.ExecuteNonQuery();
                }

                //if (parametersClaimant.Count > 1)
                //{
                //    parametersClaimant.Add(new SqlParameter("@AutoAudit_ModifiedDate", DateTime.Now));
                //    parametersClaimant.Add(new SqlParameter("@AutoAudit_ModifiedBy", //Globals.UserDetails.UserId));
                    

                //    query = "UPDATE Clients SET ";
                //    foreach (SqlParameter param in parametersClaimant)
                //    {
                //        updateCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                //        if (param.ParameterName == "@numClientID") continue;
                //        query += param.ParameterName.Replace("@", "") + " = " + param.ParameterName + ",";
                //    }
                //    query = query.TrimEnd(',') + " WHERE numClientID = @numClientID";
                //    updateCommand.CommandText = query;
                //    //updateCommand.Parameters.AddWithValue(identityColumn, uniqueId);

                //    parametersClaimant.Add(new SqlParameter(identityColumn, uniqueId));

                //    //Globals.Monitoring.AddUserAction("SQL FUNCTION",
                //        //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(query, parametersClaimant),
                //        "SQL UPDATE");
                //    updateCommand.ExecuteNonQuery();

                //}
                //if (parametersListing.Count > 1)
                //{
                //    updateCommand = new SqlCommand("", sqlCon);

                //    parametersListing.Add(new SqlParameter("@AutoAudit_ModifiedDate", DateTime.Now));
                //    parametersListing.Add(new SqlParameter("@AutoAudit_ModifiedBy", //Globals.UserDetails.UserId));

                //    query = "UPDATE DoctorListing SET ";
                //    foreach (SqlParameter param in parametersListing)
                //    {
                //        updateCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                //        if (param.ParameterName == "@numListingID") continue;
                //        query += param.ParameterName.Replace("@", "") + " = " + param.ParameterName + ",";
                //    }
                //    query = query.TrimEnd(',') + " WHERE numListingID = @numListingID";
                //    updateCommand.CommandText = query;
                //    //updateCommand.Parameters.AddWithValue(identityColumn, uniqueId);

                //    parametersListing.Add(new SqlParameter(identityColumn, uniqueId));
                //    //Globals.Monitoring.AddUserAction("SQL FUNCTION",
                //        //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(query, parametersClaimant),
                //        "SQL UPDATE");
                //    updateCommand.ExecuteNonQuery();
                //}
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                //Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "",
                  //  "SQL Update error.");
                throw;
            }
            finally
            {
                adapter.Dispose();
                updateCommand.Dispose();
                sqlCon.Close();
            }

            return myDataTable;
        }

        //[Attributes.MethodDescription("Sql Procedure")]
        public static object SqlStoredProcedure(string procedureName, SqlConnection sqlCon, List<SqlParameter> parameters)
        {
            SqlCommand procedureCommand = new SqlCommand(procedureName, sqlCon);
            procedureCommand.CommandType = CommandType.StoredProcedure;
            procedureCommand.CommandTimeout = 60;

            //Globals.Monitoring.AddUserAction("SQL FUNCTION", //Globals.Functions.GenericFunctions.ConvertCommandParamatersToLiteralValues(procedureCommand.CommandText, parameters), procedureName);
            //Handle rules and old structure maintainability
            if (procedureName == "DoctorListing_Upsert")
            {
                parameters = HandleListingUpsertRules(parameters);
            }

            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    procedureCommand.Parameters.AddWithValue(param.ParameterName, param.SqlValue);
                }
                //procedureCommand.Parameters.Add("")
            }
            string result = "";
            try
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    sqlCon.Open();
                }
                result = procedureCommand.ExecuteNonQuery().ToString();
            }
            catch (SqlException sqlEx)
            {
                //Globals.Monitoring.CurrentException = sqlEx;
                //Globals.Monitoring.ErrorOccured = true;
                //Globals.Functions.ErrorHandling.HandleError(sqlEx, HttpContext.Current.Request.Url.AbsoluteUri, "", "SQL Stored Procedure error.");
                throw;
            }
            finally
            {
                sqlCon.Close();
            }

            return result;
        }

        private static List<SqlParameter> HandleListingUpsertRules(List<SqlParameter> parameters)
        {
            string caseType = (string)parameters.Find(item => item.ParameterName == "numCaseType").Value;
            string Status = (string)parameters.Find(item => item.ParameterName == "numStatus").Value;
            string subStatus = (string) parameters.Find(item => item.ParameterName == "numSubStatus").Value;


            switch (caseType)
            {
                case "1": // IME
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "Pending";
                    break;
                case "2": // Peer
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "Peer Review";
                    break;
                case "3": // File Review
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "File Review";
                    break;
                case "4": // Testimony
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "Testimony";
                    break;
                case "5": // RAD Review
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "Rad Review";
                    break;
            }

            switch (Status)
            {
                case "1": // Awaiting Report
                    parameters.Find(item => item.ParameterName == "flgKept").Value = "1";
                    break;
                case "2": // Report in QA
                    parameters.Find(item => item.ParameterName == "flgRrpt").Value = "1";
                    break;
                case "3": // Ready to Bill
                    parameters.Find(item => item.ParameterName == "flgReadyToBill").Value = "1";
                    break;
                case "4": // Completed
                    parameters.Find(item => item.ParameterName == "flgBilled").Value = "1";
                    break;
                case "5": // Paid
                    parameters.Find(item => item.ParameterName == "flgPaid").Value = "1";
                    break;
                case "6": // No Show
                    parameters.Find(item => item.ParameterName == "flgNoShow").Value = "1";
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "No Show";
                    break;
                case "7": // Cancelled
                    parameters.Find(item => item.ParameterName == "flgCancelled").Value = "1";
                    parameters.Find(item => item.ParameterName == "txtStatus").Value = "Cancelled";
                    break;
            }

            if (subStatus == "1" || subStatus == "2") // if sub status positive or negative update listing status to "ready to bill"
            {
                parameters.Find(item => item.ParameterName == "numStatus").Value = "5";
                parameters.Find(item => item.ParameterName == "txtStatus").Value = subStatus == "1" ? "Positive" : "Negative";
            }

            return parameters;
        }
    }
}

using Oracle.ManagedDataAccess.Client;
using System;

namespace PIAdaptMRP
{
    internal static class Oracle
    {
        internal static OracleConnection OConnection;

        internal static Boolean ConnectionOk;

        /// <summary>
        ///  Reconnect to Oracle
        /// </summary>
        internal static void ReConnect()
        {
            try
            {
                OConnection.Close();
                OConnection.Dispose();
                InitConnection();
                ConnectionOk = false;
                ConnectionOk = Connect();

                if (!ConnectionOk) return;

                Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("Reconnection to Oracle successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Reconnection to Oracle successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("Reconnection to Oracle successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.OracleError, "_OracleError", string.Format("Reconnection to Oracle successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", string.Format("Reconnection to Oracle successful.{0}", Environment.NewLine));
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "Cannot establish Oracle connection");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", ex.Message);
            }
        }

        /// <summary>
        ///   Initialize the Oracle connection object
        /// </summary>
        /// <returns>Oracle Connection Object</returns>
        internal static void InitConnection()
        {
            OConnection = null;

            try
            {
                OConnection = new OracleConnection { ConnectionString = string.Format("User Id={0};Password={1};Data Source={2};", Setup.OracleUserId, Setup.OraclePassword, Setup.OracleDataSource) };

                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}GetOracleConnection() successful.{1}", CHere.FunctionName(), Environment.NewLine));
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}{1}{2}", CHere.FunctionName(), ex.Message, Environment.NewLine));
            }
        }

        /// <summary>
        ///  Connect to Oracle
        /// </summary>
        /// <returns>True if sucessful</returns>
        internal static Boolean Connect()
        {
            ConnectionOk = false;

            try
            {
                OConnection.Open();
                ConnectionOk = true;
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Connection to Oracle instance {0} successful.", Setup.OracleDataSource));
            }
            catch (Exception ex)
            {
                ConnectionOk = false;
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                Logs.Msg(Setup.OracleError, "_OracleError", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
            }

            return ConnectionOk;
        }
    }
}
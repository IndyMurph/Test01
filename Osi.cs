using System;
using System.Data.OleDb;

namespace PIAdaptMRP
{
    internal class Osi
    {
        internal static OleDbConnection PiConnection;

        internal static Boolean ConnectionOk;

        /// <summary>
        ///  Reconnect to PI
        /// </summary>
        internal static void ReConnect()
        {
            try
            {
                PiConnection.Close();
                PiConnection.Dispose();
                InitConnection();
                ConnectionOk = false;
                ConnectionOk = Connect();

                if (!ConnectionOk) return;

                Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("Reconnection to PI successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Reconnection to PI successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("Reconnection to PI successful.{0}", Environment.NewLine));
                // Logs.Msg(Setup.OracleError, "_OracleError", string.Format("Reconnection to PI successful.{0}", Environment.NewLine));
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", string.Format("Reconnection to PI successful.{0}", Environment.NewLine));
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "Cannot establish PI connection");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", ex.Message);
            }
        }

        /// <summary>
        ///   Initialize the PI connection object
        /// </summary>
        /// <returns>PI Connection Object</returns>
        internal static void InitConnection()
        {
            PiConnection = null;

            try
            {
                PiConnection = new OleDbConnection { ConnectionString = string.Format("Provider=PIOLEDB.1;Data Source={0};Integrated Security=SSPI;Command Timeout=60", Setup.PiServer) };

                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}GetPiConnection() successful.{1}", CHere.FunctionName(), Environment.NewLine));
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
                PiConnection.Open();
                ConnectionOk = true;
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Connection to PIServer {0} successful.", Setup.PiServer));
            }
            catch (Exception ex)
            {
                ConnectionOk = false;
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
            }

            return ConnectionOk;
        }

        /*
        internal class Osi
        {
            /// <summary>
            ///   Return true if the tag is found in the PI Server
            /// </summary>
            /// <param name="logFileSuffix">Log file suffix</param>
            /// <param name="piTag">PI Tag to search for</param>
            /// <returns>True if PI Tag string is found in PI Server</returns>
            private static Boolean IsPiTag(String logFileSuffix, String piTag)
            {
                bool isPiTag;
                try
                {
                    isPiTag = true;
                }
                catch (Exception ex)
                {
                    Logs.Msg(Setup.Diagnostics, logFileSuffix, string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                    isPiTag = false;
                }

                Logs.Msg(Setup.Diagnostics, logFileSuffix, string.Format("In {0}: ItIsAPITag is {1}", CHere.FunctionName(), isPiTag));
                return isPiTag;
            }

        } */
    }
}
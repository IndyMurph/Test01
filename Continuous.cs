using System;
using System.Collections.Specialized;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace PIAdaptMRP
{
    internal class Continuous
    {

        private readonly Events _events = new Events();

        private DateTime _piListXmlFileModificationTime;
        internal DateTime PreviousPiListXmlFileModificationTime;
        internal DateTime PreviousSetupFileModificationTime;
        private DateTime _setupFileModificationTime;

        private Int32 _retryCount;

        internal void Loop()
        {
            _retryCount = 0;
            var sc = new ServiceController(PiAdaptMrp.RunningServiceNm);

            do
            {
                // These checks are also executed during OnStart()
                StopIfSetUpXmlDoesNotExist(sc);
                StopIfListXmlDoesNotExist(sc);

                CheckUpdatedSetupInfo(sc);
                CheckUpdatedListInfo();

                Logs.Msg(Setup.OracleError, "_OracleError", "NoDateTimeStamp");
                Logs.Msg(Setup.DailyLog, "_DailyLog", "NoDateTimeStamp");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "NoDateTimeStamp");
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", "NoDateTimeStamp");
                Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", "NoDateTimeStamp");
                Logs.Msg(Setup.OleDBError, "_OleDBError", "NoDateTimeStamp");
                Logs.Msg(Setup.Timing, "_Timing", "NoDateTimeStamp");

                // From this point,  stoppage will be subject to a delay
                PiAdaptMrp.IsStopNow = false;

                // Define end time
                var endTm = DateTime.Now.AddMilliseconds(Convert.ToDouble("-" + Setup.LagTime)).ToString();

                // Define start time
                string startTm;
                if (File.Exists(Setup.LogFileRootName + "NextScanStartTime.log"))
                {
                    var nextStartTimeLogFile = new StreamReader(string.Format("{0}NextScanStartTime.log", Setup.LogFileRootName));
                    startTm = nextStartTimeLogFile.ReadLine();
                    nextStartTimeLogFile.Close();
                    nextStartTimeLogFile.Dispose();
                }
                else
                {
                    // If start time file doesn't exist, force start time to be end time - a constant value (ScanSleepTime)
                    startTm = (DateTime.Parse(endTm).AddMilliseconds(Convert.ToDouble(string.Format("-{0}", Setup.ScanSleepTime))).ToString());
                }

                // Log the start and end times
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", string.Format("{0} Scan Interval {1} {2}", sc.ServiceName, startTm, endTm));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Processing {1} to {2}", CHere.FunctionName(), startTm, endTm));

                if (_events.Process(startTm, endTm))
                {
                    _retryCount = 0;

                    File.Delete(Setup.LogFileRootName + "NextScanStartTime.log");
                    Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", string.Format("File {0}NextScanStartTime.log deleted.", Setup.LogFileRootName));

                    Logs.Msg(Setup.LogFileRootName + "NextScanStartTime.log", "NoDateTimeStamp" + endTm);

                    PiAdaptMrp.IsStopNow = true;
                }
                else
                {
                    Retry(sc);
                }
                sc.Refresh();

                if (String.Compare(sc.Status.ToString(), "Running", StringComparison.Ordinal) == 0)
                {
                    Thread.Sleep(Setup.ScanSleepTime);
                }
            } while (String.Compare(sc.Status.ToString(), "Running", StringComparison.Ordinal) == 0);
        }

        /// <summary>
        ///  Retries connecting to PI and Oracle, in case the main process fails
        ///  Will stop the service if RetryCount exceed  MaxRestartAttempts
        /// </summary>
        /// <param name="sc">Adapter Service controler object</param>
        private void Retry(ServiceController sc)
        {
            sc.Refresh();
            PiAdaptMrp.IsStopNow = true;

            // Stop the service if its not in a running state
            if (String.Compare(sc.Status.ToString(), "Running", StringComparison.Ordinal) != 0)
            {
                sc.Stop();
            }

            // Log retry information and increment retry counter
            if (_retryCount < Setup.MaxRestartAttempts)
            {
                Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Will Attempt to reconnect to PI in " + Setup.RestartSleepTime / 60000 + " minutes.");

                Logs.Msg(Setup.DailyLog, "_DailyLog", "Will Attempt to reconnect to PI in " + Setup.RestartSleepTime / 60000 + " minutes.");
                Logs.Msg(Setup.DailyLog, "_DailyLog", "RestartCount is " + _retryCount + ".");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "Will Attempt to reconnect to PI in " + Setup.RestartSleepTime / 60000 + " minutes.");

                Logs.Msg(Setup.OleDBError, "_OleDBError", "Will Attempt to reconnect to PI in " + Setup.RestartSleepTime / 60000 + " minutes.");
                Logs.Msg(Setup.OleDBError, "_OleDBError", "RestartCount is " + _retryCount + ".");


                Logs.Msg(Setup.OracleError, "_OracleError", "Will Attempt to reconnect to PI in " + Setup.RestartSleepTime / 60000 + " minutes.");
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", "Will Attempt to reconnect to PI in " + Setup.RestartSleepTime / 60000 + " minutes.");

                Thread.Sleep(Setup.RestartSleepTime);
                _retryCount += 1;
            }
            else // Stop service, retries exceed the MaxRestartAttempts value
            {
                Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "RestartCount = " + _retryCount + ". Restart Attempts exhausted.");

                Logs.Msg(Setup.DailyLog, "_DailyLog", "RestartCount = " + _retryCount + ". Restart Attempts exhausted.");
                Logs.Msg(Setup.OleDBError, "_OleDBError", "RestartCount = " + _retryCount + ". Restart Attempts exhausted.");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "RestartCount = " + _retryCount + ". Restart Attempts exhausted.");
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", "RestartCount = " + _retryCount + ". Restart Attempts exhausted.");


                sc.Stop();
                sc.Refresh();
                Logs.Msg(Setup.DailyLog, "_DailyLog", "The service status is now set to " + sc.Status + Environment.NewLine);
            }

            // Validate XML structure, stop process if the invalid
            Setup.ValidateSetupXml();

            // Load setup information
            Setup.LoadConfig();

            // Reset Email list in case information was changed in the updated setup file
            Email.LoadList();

            // Log setup information
            Logs.LogSetupHistory();

            // Connect to PI
            Osi.ReConnect();

            Setup.DebugBitVector32 = new BitVector32(Setup.DebugLevel);

            _setupFileModificationTime = File.GetLastWriteTime(Setup.SetUpXmlFileName);
            PreviousSetupFileModificationTime = _setupFileModificationTime;

            // PI System is connected, try to connect to Oracle.  Stop service if fails
            if (Osi.ConnectionOk) //PiAdaptMrp.AfDatabase.PISystem.ConnectionInfo.IsConnected)
            {
                Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Attempting to reconnect to Oracle.");

                Logs.Msg(Setup.DailyLog, "_DailyLog", "Attempting to reconnect to Oracle");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "Attempting to reconnect to Oracle");
                Logs.Msg(Setup.OleDBError, "_OleDBError", "Attempting to reconnect to Oracle");
                Logs.Msg(Setup.OracleError, "_OracleError", "Attempting to reconnect to Oracle");
                Logs.Msg(Setup.HeartBeat, "_HeartBeat", "Attempting to reconnect to Oracle");

                Oracle.ReConnect();

                if (Oracle.ConnectionOk)
                    Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Reconnection to Oracle successful.");
                else
                {
                    Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Cannot establish Oracle connection");
                    PiAdaptMrp.IsStopNow = true;

                    sc.Stop();
                }
            }
        }


        /// <summary>
        ///  Stop service if the XML Setup file does not exist
        /// 
        ///   Note:  This check is also done during OnStart
        /// </summary>
        private void StopIfSetUpXmlDoesNotExist(ServiceController sc)
        {
            if (File.Exists(Setup.SetUpXmlFileName)) return;

            var theString = string.Format("Setup XML File \"{0}\" does not exist.", Setup.SetUpXmlFileName);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", theString);
            Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("{0} does not exist.{1}", Setup.SetUpXmlFileName, Environment.NewLine));
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("{0} does not exist.{1}", Setup.SetUpXmlFileName, Environment.NewLine));
            Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, string.Format("{0} does not exist.", Setup.SetUpXmlFileName));
            Thread.Sleep(5000);
            PiAdaptMrp.IsStopNow = true;
            sc.Stop();
        }

        /// <summary>
        ///  Stop service if the Unit List XML file does not exist
        /// 
        ///  Note:  This check is also done during OnStart
        /// </summary>
        private static void StopIfListXmlDoesNotExist(ServiceController sc)
        {
            if (File.Exists(Setup.ListXmlFileName)) return;

            var theString = string.Format("File \"{0}\" does not exist.", Setup.ListXmlFileName);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", theString);
            Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("{0} does not exist.{1}", Setup.ListXmlFileName, Environment.NewLine));
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("{0} does not exist.{1}", Setup.ListXmlFileName, Environment.NewLine));
            Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, string.Format("{0} does not exist.", Setup.ListXmlFileName));
            Thread.Sleep(5000);
            PiAdaptMrp.IsStopNow = true;
            sc.Stop();
        }

        /// <summary>
        /// Update internal unit batch information if unit batch file was updated during execution
        /// </summary>
        private void CheckUpdatedListInfo()
        {
            _piListXmlFileModificationTime = File.GetLastWriteTime(Setup.ListXmlFileName);

            if (_piListXmlFileModificationTime > PreviousPiListXmlFileModificationTime)
            {
                // Validate XML structure, stop process if the invalid
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In: {0} Calling: Setup.ValidateListXml()", CHere.FunctionName()));
                Setup.ValidateListXml(Setup.ListXmlFileName, Setup.ListXsdFileName);

                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Starting ProcDoc {0}", DateTime.Now));

                // Update List information
                Dictionary.LoadListXmlDoc();
                Dictionary.CreateReportAndPromptDictRecords();

                PreviousPiListXmlFileModificationTime = _piListXmlFileModificationTime;
            }

            // Regenerate Duplicate XML file if Unit List XML file has been updated recently
            if (_piListXmlFileModificationTime > File.GetLastWriteTime(string.Format("{0}{1}_Duplicate.xml", Setup.LogFileRootName, DateTime.Today.DayOfWeek)))
            {
                Setup.CreateDuplicateListXml();
            }
        }

        /// <summary>
        /// Update internal setup information if setup file was updated during execution
        /// </summary>
        private void CheckUpdatedSetupInfo(ServiceController sc)
        {
            _setupFileModificationTime = File.GetLastWriteTime(Setup.SetUpXmlFileName);
            if (_setupFileModificationTime <= PreviousSetupFileModificationTime) return;

            // Validate XML structure, stop process if the invalid
            Setup.ValidateSetupXml();

            // Load setup information
            Setup.LoadConfig();

            // Log setup information
            Logs.LogSetupHistory();

            // Reset Email list in case information was changed in the updated setup file
            Email.LoadList();

            // Reset Oracle connection if already connected
            if (true) //PiAdaptMrp.AfDatabase.PISystem.ConnectionInfo.IsConnected)
            {
                // Reconnect in case connection information was changed in the updated setup file
                // PiAdaptMrp.AfDatabase = _osi.ConnectToAfDb();

                if (true) //PiAdaptMrp.AfDatabase == null || !PiAdaptMrp.AfDatabase.PISystem.ConnectionInfo.IsConnected)
                {
                    Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Cannot Acquire AF Database", CHere.FunctionName()));
                    Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Cannot Acquire AF Database");
                    PiAdaptMrp.IsStopNow = true;

                    sc.Stop();
                }

                // Reconnect in case connection information was changed in the updated setup file
                Osi.ReConnect();

                if (Osi.ConnectionOk)
                    Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Reconnection to Oracle successful.");
                else
                {
                    Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Cannot establish Oracle connection");
                    PiAdaptMrp.IsStopNow = true;

                    sc.Stop();
                }
            }

            Setup.DebugBitVector32 = new BitVector32(Setup.DebugLevel);

            PreviousSetupFileModificationTime = _setupFileModificationTime;
        }
    }
}
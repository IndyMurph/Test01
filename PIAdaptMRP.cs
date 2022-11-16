using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.ServiceProcess;
using System.Threading;

namespace PIAdaptMRP
{
    public partial class PiAdaptMrp : ServiceBase
    {
        internal static String RunningServiceNm;

        internal static bool IsStopNow;
        private static Int32 _myPid;

        internal readonly string[] Args;
        private int _timingMessageCounter;
        private DateTime _was;

        private Thread _workerThread;
        private readonly Continuous _continuous;

        internal PiAdaptMrp(string[] args)
        {
            Args = args;
            RunningServiceNm = Process.GetCurrentProcess().MainModule.ModuleName.Remove(Process.GetCurrentProcess().MainModule.ModuleName.IndexOf('.'));

            InitializeComponent();
            ServiceName = RunningServiceNm;
            _continuous = new Continuous();
        }

        protected override void OnStart(string[] args)
        {
            _was = DateTime.Now;

            Setup.XmlFileGood = true;
            Setup.ServiceStart = true;
            IsStopNow = false;

            RunningServiceNm = GetServiceName();

            CheckCommandLineParameters(args);
            GetCommandLineParameters(args, ref Setup.SetUpXmlFileName, ref Setup.SetUpServiceName, ref Setup.SetUpXsdFileName);

            // Load Setup XML information
            Setup.LoadConfig();

            Setup.DebugBitVector32 = new BitVector32(Setup.DebugLevel);
            Setup.PagerBitVector32 = new BitVector32(Setup.PagerLevel);

            // Load Email list from setup file information
            Email.LoadList();

            // Validate XML structure, stop process if the invalid
            Setup.ValidateSetupXml();

            // Log setup information
            Logs.LogSetupHistory();

            const string userRoot = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services";
            var subkey = RunningServiceNm;

            var keyName = userRoot + "\\" + subkey;

            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Service Name is {1}", CHere.FunctionName(), RunningServiceNm));

            try
            {
                var tempString = (String)Registry.GetValue(keyName, "Description", string.Format("No DisplayName was found for Service {0}", RunningServiceNm));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Registry Description Name for Service {1} is {2}", CHere.FunctionName(), RunningServiceNm, tempString));
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.DailyLog, "_DailyLog", ex.Message);
            }

            try
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: The Executable Name is {1}", CHere.FunctionName(), RunningServiceNm));

                var sc = new ServiceController(RunningServiceNm);
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: The Service display name is {1}", CHere.FunctionName(), sc.DisplayName));

                SetServiceDescription(true);
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.DailyLog, "_DailyLog", ex.Message);
            }

            // Validate XML structure, stop process if the invalid
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In: {0} Calling: Setup.ValidateListXml()", CHere.FunctionName()));
            Setup.ValidateListXml(Setup.ListXmlFileName, Setup.ListXsdFileName);

            _continuous.PreviousSetupFileModificationTime = File.GetLastWriteTime(Setup.SetUpXmlFileName);

            Logs.Msg(Setup.Timing, "_Timing", "NoDateTimeStamp************************************************************************");

            LogOnStartTimingInfo("Entering", 0);


            // Initialize PI Connection
            try
            {
                Osi.InitConnection();
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "Cannot initialize PI connection object");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", ex.Message);

                Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Cannot initialize PI connection object");
                IsStopNow = true;
                Stop();
            }

            // Stop process if we cant connect to PI
            if ((Osi.ConnectionOk = Osi.Connect()) == false)
            {
                IsStopNow = true;
                Stop();
            }

            // Initialize Oracle Connection
            try
            {
                Oracle.InitConnection();
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "Cannot initialize Oracle connection object");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", ex.Message);

                Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName, "Cannot initialize Oracle connection object");
                IsStopNow = true;
                Stop();
            }

            // Stop process if we cant connect to Oracle
            if ((Oracle.ConnectionOk = Oracle.Connect()) == false)
            {
                IsStopNow = true;
                Stop();
            }

            Dictionary.LoadListXmlDoc();
            Setup.CreateDuplicateListXml();
            Dictionary.CreateReportAndPromptDictRecords();

            _continuous.PreviousPiListXmlFileModificationTime = File.GetLastWriteTime(Setup.ListXmlFileName);

            _workerThread = new Thread(ServiceWorkerThread);
            _workerThread.Start();

            LogOnStartTimingInfo("Leaving", 1);
        }


        private void ServiceWorkerThread()
        {
            Logs.Msg(Setup.DailyLog, "_DailyLog", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.DailyLog, "_DailyLog", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            Logs.Msg(Setup.HeartBeat, "_HeartBeat", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.HeartBeat, "_HeartBeat", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            Logs.Msg(Setup.Diagnostics, "_Diagnostics", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            Logs.Msg(Setup.Timing, "_Timing", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.Timing, "_Timing", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            Logs.Msg(Setup.OracleError, "_OracleError", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.OracleError, "_OracleError", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            Logs.Msg(Setup.OleDBError, "_OleDBError", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.OleDBError, "_OleDBError", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", "NoDateTimeStamp************************************************************************");
            Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", ServiceName + " Started Using Setup " + Setup.SetUpName + " " + DateTime.Now + Environment.NewLine);

            if (true)
            {
                _continuous.Loop();
            }
        }

        /// <summary>
        ///  Code executed when service is stopping
        /// </summary>
        protected override void OnStop()
        {
            var count = 0;
            while (IsStopNow == false && count++ < Setup.ScanSleepTime / 1000)
            {
                Thread.Sleep(1000);
            }

            Logs.Msg(Setup.DailyLog, "_DailyLog", "NoDateTimeStamp ");
            Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.DailyLog, "_DailyLog", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.HeartBeat, "_HeartBeat", "NoDateTimeStamp ");
            Logs.Msg(Setup.HeartBeat, "_HeartBeat", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.HeartBeat, "_HeartBeat", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.Diagnostics, "_Diagnostics", "NoDateTimeStamp ");
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.Timing, "_Timing", "NoDateTimeStamp ");
            Logs.Msg(Setup.Timing, "_Timing", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.Timing, "_Timing", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.OracleError, "_OracleError", "NoDateTimeStamp ");
            Logs.Msg(Setup.OracleError, "_OracleError", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.OracleError, "_OracleError", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.OleDBError, "_OleDBError", "NoDateTimeStamp ");
            Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", "NoDateTimeStamp ");
            Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.HouseKeeping, "_HouseKeeping", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));

            Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", "NoDateTimeStamp ");
            Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", string.Format("{0} ended {1}", Setup.SetUpName, DateTime.Now));
            Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", string.Format("NoDateTimeStamp************************************************************************{0}", Environment.NewLine));


            if (IsStopNow)
            {
                Email.SendMsgToList(Setup.EmailAdmin, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName,
                    "PIAdapter Has Been Shutdown");
            }
            else
            {
                Email.SendMsgToList(Setup.EmailAdmin, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName,
                    "PIAdapter Has Been Shutdown After Delay");
                Thread.Sleep(Setup.ScanSleepTime / 2);
                try
                {
                }
                catch (Exception workere)
                {
                    Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", "No Worker Thread - Service aborted.");
                    Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", workere.Message);
                }
            }

            if (Osi.ConnectionOk)
            {
                try
                {
                    Osi.PiConnection.Close();
                    Osi.PiConnection.Dispose();
                }
                catch (Exception piConnectione)
                {
                    Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", "No PiConnection - Service aborted.");
                    Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", piConnectione.Message);

                    Logs.Msg(Setup.OleDBError, "_OleDBError", "No PiConnection - Service aborted.");
                    Logs.Msg(Setup.OleDBError, "_OleDBError", piConnectione.Message);
                }
            }

            if (Oracle.ConnectionOk)
            {
                try
                {
                    Oracle.OConnection.Close();
                    Oracle.OConnection.Dispose();
                }
                catch (Exception oConnectione)
                {
                    Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", "No OracleConnection - Service aborted.");
                    Logs.Msg(Setup.LogFileRootName + "SetUpHistory.log", oConnectione.Message);

                    Logs.Msg(Setup.OracleError, "_OracleError", "No OracleConnection - Service aborted.");
                    Logs.Msg(Setup.OracleError, "_OracleError", oConnectione.Message);
                }
            }

            SetServiceDescription(false);
            _workerThread.Abort();
        }


        /// <summary>
        ///   Set the service description
        /// </summary>
        /// <param name="addPid">Add the PID or not</param>
        protected void SetServiceDescription(Boolean addPid)
        {
            const string userRoot = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services";
            var subkey = RunningServiceNm;

            var keyName = userRoot + "\\" + subkey;
            var tempStr = (String)Registry.GetValue(keyName, "Description", string.Format("No DisplayName was found for Service {0}", ServiceName));

            try
            {
                tempStr = tempStr.Remove(tempStr.IndexOf(" :PID", StringComparison.Ordinal));
            }
            catch (Exception tempStre)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: TempStr did not contain :PID", CHere.FunctionName()));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", tempStre.Message);
            }

            if (addPid)
            {
                tempStr += string.Format(" :PID {0}", _myPid);
            }

            Registry.SetValue(keyName, "Description", tempStr);
        }

        /// <summary>
        ///  Get the service name
        /// </summary>
        /// <returns>Service name</returns>
        private static String GetServiceName()
        {
            var scServices = ServiceController.GetServices();

            // Display the list of services currently running on this computer.
            var myPid = Process.GetCurrentProcess().Id;

            foreach (var scTemp in scServices)
            {
                // Write the service name and the display name
                // for each running service.

                // Query WMI for additional information about this service.
                // Display the start name (LocalSytem, etc) and the service
                // description.
                var wmiService = new ManagementObject("Win32_Service.Name='" + scTemp.ServiceName + "'");
                wmiService.Get();

                var id = Convert.ToInt32(wmiService["ProcessId"]);
                if (id == myPid)
                {
                    _myPid = id;
                    return scTemp.ServiceName;
                }
            }
            return "NotFound";
        }

        /// <summary>
        ///     Verify the command line arguments of the service
        ///     Arg[0] : XML Setup File name
        ///     Arg[2} : Setup XSD file name
        ///     Note: Fatal error are logged to files in C:\Temp folder
        /// </summary>
        /// <param name="args">List of command line arguments</param>
        internal void CheckCommandLineParameters(IList<string> args)
        {
            // Set to False if a parameter configuration error is detected
            var commandLineParametersOk = true;

            // If we use the environment variables, verify that they all exist.  
            // Otherwise create / update Fatal Error log file
            if (args == null || args.Count == 0)
            {
                // Check if Setup file Environment variable is set
                if (Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXMLFile") == null)
                {
                    commandLineParametersOk = SetupXmlVariableNotExist(true);
                }
                else
                {
                    // Check if Setup file exist
                    if (File.Exists(Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXMLFile")) == false)
                    {
                        commandLineParametersOk = SetupFileXmlNotExist(Environment.GetEnvironmentVariable(string.Format("{0}SetUpXMLFile", RunningServiceNm)), true);
                    }
                }
                // Check if Setup name Environment variable is set
                if (Environment.GetEnvironmentVariable(RunningServiceNm + "SetupName") == null)
                {
                    commandLineParametersOk = SetupNameVariableNotExist(commandLineParametersOk);
                }
                // Check setup  XSD file Environment variable is set
                if (Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXSDFile") == null)
                {
                    commandLineParametersOk = XsdVariableNotExist(commandLineParametersOk);
                }
                else
                {
                    // Check if setup XSD file exist
                    if (File.Exists(Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXSDFile")) == false)
                    {
                        commandLineParametersOk = XsdFileNotExist(Environment.GetEnvironmentVariable(string.Format("{0}SetUpXSDFile", RunningServiceNm)), commandLineParametersOk);
                    }
                }
            }

            else
                switch (args.Count)
                {
                    case 1:
                        // Check if setup file exist
                        if (File.Exists(args[0]) == false)
                        {
                            commandLineParametersOk = SetupFileXmlNotExist(args[0], true);
                        }
                        // Check if setup file environment variable is set
                        if (Environment.GetEnvironmentVariable(RunningServiceNm + "SetupName") == null)
                        {
                            commandLineParametersOk = SetupXmlVariableNotExist(commandLineParametersOk);
                        }
                        // Check if XSD setup file environment variable is set
                        if (Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXSDFile") == null)
                        {
                            commandLineParametersOk = XsdVariableNotExist(commandLineParametersOk);
                        }
                        break;
                    case 2:
                        // Check if Setup File exist
                        if (File.Exists(args[0]) == false)
                        {
                            commandLineParametersOk = SetupFileXmlNotExist(args[0], true);
                        }
                        // Check if XSD Setup Environment variable is set
                        if (Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXSDFile") == null)
                        {
                            commandLineParametersOk = XsdVariableNotExist(commandLineParametersOk);
                        }
                        else
                        {
                            // Check if setup XSD file exist
                            if (File.Exists(Environment.GetEnvironmentVariable(RunningServiceNm + "SetUpXSDFile")) == false)
                            {
                                commandLineParametersOk = XsdFileNotExist(Environment.GetEnvironmentVariable(string.Format("{0}SetUpXSDFile", RunningServiceNm)), commandLineParametersOk);
                            }
                        }
                        break;
                    case 3:
                        // Check if Setup File exist
                        if (File.Exists(args[0]) == false)
                        {
                            commandLineParametersOk = SetupFileXmlNotExist(args[0], true);
                        }
                        // Check if XSD Setup File exist
                        if (File.Exists(args[2]) == false)
                        {
                            commandLineParametersOk = XsdFileNotExist(args[2], commandLineParametersOk);
                        }
                        break;
                }

            if (commandLineParametersOk == false)
            {
                Stop();
            }
        }

        /// <summary>
        ///     Logs fatal error message when the Setup Name environment variable is not set
        /// </summary>
        /// <param name="commandLineParametersOk">To determine if header is required</param>
        /// <returns>False</returns>
        private static bool SetupNameVariableNotExist(bool commandLineParametersOk)
        {
            if (commandLineParametersOk)
            {
                Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", string.Format("NoDateTimeStamp {0}************************************************************************{0}", Environment.NewLine));
            }
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", "Insufficient formal parameters for service startup.");

            var errMsg = string.Format("System Variable {0}SetupName does not exist.", RunningServiceNm);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", errMsg);

            return false;
        }

        /// <summary>
        ///     Logs fatal error message when the XSD file environment variable is not set
        /// </summary>
        /// <param name="commandLineParametersOk">To determine if header is required</param>
        /// <returns>False</returns>
        private static bool XsdVariableNotExist(bool commandLineParametersOk)
        {
            if (commandLineParametersOk)
            {
                Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", string.Format("NoDateTimeStamp {0}************************************************************************{0}", Environment.NewLine));
            }

            var errMsg = string.Format("System Variable {0}SetUpXSDFile does not exist.", RunningServiceNm);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", errMsg);

            return false;
        }

        /// <summary>
        ///     Logs fatal error message when the XML setup file environment variable is not set
        /// </summary>
        /// <param name="commandLineParametersOk">To determine if header is required</param>
        /// <returns>False</returns>
        private static bool SetupXmlVariableNotExist(bool commandLineParametersOk)
        {
            if (commandLineParametersOk)
            {
                Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", string.Format("NoDateTimeStamp {0}************************************************************************{0}", Environment.NewLine));
            }

            var errMsg = string.Format("System Variable {0}SetUpXMLFile does not exist.", RunningServiceNm);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", errMsg);

            return false;
        }

        /// <summary>
        ///     Logs fatal error message when XSD File does not exist
        /// </summary>
        /// <param name="fileNm">Missing file name</param>
        /// <param name="commandLineParametersOk">To determine if header is required</param>
        /// <returns>false</returns>
        private static bool XsdFileNotExist(string fileNm, bool commandLineParametersOk)
        {
            if (commandLineParametersOk)
            {
                Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", string.Format("NoDateTimeStamp {0}************************************************************************{0}", Environment.NewLine));
            }
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", "Insufficient formal parameters for service startup.");

            var errMsg = string.Format("XSD File \"{0}\" does not exist.", fileNm);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", errMsg);

            return false;
        }

        /// <summary>
        ///     Logs fatal error message when setup XML File does not exist
        /// </summary>
        /// <param name="fileNm">Missing file name</param>
        /// <param name="commandLineParametersOk">To determine if header is required</param>
        /// <returns>false</returns>
        private static bool SetupFileXmlNotExist(string fileNm, bool commandLineParametersOk)
        {
            if (commandLineParametersOk)
            {
                Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", string.Format("NoDateTimeStamp {0}************************************************************************{0}", Environment.NewLine));
            }
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", "Insufficient formal parameters for service startup.");

            var errMsg = string.Format("Setup File \"{0}\" does not exist.", fileNm);
            Logs.Msg("c:\\temp\\PIAdapterFatalErrors.txt", errMsg);

            return false;
        }


        /// <summary>
        ///     Log timing info when the service in entering and leaving the OnStart function
        /// </summary>
        /// <param name="action">Entering or Leaving</param>
        /// <param name="counter">0 when entering or 1 when leaving</param>
        private void LogOnStartTimingInfo(String action, int counter)
        {
            var isNow = DateTime.Now;
            var diff = isNow.Subtract(_was);
            _was = isNow;
            Logs.Msg(Setup.Timing, "_Timing", string.Format("In OnStart ::{0}: {1} ::::::::::: {2} :::: {3}::::", action, counter, _timingMessageCounter++, diff.TotalMilliseconds));
        }

        /// <summary>
        ///     Get the setup information from the command lines or environment variables
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="setUpXmlFileName">XML setup File Name</param>
        /// <param name="setUpName">Setup Name</param>
        /// <param name="setUpXsdFileName">XSD Setup file name</param>
        /// <param name="allowUnitPrefix">Unit Prefix</param>
        private static void GetCommandLineParameters(IList<string> args,
            ref String setUpXmlFileName,
            ref String setUpName,
            ref String setUpXsdFileName)
        {
            setUpXmlFileName = (args != null && args.Count > 0) ? args[0] : Environment.GetEnvironmentVariable(string.Format("{0}SetUpXMLFile", RunningServiceNm));
            setUpName = (args != null && args.Count > 1) ? args[1] : Environment.GetEnvironmentVariable(string.Format("{0}SetupName", RunningServiceNm));
            setUpXsdFileName = (args != null && args.Count > 2) ? args[2] : Environment.GetEnvironmentVariable(string.Format("{0}SetUpXSDFile", RunningServiceNm));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wildCardString">Wild card string</param>
        /// <param name="stringToMatch">String to match</param>
        /// <param name="delimiters">Delimiter</param>
        /// <returns>True if match is found</returns>
        internal static Boolean Matches(String wildCardString, String stringToMatch, char[] delimiters)
        {
            wildCardString = wildCardString.ToUpper();
            stringToMatch = stringToMatch.ToUpper();

            var pieces = wildCardString.Split(delimiters);

            if (pieces.Length == 1)
            {
                return String.Compare(wildCardString, stringToMatch, StringComparison.Ordinal) == 0;
            }

            if (!stringToMatch.StartsWith(pieces[0]) || !stringToMatch.EndsWith(pieces[pieces.Length - 1]))
                return false;

            var theOffset = 0;

            foreach (var t in pieces)
            {
                if (stringToMatch.IndexOf(t, theOffset, StringComparison.Ordinal) > -1)
                {
                    theOffset = t.Length + stringToMatch.IndexOf(t, theOffset, StringComparison.Ordinal);
                }
                else
                    return false;
            }

            return (true);
        }
    }
}
using System;
using System.IO;
using System.Threading;

namespace PIAdaptMRP
{
    internal static class Logs
    {
        /// <summary>
        ///     Log messages to log files
        /// </summary>
        /// <param name="debugBitMask">Debug Bit Mask</param>
        /// <param name="logFileSuffix">Log File Suffix</param>
        /// <param name="msg">Message to log</param>
        internal static void Msg(int debugBitMask, String logFileSuffix, String msg)
        {
            if (!Setup.DebugBitVector32[debugBitMask]) return;
            var logFileName = "";
            logFileName = DetermineDailyLogFileNameWithType(logFileSuffix);

            var outfil = new StreamWriter(logFileName, true);

            if (msg.StartsWith("NoDateTimeStamp"))
            {
                msg = msg.Remove(0, 15);
                if (msg.Length > 0)
                {
                    outfil.WriteLine(msg);
                }
            }
            else
            {
                var now = DateTime.Now;
                outfil.WriteLine(now + " " + msg);
            }

            outfil.Close();
            outfil.Dispose();
        }

        /// <summary>
        ///     Log messages to log files
        /// </summary>
        /// <param name="logfileNm">Logfile name</param>
        /// <param name="msg">Message to log</param>
        internal static void Msg(String logfileNm, String msg)
        {
            var outfil = new StreamWriter(logfileNm, true);

            // Remove NoDateTimeStamp from the string, otherwise insert the actual date and time
            if (msg.StartsWith("NoDateTimeStamp"))
            {
                msg = msg.Remove(0, 15);
                outfil.WriteLine(msg);
            }
            else
            {
                var now = DateTime.Now;
                outfil.WriteLine(now + " " + msg);
            }

            outfil.Close();
            outfil.Dispose();
        }

        /// <summary>
        ///  Determine daily log file name 
        /// </summary>
        /// <param name="logFileSuffix">Log file suffix</param>
        private static string DetermineDailyLogFileNameWithType(String logFileSuffix)
        {
            try
            {
                var logFileNm = Setup.LogFileRootName + DateTime.Today.DayOfWeek + logFileSuffix + ".log";

                if (File.Exists(logFileNm))
                {
                    var dt = File.GetCreationTime(logFileNm);

                    if (dt.AddDays(2) < DateTime.Now)
                    {
                        File.SetCreationTime(logFileNm, DateTime.Now);
                        File.Delete(logFileNm);
                        Msg(Setup.HouseKeeping, "_HouseKeeping", string.Format("File {0} deleted.", logFileNm));
                        Msg(logFileNm, string.Format("{0} creation date/time = {1}{2}", logFileNm, DateTime.Now, Environment.NewLine));
                    }
                }
                else
                {
                    Msg(logFileNm, logFileNm + " creation date/time = " + DateTime.Now + Environment.NewLine);
                }
                return logFileNm;
            }
            catch (Exception ex)
            {
                Msg("c:\\temp\\PIAdapterFatalErrors.txt", string.Format("NoDateTimeStamp {0}************************************************************************{0}", Environment.NewLine));
                Msg("c:\\temp\\PIAdapterFatalErrors.txt", "Cannot determine Daily Log File Name.");
                Msg("c:\\temp\\PIAdapterFatalErrors.txt", ex.Message + Environment.NewLine);
                Email.SendMsgToList(Setup.EmailDiag, Setup.EmailFromAddress, Email.EmailList, Setup.SetUpName,
                    "Cannot determine Daily Log File Name.");
                Thread.Sleep(Setup.ScanSleepTime / 2);
                Thread.CurrentThread.Abort();
                return "";
            }

        }

        /// <summary>
        ///     Logs the current setup in SetupHistory.log
        /// </summary>
        internal static void LogSetupHistory()
        {
            try
            {
                var sw = new StreamWriter(string.Format("{0}SetupHistory.log", Setup.LogFileRootName), true);
                sw.WriteLine("");

                // Logs Service Start Arguments when Service is started
                if (Setup.ServiceStart)
                {
                    sw.WriteLine("Service Start Arguments for {0} as of {1}", Setup.SetUpServiceName, DateTime.Now);
                    sw.WriteLine("************************************************************************");
                    sw.WriteLine("  SetUpXmlFileName  : {0}", Setup.SetUpXmlFileName);
                    sw.WriteLine("  SetUpName         : {0}", Setup.SetUpServiceName);
                    sw.WriteLine("  SetUpXsdFileName  : {0}", Setup.SetUpXsdFileName);
                    sw.WriteLine("");
                    Setup.ServiceStart = false;
                }

                sw.WriteLine("SetUp as of {0}", DateTime.Now);
                sw.WriteLine("");
                sw.WriteLine("SetUpName is                       : {0}", Setup.SetUpName);
                sw.WriteLine("LogFileRootName is                 : {0}", Setup.LogFileRootName);
                sw.WriteLine("PIListXMLFileName is               : {0}", Setup.ListXmlFileName);
                sw.WriteLine("PIListXSDFileName is               : {0}", Setup.ListXsdFileName);
                sw.WriteLine("PIServer is                        : {0}", Setup.PiServer);
                sw.WriteLine("InitScanDays is                    : {0}", Setup.InitScanDays);
                sw.WriteLine("InitScanBatches is                 : {0}", Setup.InitScanBatches);
                sw.WriteLine("NormalScanDays is                  : {0}", Setup.NormalScanDays);
                sw.WriteLine("NormalScanBatches is               : {0}", Setup.NormalScanBatches);
                sw.WriteLine("PIToOracle_DateTime_FormatModel is : {0}", Setup.PiToOracleDateTimeFormatModel);
                sw.WriteLine("OracleDataSource is                : {0}", Setup.OracleDataSource);
                sw.WriteLine("OracleUserID is                    : {0}", Setup.OracleUserId);
                sw.WriteLine("OraclePassword is                  : {0}", Setup.OraclePassword);
                sw.WriteLine("ScanSleepTime is                   : {0}", Setup.ScanSleepTime);
                sw.WriteLine("LagTime is                         : {0}", Setup.LagTime);
                sw.WriteLine("ThrottleTime is                    : {0}", Setup.Throttletime);
                sw.WriteLine("RestartSleepTime is                : {0}", Setup.RestartSleepTime);
                sw.WriteLine("MaxRestartAttempts is              : {0}", Setup.MaxRestartAttempts);
                sw.WriteLine("DebugLevel is                      : {0}", Setup.DebugLevel);
                sw.WriteLine("PagerLevel is                      : {0}", Setup.PagerLevel);
                sw.WriteLine("EmailFromAddress is                : {0}", Setup.EmailFromAddress);
                sw.WriteLine("EmailServer is                     : {0}", Setup.EmailServer);
                sw.WriteLine("DailyLog Mask is                   : {0}", Setup.DailyLog);
                sw.WriteLine("DailyLogInsert Mask is             : {0}", Setup.DailyLogInsert);
                sw.WriteLine("DailyLogSelect Mask is             : {0}", Setup.DailyLogSelect);
                sw.WriteLine("Diagnostics Mask is                : {0}", Setup.Diagnostics);
                sw.WriteLine("HeartBeat Mask is                  : {0}", Setup.HeartBeat);
                sw.WriteLine("HouseKeeping Mask is               : {0}", Setup.HouseKeeping);
                sw.WriteLine("OleDBError Mask is                 : {0}", Setup.OleDBError);
                sw.WriteLine("OracleError Mask is                : {0}", Setup.OracleError);
                sw.WriteLine("Timing Mask is                     : {0}", Setup.Timing);
                sw.WriteLine("PagerAdmin mask is                 : {0}", Setup.EmailAdmin);
                sw.WriteLine("PagerDiag mask is                  : {0}", Setup.EmailDiag);

                sw.WriteLine("");

                var llPagerPointer = Email.EmailList.First;
                while (llPagerPointer != null)
                {
                    sw.WriteLine("Pager Name                     : {0}", llPagerPointer.Value.EmailNm);
                    sw.WriteLine("Pager Address                  : {0}", llPagerPointer.Value.EmailAdd);
                    llPagerPointer = llPagerPointer.Next;
                }

                sw.WriteLine("");
                sw.WriteLine("");

                sw.Close();
                sw.Dispose();
            }
            catch (Exception ex)
            {
                Msg(@"_PIAdapterFatalErrors", string.Format("Cannot Log SetUp History{0}", Environment.NewLine));
                Msg(@"_PIAdapterFatalErrors", ex.Message);
            }
        }
    }
}
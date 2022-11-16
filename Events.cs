using System;
using System.Data.OleDb;

namespace PIAdaptMRP
{
    public class Events
    {
        public static DateTime startDt;
        public static DateTime endDt;
        public static char pipe = '|';
        public static char slash = '\u005c';

        /// <summary>
        ///   Process events betweeen start and end time
        /// </summary>
        /// <param name="startTm">Start Time</param>
        /// <param name="endTm">End time</param>
        /// <returns>True is sucessful</returns>
        internal Boolean Process(String startTm, String endTm)
        {


            try
            {
                // Convert string date and time to a DateTime type
                // Remove 100 msec from the start time,  Add 100 msec to the end time
                startDt = DateTime.Parse(startTm).AddMilliseconds(-100.0);
                endDt = DateTime.Parse(endTm).AddMilliseconds(100);
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", "---------------------------------------------------------------------------------------------------------");
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: StartTime: {1} EndTime: {2}", CHere.FunctionName(), startDt.ToString("dd-MMM-yyyy HH:mm:ss"), endDt.ToString("dd-MMM-yyyy HH:mm:ss")));
                return SubProcessReportAndPromptEvents();
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: CollectionStartTime is {1}", CHere.FunctionName(), startTm));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: CollectionEndTime   is {1}", CHere.FunctionName(), endTm));
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Cannot calculate Start/End Times. {1}", CHere.FunctionName(), ex.Message));
                return false;
            }
        }

        internal static Boolean SubProcessReportAndPromptEvents()
        {
            try 
            {
                foreach (int value in Dictionary.ReportAndPromptDict.Values)
                {

                    string[] element = Dictionary.ReportAndPromptItems[value].ToString().Split(pipe);

                    // Split SubBatchPath
                    string[] sbPath = element[3].Split(slash);

                    switch (element[0].TrimEnd())
                    {

                        case "NumericReport":
                            DoNumericReport(ref element, ref sbPath);
                            break;

                        case "TextReport":
                            break;

                        case "PromptEvent":
                            break;
                    }
                    Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: value: {1}", CHere.FunctionName(), element[0]));

                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                return false;
            }
        }

        public static Boolean DoNumericReport(ref string[] ele, ref string[] sbp)
        {
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: value: {1}", CHere.FunctionName(), ele[0]));
            
            string dataQry = string.Format(
              @"SELECT TOP {0} time
					FROM [piarchive]..[picomp2]
					WHERE time BETWEEN '*-{1}d' AND '{2}'
					  AND tag = '{3}:Unit_Procedure'
					  AND CAST (value AS string) LIKE '{4}' 
					ORDER BY time DESC", Setup.InitScanBatches, Setup.InitScanDays, endDt.ToString(), ele[1], ele[2]);

            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Query: {0}", dataQry));

            var dataCmd = new OleDbCommand(dataQry, Osi.PiConnection);

            try
            {
                OleDbDataReader dataReader;
                dataReader = dataCmd.ExecuteReader();
                string overallStartTm = null;

                if (dataReader.HasRows)
                {
                    
                    while (dataReader.Read())
                    {

                        overallStartTm = dataReader["time"].ToString();
                        Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0} Time Value: {1} ", CHere.FunctionName(), overallStartTm));

                    }
                    Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0} 1st Batch StartTime: {1} ", CHere.FunctionName(), overallStartTm));
                    return true;
                }
                else
                {
                    Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Query return NO ROWS!"));
                    overallStartTm = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("In {0}: " + "Cannot perform the Select for the following statement.", CHere.FunctionName()));
                Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("In {0}: {1}", CHere.FunctionName(), dataQry));
                Logs.Msg(Setup.OleDBError, "_OleDBError", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                return false;
            }
            finally
            {
                dataCmd.Dispose();
            }
        }
    }
}


/*



                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: query: {1}", CHere.FunctionName(), dataQry.ToString()));

                selectStr = string.Format(
                  @"SELECT time AS Time,
					  substr(CAST (value AS string),1,instr(CAST (value AS string),'|',1)-1) AS BatchID,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,1)+1,instr(CAST (value AS string),'|',1,2)-instr(CAST (value AS string),'|',1,1)-1) AS Recipe,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,2)+1,instr(CAST (value AS string),'|',1,3)-instr(CAST (value AS string),'|',1,2)-1) AS UnitProcedureName,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,3)+1,instr(CAST (value AS string),'|',1,4)-instr(CAST (value AS string),'|',1,3)-1) AS OperationName,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,4)+1,instr(CAST (value AS string),'|',1,5)-instr(CAST (value AS string),'|',1,4)-1) AS PhaseName,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,5)+1,instr(CAST (value AS string),'|',1,6)-instr(CAST (value AS string),'|',1,5)-1) AS AltPhaseName,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,6)+1,instr(CAST (value AS string),'|',1,7)-instr(CAST (value AS string),'|',1,6)-1) AS ParameterName,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,7)+1,instr(CAST (value AS string),'|',1,8)-instr(CAST (value AS string),'|',1,7)-1) AS ParameterValue,
					  substr(CAST (value AS string),instr(CAST (value AS string),'|',1,8)+1,len(CAST (value AS string))) AS UOM
					FROM [piarchive]..[picomp2]
					WHERE time BETWEEN '*-{0}d' AND '{1}'
					  AND tag = '{2}:Report'
					  AND CAST (value AS string) LIKE '*|{3}|{4}|{5}|{6}|{7}|*'
					ORDER BY time ASC", Setup.InitScanDays, endDt.ToString(), ele[1], ele[2], sbp[1], sbp[2], ele[4], ele[5]);

                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: query: {1}", CHere.FunctionName(), selectStr));
*/

using System;
using System.Collections.Generic;
using System.Xml;

namespace PIAdaptMRP
{


    internal static class Dictionary
    {
        private static readonly Osi Osi = new Osi();

        internal static XmlDocument ListXmlDoc;
        internal static SortedDictionary<string, int> ReportAndPromptDict = new SortedDictionary<string, int>();
        internal static List<string> ReportAndPromptItems = new List<string>();
        internal static char space = ' ';

        /// <summary>
        ///  Load the Unit List XML file to an XMLDocument object
        /// </summary>
        internal static void LoadListXmlDoc()
        {
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Entering {0}", CHere.FunctionName()));
            ClearReportAndPromptDict();
            ListXmlDoc = new XmlDocument();
            try
            {
                var docreader = new XmlTextReader(Setup.ListXmlFileName) { WhitespaceHandling = WhitespaceHandling.None };
                docreader.Read();
                ListXmlDoc.Load(docreader);
                docreader.Close();
            }
            catch (Exception ex)
            {
                Logs.Msg(string.Format("{0}SetUpHistory.log", Setup.LogFileRootName), ex.Message);
            }

            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Exiting  {0}", CHere.FunctionName()));

            CreateReportAndPromptDictRecords();
            ReportAndPromptDictToLog();

        }

        /// <summary>
        ///  Clear the ReportAndPromptDict SortedDictionary and List pair
        /// </summary>
        internal static void ClearReportAndPromptDict()
        {
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Entering {0}", CHere.FunctionName()));
            try
            {
                ReportAndPromptDict.Clear();
                ReportAndPromptItems.Clear();
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
            }

        }

            /// <summary>
            ///  Create the ReportAndPromptDict dictionary records to receive XML <Unit>:Report and <Unit>:Prompt information
            /// </summary>
            internal static void CreateReportAndPromptDictRecords()
        {
            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Entering {0}", CHere.FunctionName()));
            
            XmlNode xmlRoot = ListXmlDoc.DocumentElement;

            if (xmlRoot != null)
            {
                // Add NumericReport Elements to the ReportAndPromptDict SortedDictionarydictionary
                var nodeList = xmlRoot.SelectNodes("descendant::NumericReport");
                if (nodeList != null)

                {
                    foreach (XmlNode node in nodeList)
                    {
                        try
                        {
                            var unitNm = node["PIUnitName"];
                            if (unitNm == null) continue;
                            var unitProcedure = node["PIUnitProcedure"];
                            if (unitProcedure == null) continue;
                            var subBatchPath = node["PISubBatchPath"];
                            if (subBatchPath == null) continue;
                            var altPhaseNm = node["AltPhaseName"];
                            if (altPhaseNm == null) continue;
                            var paramNm = node["ParameterName"];
                            if (paramNm == null) continue;
                            var startOffset = node["StartOffset"];
                            if (startOffset == null) continue;
                            var endOffset = node["EndOffset"];
                            if (endOffset == null) continue;
                            var enumerate = node["Enumerate"];
                            if (enumerate == null) continue;
                            var ignoreZero = node["IgnoreZero"];
                            if (ignoreZero == null) continue;
                            CreateReportAndPromptDictRecord(
                                unitNm.FirstChild.InnerText + "|" +
                                    unitProcedure.FirstChild.InnerText + "|" +
                                    "1|NumericReport|" +
                                    subBatchPath.FirstChild.InnerText + "|" +
                                    altPhaseNm.FirstChild.InnerText + "|" +
                                    paramNm.FirstChild.InnerText + "|" +
                                    startOffset.FirstChild.InnerText + "|" +
                                    endOffset.FirstChild.InnerText + "|" +
                                    enumerate.FirstChild.InnerText + "|" +
                                    ignoreZero.FirstChild.InnerText,

                                    "NumericReport|" +
                                    unitNm.FirstChild.InnerText + "|" +
                                    unitProcedure.FirstChild.InnerText + "|" +
                                    subBatchPath.FirstChild.InnerText + "|" +
                                    altPhaseNm.FirstChild.InnerText + "|" +
                                    paramNm.FirstChild.InnerText + "|" +
                                    startOffset.FirstChild.InnerText + "|" +
                                    endOffset.FirstChild.InnerText + "|" +
                                    enumerate.FirstChild.InnerText + "|" +
                                    ignoreZero.FirstChild.InnerText
                                    );
                        }
                        catch (Exception ex)
                        {
                            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                        }
                    }
                }

                // Add TextReport Elements to  all ReportAndPromptDict SortedDictionarydictionary
                nodeList = xmlRoot.SelectNodes("descendant::TextReport");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        try
                        {
                            var unitNm = node["PIUnitName"];
                            if (unitNm == null) continue;
                            var unitProcedure = node["PIUnitProcedure"];
                            if (unitProcedure == null) continue;
                            var subBatchPath = node["PISubBatchPath"];
                            if (subBatchPath == null) continue;
                            var altPhaseNm = node["AltPhaseName"];
                            if (altPhaseNm == null) continue;
                            var paramNm = node["ParameterName"];
                            if (paramNm == null) continue;
                            var paramVal = node["ParameterValue"];
                            if (paramVal == null) continue;
                            var startOffset = node["StartOffset"];
                            if (startOffset == null) continue;
                            var endOffset = node["EndOffset"];
                            if (endOffset != null)
                                CreateReportAndPromptDictRecord(
                                    unitNm.FirstChild.InnerText + "|" +
                                    unitProcedure.FirstChild.InnerText + "|" +
                                    "2|TextReport|" +
                                    subBatchPath.FirstChild.InnerText + "|" +
                                    altPhaseNm.FirstChild.InnerText + "|" +
                                    paramNm.FirstChild.InnerText + "|" +
                                    paramVal.FirstChild.InnerText + "|" +
                                    startOffset.FirstChild.InnerText + "|" +
                                    endOffset.FirstChild.InnerText,

                                    "TextReport   |" +
                                    unitNm.FirstChild.InnerText + "|" +
                                    unitProcedure.FirstChild.InnerText + "|" +
                                    subBatchPath.FirstChild.InnerText + "|" +
                                    altPhaseNm.FirstChild.InnerText + "|" +
                                    paramNm.FirstChild.InnerText + "|" +
                                    paramVal.FirstChild.InnerText + "|" +
                                    startOffset.FirstChild.InnerText + "|" +
                                    endOffset.FirstChild.InnerText
                                    );
                        }
                        catch (Exception ex)
                        {
                            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                        }
                    }
                }


                // Add PromptEvent Elements to the ReportAndPromptDict SortedDictionarydictionary
                nodeList = xmlRoot.SelectNodes("descendant::PromptEvent");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        try
                        {
                            var unitNm = node["PIUnitName"];
                            if (unitNm == null) continue;
                            var unitProcedure = node["PIUnitProcedure"];
                            if (unitProcedure == null) continue;
                            var subBatchPath = node["PISubBatchPath"];
                            if (subBatchPath == null) continue;
                            var altPhaseNm = node["AltPhaseName"];
                            if (altPhaseNm == null) continue;
                            var promptTxt = node["PromptText"];
                            if (promptTxt == null) continue;
                            var getPrompt = node["GetPrompt"];
                            if (getPrompt == null) continue;
                            var getResponse = node["GetResponse"];
                            if (getResponse == null) continue;
                            var responseTxt = node["ResponseText"];
                            if (responseTxt == null) continue;
                            var startOffset = node["StartOffset"];
                            if (startOffset == null) continue;
                            var endOffset = node["EndOffset"];
                            if (endOffset != null)
                                 CreateReportAndPromptDictRecord(
                                    unitNm.FirstChild.InnerText + "|" +
                                    unitProcedure.FirstChild.InnerText + "|" +
                                    "3|PromptEvent|" +
                                    subBatchPath.FirstChild.InnerText + "|" +
                                    altPhaseNm.FirstChild.InnerText + "|" +
                                    promptTxt.FirstChild.InnerText + "|" +
                                    getPrompt.FirstChild.InnerText + "|" +
                                    getResponse.FirstChild.InnerText + "|" +
                                    responseTxt.FirstChild.InnerText + "|" +
                                    startOffset.FirstChild.InnerText + "|" +
                                    endOffset.FirstChild.InnerText,

                                    "PromptEvent  |" +
                                    unitNm.FirstChild.InnerText + "|" +
                                    unitProcedure.FirstChild.InnerText + "|" +
                                    subBatchPath.FirstChild.InnerText + "|" +
                                    altPhaseNm.FirstChild.InnerText + "|" +
                                    promptTxt.FirstChild.InnerText + "|" +
                                    getPrompt.FirstChild.InnerText + "|" +
                                    getResponse.FirstChild.InnerText + "|" +
                                    responseTxt.FirstChild.InnerText + "|" +
                                    startOffset.FirstChild.InnerText + "|" +
                                    endOffset.FirstChild.InnerText
                                    );
                        }
                        catch (Exception ex)
                        {
                            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
                        }
                    }
                }
            }

            Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("Exiting  {0}", CHere.FunctionName()));
        }

        /// <summary>
        ///  Create a new Item in ReportAndPromptDict dictionary
        /// </summary>
        private static void CreateReportAndPromptDictRecord(string keyStr, string valStr)
        {
            try
            {
                // Does the record exist in the dictionary
                if (!ReportAndPromptDict.ContainsKey(keyStr))
                {
                    Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Adding Index: ", CHere.FunctionName(), ReportAndPromptItems.Count));
                    ReportAndPromptDict.Add(keyStr, ReportAndPromptItems.Count);
                    ReportAndPromptItems.Add(valStr);
                    int i = ReportAndPromptItems.Count - 1;
                    Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Index: {1} Value: {2}", CHere.FunctionName(), ReportAndPromptItems.Count.ToString().PadLeft(4,space), valStr));
                    
                }
            }
            catch (Exception ex)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: {1}", CHere.FunctionName(), ex.Message));
            }
        }

        /// <summary>
        ///  Create a new Item in ReportAndPromptDict dictionary
        /// </summary>
        internal static void ReportAndPromptDictToLog()
        {
            foreach (int value in ReportAndPromptDict.Values)
            {
                Logs.Msg(Setup.Diagnostics, "_Diagnostics", string.Format("In {0}: Index: {1} Value: {2}", CHere.FunctionName(), value.ToString().PadLeft(4, space), ReportAndPromptItems[value]));
            }
        }
    }
}
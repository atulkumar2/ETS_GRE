using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Collections;

namespace ETS_GRE
{
    class ETSOrg
    {
        protected BooleanSwitch bTraceData;
        protected TraceSwitch bTraceApp;

        private readonly String urlIssueTaskSamplePool = "http://www.ets.org/gre/revised_general/prepare/analytical_writing/issue/pool";
        private readonly String urlArgumentTaskSamplePool = "http://www.ets.org/gre/revised_general/prepare/analytical_writing/argument/pool";
        private readonly String fnIssueTaskSamplePool = "ETSIssuetaskSamplePool";
        private readonly String fnArgumentTaskSamplePool = "ETSArgumentTaskSamplePool";
        private readonly String feIssueTaskSamplePool = "csv";
        private readonly String feArgumentTaskSamplePool = "csv";
        private readonly String feHTML = "html";
        private readonly String fnTempPrefix = "temp-";
        private readonly String fieldSeparator = " ## ";
        
        public ETSOrg()
        {
            bTraceData = Tracing.dataSwitch;
            bTraceApp = Tracing.appSwitch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SaveCSVIssueTaskSamplePool()
        {
            String DownloadedPagePath = new StringBuilder("").AppendFormat("{0}\\{1}{2}.{3}",
                    Environment.CurrentDirectory, fnTempPrefix, fnIssueTaskSamplePool, feHTML).ToString();
            String OutputFilePath = new StringBuilder("").AppendFormat("{0}\\{1}.{2}",
                    Environment.CurrentDirectory, fnIssueTaskSamplePool, feIssueTaskSamplePool).ToString();

            // Try to create files
            try
            {
                if (File.Exists(OutputFilePath))
                {
                    Console.WriteLine("Output File already exists {0}. Please delete it before rerunning.", OutputFilePath);
                    return true;
                }
                testFileCreation(OutputFilePath);
                
                if (!File.Exists(DownloadedPagePath))
                {
                    testFileCreation(DownloadedPagePath);
                }
            }
            catch (Exception e)
            {
                if (bTraceData.Enabled && bTraceApp.TraceError)
                {
                    Trace.TraceError("Failed to create file {0}. Exception {1}", OutputFilePath, e.ToString());
                }
                throw e;
            }

            // Save the downloaded page locally if it does not exist
            if (!File.Exists(DownloadedPagePath))
            {
                DownloadAndSave(urlIssueTaskSamplePool, DownloadedPagePath);
            }

            ArrayList IssueList = extractIssueList(DownloadedPagePath);
            saveArrayListToOutputFile(IssueList, OutputFilePath);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SaveCSVArgumentTaskSamplePool()
        {
            String DownloadedPagePath = new StringBuilder("").AppendFormat("{0}\\{1}{2}.{3}",
                    Environment.CurrentDirectory, fnTempPrefix, fnArgumentTaskSamplePool, feHTML).ToString();
            String OutputFilePath = new StringBuilder("").AppendFormat("{0}\\{1}.{2}",
                    Environment.CurrentDirectory, fnArgumentTaskSamplePool, feArgumentTaskSamplePool).ToString();

            // Try to create files
            try
            {
                if (File.Exists(OutputFilePath))
                {
                    Console.WriteLine("Output File already exists {0}. Please delete it before rerunning.", OutputFilePath);
                    return true;
                }
                testFileCreation(OutputFilePath);

                if (!File.Exists(DownloadedPagePath))
                {
                    testFileCreation(DownloadedPagePath);
                }
            }
            catch (Exception e)
            {
                if (bTraceData.Enabled && bTraceApp.TraceError)
                {
                    Trace.TraceError("Failed to create file {0}. Exception {1}", OutputFilePath, e.ToString());
                }
                throw e;
            }

            // Save the downloaded page locally if it does not exist
            if (!File.Exists(DownloadedPagePath))
            {
                DownloadAndSave(urlArgumentTaskSamplePool, DownloadedPagePath);
            }

            ArrayList IssueList = extractIssueList(DownloadedPagePath);
            saveArrayListToOutputFile(IssueList, OutputFilePath);

            return true;
        }

        //////////////////////////////////////////
        // Private Members
        private struct IssueListElement
        {
            public String Issue;
            public String IssueTask;
        }

        //////////////////////////////////////////
        // Private Methods
        private bool saveArrayListToOutputFile(ArrayList IssueList, String OutputFilePath)
        {
            StreamWriter OutputFile;
            try
            {
                OutputFile = new StreamWriter(OutputFilePath);
            }
            catch (Exception e)
            {
                if (bTraceData.Enabled && bTraceApp.TraceError)
                {
                    Trace.TraceError("Failed to create file {0}. Exception {1}", OutputFilePath, e.ToString());
                }
                throw e;
            }
            int Count = 0;
            foreach (IssueListElement Element in IssueList)
            {
                String Temp = new StringBuilder("").AppendFormat("{0}. {1}{2}{3}",
                            (++Count).ToString(), Element.Issue, fieldSeparator, Element.IssueTask).ToString();
                OutputFile.WriteLine(Temp + Environment.NewLine);
            }
            OutputFile.Close();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        private ArrayList extractIssueList(String FilePath)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(FilePath);
            ArrayList IssueList = new ArrayList();

            foreach (HtmlNode InputNode in htmlDoc.DocumentNode.SelectNodes("//div[@class='divider-50']"))
            {
                IssueListElement Element = new IssueListElement();
                HtmlNode TempNode = InputNode.NextSibling;
                while (TempNode.NodeType != HtmlNodeType.Element)
                {
                    TempNode = TempNode.NextSibling;
                }
                Element.Issue = TempNode.InnerText;

                HtmlNode issueNodeTask = TempNode.NextSibling.NextSibling;
                Element.IssueTask = issueNodeTask.InnerText;
                IssueList.Add(Element);
            }

            return IssueList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlString"></param>
        /// <param name="FileSavePath"></param>
        /// <returns></returns>
        private bool DownloadAndSave(string urlString, string FileSavePath)
        {
            HttpWebRequest httpReq;
            HttpWebResponse httpResponse;
            HtmlDocument htmlDoc;
            Stream receiveStream;
            StreamReader readStream;

            httpReq = (HttpWebRequest)WebRequest.Create(urlString);
            httpReq.MaximumAutomaticRedirections = 4;
            httpReq.MaximumResponseHeadersLength = 4;
            httpReq.Credentials = CredentialCache.DefaultCredentials;

            try
            {
                httpResponse = (HttpWebResponse)httpReq.GetResponse();
            }
            catch (Exception e)
            {
                if (bTraceData.Enabled && bTraceApp.TraceError)
                {
                    Trace.TraceError("Failed to fetch {0}. Exception {1}", urlString, e.ToString());
                }
                return false;
            }
            if (bTraceData.Enabled && bTraceApp.TraceVerbose)
            {
                Trace.TraceInformation("Content length is {0}", httpResponse.ContentLength);
                Trace.TraceInformation("Content type is {0}", httpResponse.ContentType);
            }

            // Process fetched content and save in temporary file
            receiveStream = httpResponse.GetResponseStream();
            htmlDoc = new HtmlDocument();

            try
            {
                readStream = new StreamReader(receiveStream, Encoding.UTF8);
                htmlDoc.LoadHtml(readStream.ReadToEnd());
                readStream.Close();
                htmlDoc.Save(FileSavePath);
            }
            catch (Exception e)
            {
                if (bTraceData.Enabled && bTraceApp.TraceError)
                {
                    Trace.TraceError("Failed to save {0}. Exception {1}", FileSavePath, e.ToString());
                }
                throw e;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        private void testFileCreation(String FilePath)
        {
            FileStream tempFile = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);
            tempFile.Close();
            File.Delete(FilePath);
        }
    }
}

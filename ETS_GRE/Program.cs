using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace ETS_GRE
{
    public class Start
    {
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (Tracing.appSwitch.TraceInfo)
            {
                Trace.TraceInformation(new String('=', 80));
                Trace.TraceInformation("Application started at {0}", DateTime.Now.ToString());
                Trace.Flush();
            }

            Start Driver = null;
            try
            {
                Driver = new Start();
                Driver.Run(args);
            }
            catch (Exception ex)
            {
                if (Tracing.appSwitch.TraceError)
                {
                    Trace.TraceError("Unhandled Exception {0}{1}", ex.ToString(), Environment.NewLine);
                }
            }
            if (Tracing.appSwitch.TraceInfo)
            {
                Trace.TraceInformation("Application exiting at {0}{1}", DateTime.Now.ToString(), Environment.NewLine);
            }
            Trace.Flush();
            return;
        }

        /// <summary>
        /// Actual Running function for entry
        /// </summary>
        /// <param name="Args"></param>
        public void Run(String[] Args)
        {
            Console.WriteLine("");
            Console.WriteLine("Output will be saved in the same directory as you are running");
            Console.WriteLine("Current directory is {0}", Environment.CurrentDirectory);
            Console.WriteLine(Environment.NewLine + "Choose what you want");

            showMenuOptions();
            Console.Write("Enter Your choice: ");
            String StrChoice = "";
            StrChoice = Console.ReadLine();

            int Choice;
            if (false == Int32.TryParse(StrChoice, out Choice))
            {
                Console.WriteLine("Enter menu choice correctly. Start program again.");
                return;
            }
            if ((Int32)QueryType.None >= Choice || (Int32)QueryType.END <= Choice)
            {
                Console.WriteLine("Enter menu choice within range. Start program again.");
                return;
            }

            String[] OldFileList = Directory.GetFiles(Environment.CurrentDirectory);
            QueryType qChoice = (QueryType)Choice;
            switch(qChoice)
            {
                case QueryType.ETSIssueTaskSamplePoolCSV:
                    new ETSOrg().SaveCSVIssueTaskSamplePool();
                    break;
                case QueryType.ETSArgumentTaskSamplePoolCSV:
                    new ETSOrg().SaveCSVArgumentTaskSamplePool();
                    break;
                case QueryType.None:
                case QueryType.ShowHelp:
                default:
                    Console.WriteLine("Rerun the program again. GoodBye.");
                    break;
            }
            String[] NewFileList = Directory.GetFiles(Environment.CurrentDirectory);
            if (NewFileList.Length > OldFileList.Length)
            {
                Console.WriteLine("We added some new files as output. Check the file system." + Environment.NewLine);
            }
            Console.WriteLine("Good Bye. We did as you asked. See you again." + Environment.NewLine);

            return;
        }

        /////////////////////////////////
        // Private methods
        private void showMenuOptions()
        {
            Console.WriteLine(" {0}. Do nothing and exit. CTL+C will also do.", (int)QueryType.None);
            Console.WriteLine(" {0}. Show Help.", (int)QueryType.ShowHelp);
            Console.WriteLine(" {0}. Save ETS Issue task pool in CSV file.", (int)QueryType.ETSIssueTaskSamplePoolCSV);
            Console.WriteLine(" {0}. Save ETS argument task pool in CSV file.", (int)QueryType.ETSArgumentTaskSamplePoolCSV);
            return;
        }

        /////////////////////////////////
        // Private members
    }

    public enum QueryType
    {
        None = 0,
        ShowHelp,
        ETSIssueTaskSamplePoolCSV,
        ETSArgumentTaskSamplePoolCSV,
        END
    }

    /// <summary>
    /// Tracing primitives
    /// </summary>
    class Tracing
    {
        public static BooleanSwitch dataSwitch = new BooleanSwitch("DataMessagesSwitch", "DataAccess module");
        public static TraceSwitch appSwitch = new TraceSwitch("TraceLevelSwitch", "Entire application");

        public Tracing()
        {
        }
    }

    public enum ErrorType
    {
        Success = 0,
        GenericFailure,
        WrongClassState,
        WrongArgumentState,
        WrongArgumentValue
    }
}
